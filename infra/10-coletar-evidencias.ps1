#Requires -RunAsAdministrator
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('E01','E02','E03','E04','E05','E06','E07','E08','E09','E10','E11','E12')][string[]]$Itens = @('E01','E02','E03','E04','E05','E06','E07','E08','E09','E10','E11','E12'),
    [ValidatePattern('^[A-Za-z0-9.-]+$')][string]$DnsName = 'api.termometro.local',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Site = 'Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$SitePath = 'C:\Sites\Termometro',
    [string]$PhotosPath = 'C:\Dados\Termometro\Fotos',
    [ValidateRange(5,1440)][int]$Minutos = 60,
    [string]$Destino
)
$ErrorActionPreference = 'Stop'
if (-not $Destino) { $Destino = Join-Path $PSScriptRoot 'evidencias' }
# So leitura: nenhum comando abaixo muda a maquina. Valores das variaveis do pool, chaves e senhas nunca entram nos arquivos.
$desde = (Get-Date).AddMinutes(-$Minutos)
$comandos = [ordered]@{
    E01 = { hostname },
          { Get-ComputerInfo | Select-Object WindowsProductName,WindowsVersion,OsBuildNumber,CsDomain },
          { ipconfig /all },
          { Get-NetIPInterface -AddressFamily IPv4 | Select-Object InterfaceAlias,Dhcp,ConnectionState }
    E02 = { nslookup $DnsName },
          { $ip = @(Resolve-DnsName $DnsName -Type A | Where-Object IPAddress)[0].IPAddress; nslookup $ip },
          { Test-NetConnection $DnsName -Port 80 },
          { Test-NetConnection $DnsName -Port 443 }
    E03 = { Get-ADDomain | Select-Object DNSRoot,NetBIOSName,DistinguishedName },
          { Get-ADUser coordenador -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf },
          { Get-ADUser admin.termometro -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf },
          { Get-ADGroupMember Coordenacao | Select-Object SamAccountName },
          { Get-ADGroupMember AdminsTermometro | Select-Object SamAccountName },
          { Get-ADGroupMember -Identity S-1-5-32-555 | Select-Object SamAccountName }
    E04 = { Get-WindowsFeature Web-Server,Web-AppInit,FS-FileServer,FS-DFS-Namespace },
          { Import-Module WebAdministration; Get-Website | Select-Object Name,Id,State,PhysicalPath,ApplicationPool },
          { Import-Module WebAdministration; Get-WebBinding -Name $Site | Select-Object protocol,bindingInformation,sslFlags },
          { Import-Module WebAdministration; Get-ChildItem IIS:\SslBindings | Select-Object Port,Host,Thumbprint },
          { Import-Module WebAdministration; Get-WebGlobalModule | Where-Object Name -eq AspNetCoreModuleV2 },
          { dotnet --list-runtimes },
          { Import-Module WebAdministration; Get-Item "IIS:\AppPools\$Pool" | Select-Object Name,State,startMode,managedRuntimeVersion,@{n='identityType';e={$_.processModel.identityType}},@{n='idleTimeout';e={$_.processModel.idleTimeout}},@{n='rapidFailProtection';e={$_.failure.rapidFailProtection}} },
          { Import-Module WebAdministration; Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/sites/site[@name='$Site']/application[@path='/']" -Name preloadEnabled | Select-Object Value },
          # Nomes sem valores: a chave da coordenacao e a connection string ficam fora da evidencia.
          { Import-Module WebAdministration; (Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/applicationPools/add[@name='$Pool']" -Name environmentVariables).Collection.name }
    E05 = { icacls $SitePath },
          { icacls $PhotosPath },
          { Get-SmbShare | Select-Object Name,Path }
    E06 = { Get-Service 'MSSQL$SQLEXPRESS' },
          { Invoke-WebRequest "https://$DnsName/" -UseBasicParsing -TimeoutSec 30 | Select-Object StatusCode },
          { Invoke-WebRequest "https://$DnsName/ambientes" -UseBasicParsing -TimeoutSec 30 | Select-Object StatusCode }
    E07 = { Get-GPO -All | Select-Object DisplayName,GpoStatus },
          { Get-ADDefaultDomainPasswordPolicy },
          { gpresult /r /scope computer },
          { auditpol /get /subcategory:* },
          { Get-Acl $PhotosPath -Audit | Select-Object -ExpandProperty Audit },
          { Get-NetFirewallRule -Name TermometroRDP,TermometroHTTP,TermometroHTTPS | ForEach-Object { $f = $_ | Get-NetFirewallAddressFilter; $p = $_ | Get-NetFirewallPortFilter; [pscustomobject]@{Name=$_.Name; Enabled=$_.Enabled; Profile=$_.Profile; LocalPort=$p.LocalPort; RemoteAddress=$f.RemoteAddress} } }
    E08 = { Get-DfsnRoot },
          { Get-DfsnFolder -Path '\\termometro.local\Escola\*' },
          { Get-DfsnFolderTarget -Path '\\termometro.local\Escola\Manuais' }
    E09 = { logman query Termometro-Alertas },
          { logman query Termometro-Recursos },
          { schtasks /query /tn Termometro-Monitoramento-Boot /v /fo LIST },
          { Get-ChildItem C:\PerfLogs\Termometro -Recurse -File | Select-Object FullName,Length,LastWriteTime },
          { Get-WinEvent -FilterHashtable @{LogName='Microsoft-Windows-Diagnosis-PLA/Operational'; Id=2031; StartTime=$desde} -MaxEvents 20 | Select-Object TimeCreated,Id,Message }
    E10 = { Import-Module WebAdministration; Get-WebAppPoolState -Name $Pool },
          { & "$env:windir\System32\inetsrv\appcmd.exe" list wp },
          { Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-WAS'; Id=5002,5009,5011; StartTime=$desde} -MaxEvents 20 | Select-Object TimeCreated,Id,Message }
    E11 = { sc.exe qfailure W3SVC },
          { sc.exe qfailureflag W3SVC },
          { Get-CimInstance Win32_Service -Filter "Name='W3SVC'" | Select-Object Name,State,ProcessId,ServiceType },
          { Get-Service WAS,W3SVC },
          { Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Service Control Manager'; Id=7031,7034,7036; StartTime=$desde} -MaxEvents 40 | Where-Object Message -match 'World Wide Web|W3SVC|7031|7034' | Select-Object TimeCreated,Id,Message }
    # Uma consulta por grupo: no DC os 4624 de maquina e servico ocupariam o MaxEvents; do 4624 so o RDP (tipo 10), como na visao-termometro.xml.
    E12 = { Get-WinEvent -LogName Security -FilterXPath "*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= $($Minutos * 60000)]] and EventData[Data[@Name='LogonType']='10']]" -MaxEvents 30 | Select-Object TimeCreated,Id,Message },
          { Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4625; StartTime=$desde} -MaxEvents 30 | Select-Object TimeCreated,Id,Message },
          { Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4656,4663; StartTime=$desde} -MaxEvents 30 | Select-Object TimeCreated,Id,Message },
          { Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IIS AspNetCore Module V2'; StartTime=$desde} -MaxEvents 20 | Select-Object TimeCreated,Id,LevelDisplayName,Message },
          # Log do IIS em UTC: comparar a hora com os eventos acima.
          { Import-Module WebAdministration; $id = (Get-Website -Name $Site).Id; Get-ChildItem "C:\inetpub\logs\LogFiles\W3SVC$id\u_ex*.log" | Sort-Object LastWriteTime | Select-Object -Last 1 | Get-Content -Tail 40 }
}
$pasta = Join-Path $Destino (Get-Date -Format 'yyyy-MM-dd')
$hora = Get-Date -Format 'HHmmss'
foreach ($item in $Itens) {
    $arquivo = Join-Path $pasta "$item-$env:COMPUTERNAME-$hora.txt"
    if (-not $PSCmdlet.ShouldProcess($arquivo, "Gravar $($comandos[$item].Count) comandos de $item")) { continue }
    $texto = @("$item coletado em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz') ($([TimeZoneInfo]::Local.Id)) por $env:USERDOMAIN\$env:USERNAME em $env:COMPUTERNAME", "Parametros: DnsName=$DnsName Site=$Site Pool=$Pool SitePath=$SitePath PhotosPath=$PhotosPath; eventos desde $($desde.ToString('yyyy-MM-dd HH:mm:ss'))", '')
    foreach ($bloco in $comandos[$item]) {
        $texto += "PS> $($bloco.ToString().Trim())"
        # Um comando que falha (modulo ausente no cliente, nenhum evento no periodo) fica registrado e nao para a coleta.
        try {
            $ErrorActionPreference = 'Continue'
            # No PowerShell 5.1 o stderr vira ErrorRecord com o caminho local do script: grava so a mensagem. Stderr de nativo (nslookup) nao e falha.
            $texto += (& $bloco 2>&1 | ForEach-Object { if ($_ -isnot [System.Management.Automation.ErrorRecord]) { $_ } elseif ($_.FullyQualifiedErrorId -like 'NativeCommandError*') { $_.Exception.Message } else { "ERRO: $($_.Exception.Message)" } } | Out-String -Width 240).TrimEnd()
        } catch {
            $texto += "ERRO: $($_.Exception.Message)"
        } finally {
            $ErrorActionPreference = 'Stop'
        }
        $texto += ''
    }
    New-Item -ItemType Directory -Path $pasta -Force | Out-Null
    Set-Content -LiteralPath $arquivo -Value $texto -Encoding UTF8
    Write-Host "$item -> $arquivo"
}
Write-Host 'Revise cada arquivo antes do commit: recorte nomes, IPs ou mensagens que nao sejam do laboratorio. E13 (fluxo completo) e prova do video.'
