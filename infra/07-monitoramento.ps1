#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$Name = 'Termometro-Alertas',
    [string]$OutputPath = 'C:\PerfLogs\Termometro',
    [Parameter(Mandatory)][string]$CpuCounter,
    [Parameter(Mandatory)][string]$DiskCounter,
    [Parameter(Mandatory)][string]$MemoryCounter,
    [ValidateRange(1,100)][int]$CpuLimit = 80,
    [ValidateRange(1,100)][int]$DiskFreeLimit = 15,
    [ValidateRange(128,65536)][int]$MemoryFreeMB = 512
)
$ErrorActionPreference = 'Stop'
& sc.exe failure W3SVC reset= 86400 actions= restart/5000/restart/5000/restart/5000
if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar recuperacao W3SVC.' }
& sc.exe failureflag W3SVC 1
if ($LASTEXITCODE -ne 0) { throw 'Falha ao configurar acoes em falhas do servico.' }
# Caminhos dos contadores variam com o idioma; escolher os tres no perfmon da VM.
foreach ($counter in $CpuCounter,$DiskCounter,$MemoryCounter) {
    Get-Counter -Counter $counter -MaxSamples 1 -ErrorAction Stop | Out-Null
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
& logman.exe query $Name *> $null
if ($LASTEXITCODE -eq 0) {
    throw 'Coletor ja existe: confira seus limites no perfmon; nenhum coletor foi sobrescrito.'
}
& logman.exe create alert $Name -o "$OutputPath\Alertas" -si '00:00:15' -th "$CpuCounter>$CpuLimit" "$DiskCounter<$DiskFreeLimit" "$MemoryCounter<$MemoryFreeMB" -el
if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar coletor de alertas.' }
& logman.exe start $Name
if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar coletor de alertas.' }
Write-Host 'No perfmon, conferir alertas e configurar inicio agendado ao reiniciar. Fazer o ensaio de limite descrito no README.'
