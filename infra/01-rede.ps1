# Executar no console da VM: a troca de IP interrompe sessoes remotas.
#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InterfaceAlias,
    [Parameter(Mandatory)][ipaddress]$IPAddress,
    [ValidateRange(1,32)][int]$PrefixLength = 24,
    [Parameter(Mandatory)][ipaddress]$Gateway,
    [ValidatePattern('^[A-Za-z][A-Za-z0-9-]{0,14}$')][string]$Hostname = 'SRV-TERMOMETRO'
)
$ErrorActionPreference = 'Stop'
if ($IPAddress.AddressFamily -ne 'InterNetwork' -or $Gateway.AddressFamily -ne 'InterNetwork') {
    throw 'Informe endereco e gateway IPv4 da rede isolada da VM.'
}
$adapter = Get-NetAdapter -Name $InterfaceAlias
if ($adapter.Status -ne 'Up') { throw 'Conecte a interface antes de configurar.' }
$current = @(Get-NetIPAddress -InterfaceIndex $adapter.ifIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue)
if ($current | Where-Object { $_.PrefixOrigin -ne 'Dhcp' -and $_.IPAddress -ne $IPAddress.IPAddressToString -and $_.IPAddress -notlike '169.254.*' }) {
    throw 'Ha outro IP fixo. Revise no console antes de remover qualquer endereco.'
}
$routes = @(Get-NetRoute -InterfaceIndex $adapter.ifIndex -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue)
if ($routes | Where-Object { $_.NextHop -ne $Gateway.IPAddressToString -and $_.Protocol -ne 'Dhcp' }) { throw 'Gateway fixo existente diferente; revise no console.' }
Set-NetIPInterface -InterfaceIndex $adapter.ifIndex -AddressFamily IPv4 -Dhcp Disabled
if (-not ($current | Where-Object IPAddress -eq $IPAddress.IPAddressToString)) {
    New-NetIPAddress -InterfaceIndex $adapter.ifIndex -IPAddress $IPAddress.IPAddressToString -PrefixLength $PrefixLength | Out-Null
} else {
    Set-NetIPAddress -InterfaceIndex $adapter.ifIndex -IPAddress $IPAddress.IPAddressToString -PrefixLength $PrefixLength
}
$routes = @(Get-NetRoute -InterfaceIndex $adapter.ifIndex -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue)
if ($routes | Where-Object NextHop -ne $Gateway.IPAddressToString) { throw 'Gateway DHCP antigo persistiu; revise no console antes de continuar.' }
if (-not $routes) { New-NetRoute -InterfaceIndex $adapter.ifIndex -DestinationPrefix '0.0.0.0/0' -NextHop $Gateway.IPAddressToString | Out-Null }
# O futuro controlador de dominio usa o proprio DNS.
Set-DnsClientServerAddress -InterfaceIndex $adapter.ifIndex -ServerAddresses $IPAddress.IPAddressToString
if ($env:COMPUTERNAME -ne $Hostname) { Rename-Computer -NewName $Hostname }
Write-Host 'Confira ipconfig /all. Reinicie manualmente antes do 02.'
