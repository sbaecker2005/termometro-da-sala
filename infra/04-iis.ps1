#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Site = 'Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$SitePath = 'C:\Sites\Termometro',
    [string]$PhotosPath = 'C:\Dados\Termometro\Fotos',
    [string]$DnsName = 'api.termometro.local'
)
$ErrorActionPreference = 'Stop'
$siteFolder = [IO.Path]::GetFullPath($SitePath).TrimEnd('\')
$photosFolder = [IO.Path]::GetFullPath($PhotosPath).TrimEnd('\')
if ($siteFolder -eq $photosFolder -or $photosFolder.StartsWith("$siteFolder\",[StringComparison]::OrdinalIgnoreCase) -or $siteFolder.StartsWith("$photosFolder\",[StringComparison]::OrdinalIgnoreCase)) {
    throw 'Site e fotos devem ser pastas separadas, sem sobreposicao.'
}
Install-WindowsFeature Web-Server,Web-Mgmt-Console,Web-AppInit,Web-Http-Logging,Web-Request-Monitor,FS-FileServer -IncludeManagementTools | Out-Null
Import-Module WebAdministration
foreach ($path in $SitePath,$PhotosPath) { New-Item -ItemType Directory -Path $path -Force | Out-Null }
if (-not (Test-Path "IIS:\AppPools\$Pool")) { New-WebAppPool -Name $Pool | Out-Null }
Set-ItemProperty "IIS:\AppPools\$Pool" -Name managedRuntimeVersion -Value ''
Set-ItemProperty "IIS:\AppPools\$Pool" -Name processModel.identityType -Value 4
Set-ItemProperty "IIS:\AppPools\$Pool" -Name startMode -Value AlwaysRunning
Set-ItemProperty "IIS:\AppPools\$Pool" -Name processModel.idleTimeout -Value ([timespan]::Zero)
Set-ItemProperty "IIS:\AppPools\$Pool" -Name recycling.logEventOnRecycle -Value 'Time,Requests,Schedule,Memory,IsapiUnhealthy,OnDemand,ConfigChange,PrivateMemory'
# A protecao contra falhas repetidas continua habilitada.
Set-ItemProperty "IIS:\AppPools\$Pool" -Name failure.rapidFailProtection -Value $true
if (-not (Test-Path "IIS:\Sites\$Site")) {
    New-Website -Name $Site -PhysicalPath $SitePath -ApplicationPool $Pool -Port 80 -HostHeader $DnsName | Out-Null
} else {
    if ((Get-Item "IIS:\Sites\$Site").PhysicalPath -ne $SitePath) { throw 'Site existente usa outra pasta.' }
    Set-ItemProperty "IIS:\Sites\$Site" -Name applicationPool -Value $Pool
}
Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter "system.applicationHost/sites/site[@name='$Site']/application[@path='/']" -Name preloadEnabled -Value $true
Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Location $Site -Filter 'system.webServer/applicationInitialization' -Name doAppInitAfterRestart -Value $true
Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Location $Site -Filter 'system.webServer/security/authentication/anonymousAuthentication' -Name userName -Value ''
# ACL explicita: administracao e recuperacao continuam possiveis por SYSTEM e Administrators.
foreach ($entry in @(@($SitePath,'ReadAndExecute'),@($PhotosPath,'Modify'))) {
    $acl = Get-Acl -LiteralPath $entry[0]
    $acl.SetAccessRuleProtection($true,$false)
    foreach ($rule in @($acl.Access)) { [void]$acl.RemoveAccessRuleSpecific($rule) }
    foreach ($sid in 'S-1-5-18','S-1-5-32-544') {
        $acl.AddAccessRule([System.Security.AccessControl.FileSystemAccessRule]::new([System.Security.Principal.SecurityIdentifier]::new($sid),'FullControl','ContainerInherit,ObjectInherit','None','Allow'))
    }
    $acl.AddAccessRule([System.Security.AccessControl.FileSystemAccessRule]::new("IIS AppPool\$Pool",$entry[1],'ContainerInherit,ObjectInherit','None','Allow'))
    Set-Acl -LiteralPath $entry[0] -AclObject $acl
}
if (-not (Get-NetFirewallRule -Name TermometroHTTP -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -Name TermometroHTTP -DisplayName 'Termometro HTTP laboratorio' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 80 -Profile Domain,Private -RemoteAddress LocalSubnet | Out-Null
}
Write-Host 'Instale manualmente o Hosting Bundle .NET 8 x64 depois do IIS e reinicie a VM. Fotos ficam fora do site; nao sao compartilhadas por SMB.'
