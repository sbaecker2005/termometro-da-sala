#Requires -RunAsAdministrator
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Name = 'Termometro-Alertas',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$HistoryName = 'Termometro-Recursos',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$BootTask = 'Termometro-Monitoramento-Boot',
    [string]$OutputPath = 'C:\PerfLogs\Termometro',
    [Parameter(Mandatory)][string]$CpuCounter,
    [Parameter(Mandatory)][string]$DiskCounter,
    [Parameter(Mandatory)][string]$MemoryCounter,
    [ValidateRange(1,100)][int]$CpuLimit = 80,
    [ValidateRange(1,100)][int]$DiskFreeLimit = 15,
    [ValidateRange(128,65536)][int]$MemoryFreeMB = 512
)
$ErrorActionPreference = 'Stop'
# Caminhos dos contadores variam com o idioma; escolher os tres no perfmon da VM.
foreach ($counter in $CpuCounter,$DiskCounter,$MemoryCounter) {
    Get-Counter -Counter $counter -MaxSamples 1 -ErrorAction Stop | Out-Null
}
& logman.exe query $Name *> $null
if ($LASTEXITCODE -eq 0) {
    throw 'Coletor ja existe: confira seus limites no perfmon; nenhum coletor foi sobrescrito.'
}
& logman.exe query $HistoryName *> $null
$historyExists = $LASTEXITCODE -eq 0
$taskExists = [bool](Get-ScheduledTask -TaskName $BootTask -ErrorAction SilentlyContinue)
if ($PSCmdlet.ShouldProcess('W3SVC', 'Recuperacao: 3 reinicios com 5 s, contador zerado em 1 dia')) {
    & sc.exe failure W3SVC reset= 86400 actions= restart/5000/restart/5000/restart/5000
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar recuperacao W3SVC.' }
    & sc.exe failureflag W3SVC 1
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar acoes em falhas do servico.' }
}
# Historico em BLG circular: 100 MB no maximo, o disco do servidor nao enche com amostras.
if ($historyExists) {
    Write-Host "$HistoryName ja existe e foi mantido como esta."
} elseif ($PSCmdlet.ShouldProcess($HistoryName, "Criar coletor de historico BLG a cada 15 s em $OutputPath\Recursos")) {
    New-Item -ItemType Directory -Path "$OutputPath\Recursos" -Force | Out-Null
    & logman.exe create counter $HistoryName -c $CpuCounter $DiskCounter $MemoryCounter -si '00:00:15' -f bincirc -max 100 -o "$OutputPath\Recursos\$HistoryName"
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar coletor de historico.' }
}
# -rdcs: o disparo do alerta religa o historico, se alguem o tiver parado.
if ($PSCmdlet.ShouldProcess($Name, "Criar alerta CPU>$CpuLimit, disco livre<$DiskFreeLimit, memoria<$MemoryFreeMB MB a cada 15 s")) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    & logman.exe create alert $Name -o "$OutputPath\Alertas" -si '00:00:15' -th "$CpuCounter>$CpuLimit" "$DiskCounter<$DiskFreeLimit" "$MemoryCounter<$MemoryFreeMB" -el -rdcs $HistoryName
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar coletor de alertas.' }
}
if ($taskExists) {
    Write-Host "Tarefa $BootTask ja existe e foi mantida como esta."
} elseif ($PSCmdlet.ShouldProcess($BootTask, "Tarefa no boot como SYSTEM: iniciar $Name e $HistoryName")) {
    # cmd com & inicia o segundo coletor mesmo que o primeiro ja esteja rodando.
    $action = New-ScheduledTaskAction -Execute "$env:windir\System32\cmd.exe" -Argument "/c logman.exe start $Name & logman.exe start $HistoryName"
    $trigger = New-ScheduledTaskTrigger -AtStartup
    $trigger.Delay = 'PT1M'
    $principal = New-ScheduledTaskPrincipal -UserId 'NT AUTHORITY\SYSTEM' -LogonType ServiceAccount -RunLevel Highest
    Register-ScheduledTask -TaskName $BootTask -Action $action -Trigger $trigger -Principal $principal -Description 'Inicia os coletores do Termometro depois do boot.' | Out-Null
}
foreach ($set in $Name,$HistoryName) {
    if ($PSCmdlet.ShouldProcess($set, 'Iniciar coletor')) {
        & logman.exe start $set
        # 0x803000AA = PLA_E_DCS_ALREADY_STARTED: historico mantido ja rodando nao e falha.
        if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne [int]0x803000AA) { throw "Falha ao iniciar $set." }
    }
}
Write-Host 'Conferir no perfmon os limites e o historico; reiniciar a VM uma vez para provar que os dois coletores voltam sozinhos. Fazer o ensaio de limite descrito no README.'
