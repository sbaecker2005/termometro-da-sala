#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$Coordenador = 'coordenador',
    [string]$Administrador = 'admin.termometro'
)
$ErrorActionPreference = 'Stop'
Import-Module ActiveDirectory
$domain = Get-ADDomain
$ou = "OU=Termometro,$($domain.DistinguishedName)"
if (-not (Get-ADOrganizationalUnit -Filter 'Name -eq "Termometro"' -SearchBase $domain.DistinguishedName -SearchScope OneLevel)) {
    New-ADOrganizationalUnit -Name Termometro -Path $domain.DistinguishedName
}
foreach ($name in 'Coordenacao','AdminsTermometro') {
    if (-not (Get-ADGroup -Filter "SamAccountName -eq '$name'")) {
        New-ADGroup -Name $name -SamAccountName $name -GroupScope Global -GroupCategory Security -Path $ou
    }
}
foreach ($name in @($Coordenador,$Administrador)) {
    if ($name -notmatch '^[a-zA-Z][a-zA-Z0-9.\-]{0,19}$') { throw 'Nome de usuario invalido.' }
    if (-not (Get-ADUser -Filter "SamAccountName -eq '$name'")) {
        $password = Read-Host "Senha inicial de $name (nao registrar)" -AsSecureString
        New-ADUser -Name $name -SamAccountName $name -UserPrincipalName "$name@$($domain.DNSRoot)" -Path $ou -AccountPassword $password -Enabled $true -ChangePasswordAtLogon $true
    }
}
Add-ADGroupMember -Identity Coordenacao -Members $Coordenador
Add-ADGroupMember -Identity AdminsTermometro -Members $Administrador
Write-Host 'AdminsTermometro autoriza RDP; nao concede administracao do dominio. Use conta administrativa separada para instalar.'
