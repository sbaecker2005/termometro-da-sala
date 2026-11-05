#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PublishPath,
    [string]$SitePath = 'C:\Sites\Termometro',
    [string]$BackupRoot = 'C:\Backups\Termometro',
    [string]$PhotosPath = 'C:\Dados\Termometro\Fotos',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$AssemblyName = 'TermometroDaSala.Api',
    [Parameter(Mandatory)][uri]$ProbeUri,
    [switch]$MqttDesligado
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
if (-not (Test-Path -LiteralPath "$source\wwwroot\index.html" -PathType Leaf)) { throw 'Painel wwwroot/index.html ausente.' }
$photosFolder = [IO.Path]::GetFullPath($PhotosPath).TrimEnd('\')
if (-not (Test-Path -LiteralPath $photosFolder -PathType Container)) { throw "Pasta de fotos ausente: $photosFolder. Rode o 04." }
function Get-PoolVar([string]$Name) {
    try {
        $vars = @(Get-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter "system.applicationHost/applicationPools/add[@name='$Pool']" -Name environmentVariables -ErrorAction Stop).Collection
        $found = @($vars | Where-Object { $_.name -eq $Name })
        if (-not $found) { return '' }
        return [string]$found[0].value
    } catch { return '' }
}
# shortcut: o cmdlet do IIS nao foi ensaiado aqui, confira os nomes no pool se a variavel nao gravar.
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
$alvos = @($ProbeUri.AbsoluteUri)
$raiz = '{0}://{1}/' -f $ProbeUri.Scheme, $ProbeUri.Authority
if ($raiz -ne $ProbeUri.AbsoluteUri) { $alvos += $raiz }
foreach ($uri in $alvos) {
    $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -ne 200) { throw "Publicacao nao respondeu HTTP 200 em $uri; revisar logs e backup." }
}
Write-Host "HTTP 200 em $($alvos -join ', '). Backup: $backup. Copia preserva arquivos antigos; confira o conteudo publicado."
