#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$DomainName = 'termometro.local',
    [Parameter(Mandatory)][ipaddress]$ServerIP
)
$ErrorActionPreference = 'Stop'
Install-WindowsFeature AD-Domain-Services,DNS,FS-FileServer,FS-DFS-Namespace,GPMC -IncludeManagementTools | Out-Null
$computer = Get-CimInstance Win32_ComputerSystem
if ($computer.DomainRole -lt 4) {
    if ($computer.PartOfDomain) { throw 'Servidor ja pertence a um dominio; nao criar outra floresta.' }
    $dsrm = Read-Host 'Senha DSRM (nao registrar)' -AsSecureString
    Install-ADDSForest -DomainName $DomainName -DomainNetbiosName 'TERMOMETRO' -InstallDns -SafeModeAdministratorPassword $dsrm -NoRebootOnCompletion -Force
    Write-Host 'Reinicie e execute o 02 novamente para criar o registro DNS.'
    return
}
Import-Module ActiveDirectory
if ((Get-ADDomain).DNSRoot -ne $DomainName) { throw 'Dominio existente diferente do solicitado.' }
$record = @(Get-DnsServerResourceRecord -ZoneName $DomainName -Name 'api' -RRType A -ErrorAction SilentlyContinue)
if ($record -and ($record.Count -ne 1 -or $record[0].RecordData.IPv4Address.IPAddressToString -ne $ServerIP.IPAddressToString)) {
    throw 'Registro api existente diverge. Revise antes de alterar.'
}
if (-not $record) { Add-DnsServerResourceRecordA -ZoneName $DomainName -Name 'api' -IPv4Address $ServerIP }
