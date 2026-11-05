#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Pool','Servico')][string]$Parte,
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [Parameter(Mandatory)][uri]$ProbeUri,
    [switch]$LaboratorioConfirmado
)
$ErrorActionPreference = 'Stop'
if (-not $LaboratorioConfirmado) { throw 'Leia TESTE-DE-FALHA.md e use somente na VM isolada com snapshot.' }
Import-Module WebAdministration
if ((Invoke-WebRequest -Uri $ProbeUri -UseBasicParsing -TimeoutSec 15).StatusCode -ne 200) { throw 'Aplicacao precisa funcionar antes da falha.' }
if ($Parte -eq 'Pool') {
    $workers = @(Get-CimInstance Win32_Process -Filter "Name='w3wp.exe'" | Where-Object { $_.CommandLine -match ('-ap\s+"' + [regex]::Escape($Pool) + '"') })
    if ($workers.Count -ne 1) { throw 'Esperado um unico worker do pool; confira appcmd list wp.' }
    $oldId = $workers[0].ProcessId
    Stop-Process -Id $oldId -Force
} else {
    $service = Get-CimInstance Win32_Service -Filter "Name='W3SVC'"
    if ($service.State -ne 'Running' -or $service.ProcessId -eq 0) { throw 'W3SVC nao esta executando.' }
    $shared = @(Get-CimInstance Win32_Service | Where-Object { $_.ProcessId -eq $service.ProcessId -and $_.State -eq 'Running' })
    if ($shared.Count -ne 1) { throw 'W3SVC compartilha processo. Isole conforme TESTE-DE-FALHA.md e reinicie antes do teste.' }
    $oldId = $service.ProcessId
    Stop-Process -Id $oldId -Force
}
$deadline = (Get-Date).AddSeconds(90)
do {
    Start-Sleep -Seconds 2
    if ($Parte -eq 'Pool') {
        $newId = @(Get-CimInstance Win32_Process -Filter "Name='w3wp.exe'" | Where-Object { $_.CommandLine -match ('-ap\s+"' + [regex]::Escape($Pool) + '"') -and $_.ProcessId -ne $oldId } | Select-Object -ExpandProperty ProcessId)
        $recovered = $newId.Count -gt 0
    } else {
        $now = Get-CimInstance Win32_Service -Filter "Name='W3SVC'"
        $newId = $now.ProcessId
        $recovered = $now.State -eq 'Running' -and $newId -ne 0 -and $newId -ne $oldId
    }
} until ($recovered -or (Get-Date) -ge $deadline)
# A primeira verificacao de PID ocorre SEM requisicao para provar a recuperacao automatica.
if (-not $recovered) { throw 'Nao recuperou automaticamente em 90 s. Guardar eventos antes de intervir.' }
Write-Host "PID anterior: $oldId; novo: $newId"
$response = Invoke-WebRequest -Uri $ProbeUri -UseBasicParsing -TimeoutSec 30
if ($response.StatusCode -ne 200) { throw 'Processo voltou, mas a aplicacao nao respondeu HTTP 200.' }
Get-WebAppPoolState -Name $Pool
Get-Service W3SVC,WAS
