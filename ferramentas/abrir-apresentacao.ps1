#requires -Version 5.1
$ErrorActionPreference = 'Stop'
$raiz = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projeto = Join-Path $raiz 'api/TermometroDaSala.Api'
$desktop = Join-Path $raiz 'executavel-desktop/TermometroDaSala.Desktop.exe'
# Sem o executavel do kit (caso do repositorio clonado), o desktop compila e roda a partir do codigo.
$desktopFonte = Join-Path $raiz 'desktop/TermometroDaSala.Desktop'
$reservaPorta = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 5080)
try {
    $reservaPorta.ExclusiveAddressUse = $true
    $reservaPorta.Start()
} catch {
    throw 'A porta 5080 esta em uso ou indisponivel. Encerre a API do ensaio anterior antes de abrir uma nova apresentacao.'
} finally { $reservaPorta.Stop() }
$banco = 'TermometroGS_Apresentacao_' + (Get-Date -Format 'yyyyMMdd_HHmmss')
$saida = Join-Path $raiz ('output/apresentacao/' + $banco)
New-Item -ItemType Directory -Force -Path $saida | Out-Null
& dotnet build $projeto -c Apresentacao --nologo -v minimal -m:1 /nodeReuse:false
if ($LASTEXITCODE) { throw 'Compilacao da API falhou.' }
$bytes = New-Object byte[] 32
$gerador = [Security.Cryptography.RandomNumberGenerator]::Create()
try { $gerador.GetBytes($bytes) } finally { $gerador.Dispose() }
$chave = [Convert]::ToBase64String($bytes)
$variaveis = @{
    ConnectionStrings__Sala="Server=.\SQLEXPRESS;Database=$banco;Integrated Security=True;TrustServerCertificate=True"
    ChaveCoordenacao=$chave; TERMOMETRO_CHAVE_COORDENACAO=$chave
    DOTNET_ENVIRONMENT='Demo'; ASPNETCORE_ENVIRONMENT='Demo'
    Banco__Migrar='true'; Mqtt__Desligado='true'
    Fotos__Pasta=(Join-Path $saida 'fotos')
    Logging__LogLevel__Default='Warning'; Logging__EventLog__LogLevel__Default='None'
}
$anteriores = @{}
$api = $null
try {
    foreach ($nome in $variaveis.Keys) {
        $anteriores[$nome] = [Environment]::GetEnvironmentVariable($nome,'Process')
        [Environment]::SetEnvironmentVariable($nome,$variaveis[$nome],'Process')
    }
    $api = Start-Process -FilePath (Get-Command dotnet).Source -WorkingDirectory $projeto -WindowStyle Hidden -PassThru `
        -ArgumentList 'bin/Apresentacao/net8.0/TermometroDaSala.Api.dll --urls http://localhost:5080' `
        -RedirectStandardOutput (Join-Path $saida 'api.out.log') -RedirectStandardError (Join-Path $saida 'api.err.log')
    $pronta = $false
    for ($i=0; $i -lt 100; $i++) {
        if ($api.HasExited) { throw 'API encerrou ao iniciar; confira os logs desta apresentacao.' }
        try { $null = Invoke-RestMethod 'http://localhost:5080/niveis' -TimeoutSec 2; $pronta = $true; break }
        catch { Start-Sleep -Milliseconds 200 }
    }
    if (!$pronta) { throw 'API nao ficou pronta em tempo.' }
    & (Join-Path $PSScriptRoot 'preparar-cena-ml.ps1') | Tee-Object -FilePath (Join-Path $saida 'previsao-ml.txt')
    $mapa = Invoke-RestMethod -Method Post 'http://localhost:5080/demo/mapa?periodo=madrugada'
    if ($mapa.inseridas -ne 12) { throw 'A cena de frio nao foi preparada.' }
    $tela = if (Test-Path -LiteralPath $desktop) { Start-Process -FilePath $desktop -WorkingDirectory (Split-Path $desktop) -PassThru }
        else { Start-Process -FilePath (Get-Command dotnet).Source -WorkingDirectory $desktopFonte -PassThru -ArgumentList 'run -c Release' }
    @("Banco novo de ensaio: $banco", "API PID: $($api.Id)", "Desktop PID: $($tela.Id)",
      'Dados simulados. Credenciais temporarias somente na memoria dos processos.',
      "Para encerrar a API deste ensaio: Stop-Process -Id $($api.Id)") |
        Set-Content -LiteralPath (Join-Path $saida 'PROCESSOS.txt') -Encoding UTF8
    Write-Output "Apresentacao pronta: API, previsao ML e mapa frio com vinte cadeiras. Registro: $saida"
} catch {
    if ($api -and !$api.HasExited) { Stop-Process -Id $api.Id }
    throw
} finally {
    foreach ($nome in $anteriores.Keys) { [Environment]::SetEnvironmentVariable($nome,$anteriores[$nome],'Process') }
}
