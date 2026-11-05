#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PublishPath,
    [string]$SitePath = 'C:\Sites\Termometro',
    [string]$BackupRoot = 'C:\Backups\Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$AssemblyName = 'TermometroDaSala.Api',
    [Parameter(Mandatory)][uri]$ProbeUri
)
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration
$source = (Resolve-Path -LiteralPath $PublishPath).Path.TrimEnd('\')
$target = [IO.Path]::GetFullPath($SitePath).TrimEnd('\')
$backups = [IO.Path]::GetFullPath($BackupRoot).TrimEnd('\')
if ($source -eq $target -or $source.StartsWith("$target\",[StringComparison]::OrdinalIgnoreCase) -or $target.StartsWith("$source\",[StringComparison]::OrdinalIgnoreCase) -or $backups -eq $target -or $backups.StartsWith("$target\",[StringComparison]::OrdinalIgnoreCase)) {
    throw 'Origem, destino e backup devem ser pastas separadas.'
}
foreach ($file in 'web.config',"$AssemblyName.dll") {
    if (-not (Test-Path -LiteralPath "$source\$file" -PathType Leaf)) { throw "Publicacao incompleta: $file ausente." }
}
if (-not (Get-WebGlobalModule | Where-Object Name -eq 'AspNetCoreModuleV2')) { throw 'Hosting Bundle ausente. Instale depois do IIS.' }
if (-not (Test-Path -LiteralPath "$source\wwwroot\index.html")) { Write-Warning 'Painel wwwroot/index.html ausente: isto ainda nao comprova a proposta completa.' }
$backup = Join-Path $backups ([guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $backup -Force | Out-Null
$wasRunning = (Get-WebAppPoolState -Name $Pool).Value -eq 'Started'
if ($wasRunning) { Stop-WebAppPool -Name $Pool }
try {
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-WebAppPoolState -Name $Pool).Value -ne 'Stopped') {
        if ((Get-Date) -gt $deadline) { throw 'Pool nao parou; nenhum arquivo foi copiado.' }
        Start-Sleep -Seconds 1
    }
    Get-ChildItem -LiteralPath $target -Force | Copy-Item -Destination $backup -Recurse -Force
    # Sem espelhamento: nao apaga arquivos nem dados locais.
    Get-ChildItem -LiteralPath $source -Force | Copy-Item -Destination $target -Recurse -Force
} finally {
    if ($wasRunning) { Start-WebAppPool -Name $Pool }
}
if (-not $wasRunning) { Start-WebAppPool -Name $Pool }
$response = Invoke-WebRequest -Uri $ProbeUri -UseBasicParsing -TimeoutSec 30
if ($response.StatusCode -ne 200) { throw 'Publicacao nao respondeu HTTP 200; revisar logs e backup.' }
Write-Host "Publicacao respondeu HTTP 200. Backup: $backup. Copia preserva arquivos antigos; confira o conteudo publicado."
