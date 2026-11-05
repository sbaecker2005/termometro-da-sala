#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Site = 'Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$SitePath = 'C:\Sites\Termometro',
    [string]$PhotosPath = 'C:\Dados\Termometro\Fotos',
    [string]$DnsName = 'api.termometro.local',
    [switch]$MqttDesligado
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
function Get-PoolVar([string]$Name) {
    try {
        $vars = @(Get-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter "system.applicationHost/applicationPools/add[@name='$Pool']" -Name environmentVariables -ErrorAction Stop).Collection
        $found = @($vars | Where-Object { $_.name -eq $Name })
        if (-not $found) { return '' }
        return [string]$found[0].value
    } catch { return '' }
}
# Cria a variavel no pool ou troca o valor se ela ja existir; os nomes sao conferidos no passo 4 do README.
function Set-PoolVar([string]$Name, [string]$Value) {
    $poolFilter = "system.applicationHost/applicationPools/add[@name='$Pool']"
    $item = "$poolFilter/environmentVariables/add[@name='$Name']"
    $node = $null
    try { $node = Get-WebConfiguration -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter $item -ErrorAction Stop } catch { $node = $null }
    if ($node) {
        Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter $item -Name value -Value $Value
    } else {
        Add-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter $poolFilter -Name environmentVariables -Value @{name=$Name; value=$Value} | Out-Null
    }
}
# No pool, nao no web.config: a publicacao nao apaga a chave e ela nao entra no git.
$chave = $env:ChaveCoordenacao
if ([string]::IsNullOrWhiteSpace($chave)) { $chave = Get-PoolVar 'ChaveCoordenacao' }
if ([string]::IsNullOrWhiteSpace($chave)) {
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) { throw 'Defina ChaveCoordenacao na sessao. O valor nao entra no git.' }
    $secure = Read-Host -AsSecureString -Prompt 'ChaveCoordenacao'
    if ($secure.Length -lt 1) { throw 'ChaveCoordenacao vazia.' }
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { $chave = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}
$sala = $env:ConnectionStrings__Sala
if ([string]::IsNullOrWhiteSpace($sala)) { $sala = 'Server=.\SQLEXPRESS;Database=Termometro;Integrated Security=True;TrustServerCertificate=True' }
Set-PoolVar 'ConnectionStrings__Sala' $sala
Set-PoolVar 'Fotos__Pasta' $photosFolder
Set-PoolVar 'ChaveCoordenacao' $chave
Set-PoolVar 'ASPNETCORE_ENVIRONMENT' 'Production'
Set-PoolVar 'ASPNETCORE_HTTPS_PORT' '443'
Set-PoolVar 'Banco__Migrar' 'false'
if ($MqttDesligado -or $env:Mqtt__Desligado -eq 'true') { Set-PoolVar 'Mqtt__Desligado' 'true' }
elseif ($env:Mqtt__Desligado -eq 'false') { Set-PoolVar 'Mqtt__Desligado' 'false' }
if ((Get-WebAppPoolState -Name $Pool).Value -eq 'Started') { Restart-WebAppPool -Name $Pool }
Write-Host 'Pool: ConnectionStrings__Sala, Fotos__Pasta, ChaveCoordenacao, ASPNETCORE_ENVIRONMENT=Production, ASPNETCORE_HTTPS_PORT=443, Banco__Migrar=false. MQTT ligado exige NAT de saida na 1883; senao use -MqttDesligado. Instale o Hosting Bundle .NET 8 x64 depois do IIS e reinicie a VM. Fotos ficam fora do site; nao sao compartilhadas por SMB.'
