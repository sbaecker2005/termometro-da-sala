#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$GpoName = 'Termometro-Seguranca',
    [string]$PhotosPath = 'C:\Dados\Termometro\Fotos'
)
$ErrorActionPreference = 'Stop'
Import-Module ActiveDirectory
Import-Module GroupPolicy
$domain = Get-ADDomain
Set-ADDefaultDomainPasswordPolicy -Identity $domain.DNSRoot -MinPasswordLength 12 -ComplexityEnabled $true -PasswordHistoryCount 24 -MaxPasswordAge ([timespan]::FromDays(90)) -MinPasswordAge ([timespan]::FromDays(1)) -LockoutThreshold 5 -LockoutDuration ([timespan]::FromMinutes(15)) -LockoutObservationWindow ([timespan]::FromMinutes(15))
$gpo = Get-GPO -Name $GpoName -ErrorAction SilentlyContinue
if (-not $gpo) { $gpo = New-GPO -Name $GpoName }
if (-not ((Get-GPInheritance -Target $domain.DistinguishedName).GpoLinks | Where-Object DisplayName -eq $GpoName)) {
    New-GPLink -Guid $gpo.Id -Target $domain.DistinguishedName | Out-Null
}
$desktop = 'HKCU\Software\Policies\Microsoft\Windows\Control Panel\Desktop'
foreach ($setting in @(@('ScreenSaveActive','1'),@('ScreenSaverIsSecure','1'),@('ScreenSaveTimeOut','600'),@('SCRNSAVE.EXE','scrnsave.scr'))) {
    Set-GPRegistryValue -Guid $gpo.Id -Key $desktop -ValueName $setting[0] -Type String -Value $setting[1] | Out-Null
}
Set-GPRegistryValue -Guid $gpo.Id -Key 'HKLM\Software\Policies\Microsoft\Windows NT\Terminal Services' -ValueName fDenyTSConnections -Type DWord -Value 0 | Out-Null
Set-GPRegistryValue -Guid $gpo.Id -Key 'HKLM\Software\Policies\Microsoft\Windows NT\Terminal Services' -ValueName UserAuthentication -Type DWord -Value 1 | Out-Null
Set-GPRegistryValue -Guid $gpo.Id -Key 'HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\System' -ValueName InactivityTimeoutSecs -Type DWord -Value 600 | Out-Null
Set-GPRegistryValue -Guid $gpo.Id -Key 'HKLM\SYSTEM\CurrentControlSet\Control\Lsa' -ValueName SCENoApplyLegacyAuditPolicy -Type DWord -Value 1 | Out-Null
# GUIDs independem do idioma: Logon e File System. Persistir tambem na GPO pelo roteiro.
foreach ($subcategory in '{0CCE9215-69AE-11D9-BED3-505054503030}','{0CCE921D-69AE-11D9-BED3-505054503030}') {
    & auditpol.exe /set "/subcategory:$subcategory" /success:enable /failure:enable
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao habilitar auditoria.' }
}
# Modify inclui ReadData: leitura da foto pela API tambem gera 4663, alem de gravar e apagar.
$acl = Get-Acl -LiteralPath $PhotosPath -Audit
$rule = [System.Security.AccessControl.FileSystemAuditRule]::new([System.Security.Principal.SecurityIdentifier]::new('S-1-1-0'),'Modify','ContainerInherit,ObjectInherit','None','Success,Failure')
$acl.SetAuditRule($rule)
Set-Acl -LiteralPath $PhotosPath -AclObject $acl
Write-Host 'OBRIGATORIO: concluir direitos RDP, auditoria avancada e escopo do firewall na GPMC conforme README antes de habilitar acesso remoto.'
