#requires -Version 5.1
# Verifica o SDK .NET 8 no Windows e instala somente quando necessario.
$ErrorActionPreference = 'Stop'

if ($env:OS -ne 'Windows_NT') {
    throw 'Este instalador e para o computador Windows. No cloud Linux, use o SDK ja preparado.'
}

function Localizar-Dotnet {
    $comando = Get-Command dotnet.exe -ErrorAction SilentlyContinue
    if ($comando) { return $comando.Source }
    $candidato = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $candidato) { return $candidato }
    return $null
}

function Testar-Sdk8([string]$Executavel) {
    if (!$Executavel) { return $false }
    $versoes = @(& $Executavel --list-sdks 2>&1)
    if ($LASTEXITCODE -ne 0) { return $false }
    return @($versoes | Where-Object { $_ -match '^8\.0\.' }).Count -gt 0
}

$dotnet = Localizar-Dotnet
if (!(Testar-Sdk8 $dotnet)) {
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (!$winget) {
        throw 'SDK .NET 8 ausente e winget indisponivel. Instale o SDK Windows em https://dotnet.microsoft.com/download/dotnet/8.0 e execute novamente.'
    }
    Write-Host 'SDK .NET 8 ausente. Instalando o pacote oficial Microsoft.DotNet.SDK.8...'
    & $winget.Source install --id Microsoft.DotNet.SDK.8 --exact --source winget --accept-source-agreements --accept-package-agreements --disable-interactivity
    $codigoInstalacao = $LASTEXITCODE
    $dotnet = Localizar-Dotnet
    if (!(Testar-Sdk8 $dotnet)) {
        throw "Instalacao nao confirmada (codigo $codigoInstalacao). Confira o instalador, reinicie o terminal e execute novamente."
    }
} else {
    Write-Host 'SDK .NET 8 ja instalado; nenhuma reinstalacao foi feita.'
}

$runtimes = @(& $dotnet --list-runtimes 2>&1)
if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel conferir os runtimes do .NET.' }
if (!@($runtimes | Where-Object { $_ -match '^Microsoft\.WindowsDesktop\.App 8\.' }).Count) {
    throw 'SDK encontrado, mas Windows Desktop Runtime 8 nao foi confirmado. Repare o SDK pelo instalador oficial antes de abrir o Windows Forms.'
}

Write-Host ''
Write-Host 'SDKs instalados:'
& $dotnet --list-sdks
if ($LASTEXITCODE -ne 0) { throw 'Falha na verificacao final do SDK.' }
Write-Host 'Windows Forms Runtime 8 confirmado. SDK pronto para API, testes e desktop.'
Write-Host 'O SQL Server/LocalDB e um requisito separado para executar a API com banco real.'
