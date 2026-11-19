#requires -Version 5.1
param([switch]$Autoverificar)
$ErrorActionPreference = 'Stop'
$url = 'http://localhost:5080'
$utf8 = [Text.Encoding]::UTF8

function Assinar-Leitura([long]$Instante) {
    $hmac = [Security.Cryptography.HMACSHA256]::new()
    try {
        # Valor publico do contrato, somente para a sala de demonstracao.
        $hmac.Key = $utf8.GetBytes('segredo-de-teste-sala-03')
        $canonico = "sala-03|esp32-sala-03|33.4|58|$Instante"
        return -join ($hmac.ComputeHash($utf8.GetBytes($canonico)) | ForEach-Object { $_.ToString('x2') })
    } finally { $hmac.Dispose() }
}

if ((Assinar-Leitura 1794331200) -ne '2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0') {
    throw 'O vetor HMAC publico nao conferiu. Nenhuma requisicao foi enviada.'
}
if ($Autoverificar) {
    Write-Output 'Vetor HMAC do contrato: OK. Nenhuma API ou banco foi acessado.'
    return
}

# Periodo invalido: em Demo retorna 400 sem acessar/gravar o banco; fora de Demo, 404.
# A presenca de uma rota no Swagger, sozinha, nao confirma o ambiente de execucao.
Add-Type -AssemblyName System.Net.Http
$handler = [Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$cliente = [Net.Http.HttpClient]::new($handler)
$cliente.Timeout = [TimeSpan]::FromSeconds(10)
$resposta = $null
try {
    $resposta = $cliente.PostAsync("$url/demo/mapa?periodo=verificar-ambiente", $null).GetAwaiter().GetResult()
    $conteudo = $resposta.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    if ([int]$resposta.StatusCode -ne 400 -or ($conteudo | ConvertFrom-Json).motivo -ne 'periodo-invalido') {
        throw 'A API nao confirmou o ambiente Demo. Nenhuma leitura foi enviada.'
    }
} finally {
    if ($resposta) { $resposta.Dispose() }
    $cliente.Dispose()
    $handler.Dispose()
}
$instante = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$leitura = @{
    ambienteId = 'sala-03'; dispositivo = 'esp32-sala-03'
    temperaturaC = 33.4; umidadePct = 58; medidoEm = $instante
    assinatura = Assinar-Leitura $instante
}
Invoke-RestMethod -Method Post -Uri "$url/leituras" -ContentType 'application/json' -Body ($leitura | ConvertTo-Json -Compress) -TimeoutSec 20 | Out-Null
$preparo = Invoke-RestMethod -Method Post -Uri "$url/demo/semear?ambienteId=sala-03" -TimeoutSec 20
if ($preparo.inseridas -ne 18) { throw 'O preparo nao confirmou as 18 leituras historicas esperadas.' }
$previsao = Invoke-RestMethod -Uri "$url/ambientes/sala-03/previsao" -TimeoutSec 20
if (!$previsao.demonstracao -or !$previsao.ruaDisponivel -or $null -eq $previsao.deltaSala -or $null -eq $previsao.indiceSalaPrevisto) {
    throw 'Previsao incompleta. Confira modelo, CSV e leituras; nao grave o video com esse estado.'
}
Write-Host 'DEMONSTRACAO: leitura de ensaio enviada por HTTP, sem Wokwi ou sensor fisico.'
Write-Host 'A API calculou a previsao abaixo; o valor varia com a hora e o mes.'
$previsao | ConvertTo-Json -Depth 6
