[CmdletBinding()]
param(
    # API do laboratorio ou local (banco descartavel). Nunca apontar para servico de terceiros.
    # No laboratorio use https://api.termometro.local (certificado importado conforme infra/README.md, "HTTPS no laboratorio"):
    # em Production a API redireciona HTTP para HTTPS e todo passo divergiria.
    [Parameter(Mandatory)][uri]$Api
)
# Cenarios de ataque da secao 4.2 do RELATORIO-RISCOS.md: SQLi (S1), LFI e upload (S2) e OSINT (S3).
# Cada passo imprime esperado x obtido. Usa so os vetores de teste publicos (segredo-de-teste-sala-0N).
# A chave da coordenacao vem de $env:CHAVE_COORDENACAO e nunca e impressa; necessaria para S2.5, S3.3, S3.5 e S3.6.
# Rodar antes do E04: S3 revoga esp32-sala-04 (sem rota de reativacao; recriar o banco descartavel depois).
# Faz 11 escritas, abaixo da cota de 30 por minuto. Sai com o numero de passos que divergem (0 = tudo confere).
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($env:CHAVE_COORDENACAO)) { throw 'Defina CHAVE_COORDENACAO para os downloads autorizados e a revogacao.' }
$Base = $Api.AbsoluteUri.TrimEnd('/')
$Pasta = Join-Path $env:TEMP ('termometro-ataques-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $Pasta | Out-Null
$Utf8 = New-Object System.Text.UTF8Encoding($false)
$Falhas = 0

function Pedir([string[]]$Argumentos) {
    $corpo = Join-Path $Pasta 'corpo.bin'
    # curl nao cria o arquivo quando a resposta vem sem corpo: apagar evita ler o corpo do pedido anterior.
    Remove-Item -LiteralPath $corpo -ErrorAction SilentlyContinue
    $status = & curl.exe -s --path-as-is -o $corpo -w '%{http_code}' @Argumentos
    $bytes = New-Object byte[] 0
    if (Test-Path $corpo) { $bytes = [System.IO.File]::ReadAllBytes($corpo) }
    [pscustomobject]@{ Status = [int]$status; Bytes = $bytes; Texto = [System.Text.Encoding]::UTF8.GetString($bytes) }
}

function Passo([string]$Id, [string]$Ataque, [string]$Esperado, [string]$Obtido, [bool]$Confere) {
    $marca = if ($Confere) { 'CONFERE' } else { $script:Falhas++; 'DIVERGE' }
    '{0} {1}' -f $Id, $Ataque
    '      esperado: {0}' -f $Esperado
    '      obtido:   {0}  [{1}]' -f $Obtido, $marca
}

function Leitura([string]$Sala, [double]$Temp, [int]$Umidade, [long]$MedidoEm) {
    $dispositivo = 'esp32-' + $Sala
    $hmac = New-Object System.Security.Cryptography.HMACSHA256 (, $Utf8.GetBytes('segredo-de-teste-' + $Sala))
    $canonico = '{0}|{1}|{2}|{3}|{4}' -f $Sala, $dispositivo, $Temp.ToString('0.0', [cultureinfo]::InvariantCulture), $Umidade, $MedidoEm
    $assinatura = -join ($hmac.ComputeHash($Utf8.GetBytes($canonico)) | ForEach-Object { $_.ToString('x2') })
    $hmac.Dispose()
    $json = '{{"ambienteId":"{0}","dispositivo":"{1}","temperaturaC":{2},"umidadePct":{3},"medidoEm":{4},"assinatura":"{5}"}}' -f $Sala, $dispositivo, $Temp.ToString([cultureinfo]::InvariantCulture), $Umidade, $MedidoEm, $assinatura
    $arquivo = Join-Path $Pasta 'leitura.json'
    [System.IO.File]::WriteAllText($arquivo, $json, $Utf8)
    Pedir @('-X', 'POST', '-H', 'Content-Type: application/json', '--data-binary', "@$arquivo", "$Base/leituras")
}

function Contar([string]$Texto) { if ($Texto.Trim() -eq '[]') { 0 } else { ([regex]::Matches($Texto, '"medidoEm"')).Count } }
function Motivo($r) { if ($r.Texto -match '"motivo":"([^"]+)"') { $Matches[1] } else { '-' } }
function Ocorrencia([string]$Arquivo, [string]$Nome) {
    Pedir @('-X', 'POST', '-F', 'ambienteId=sala-04', '-F', 'descricao=Teste de seguranca do laboratorio', '-F', "foto=@$Arquivo;type=image/jpeg;filename=$Nome", "$Base/ocorrencias")
}

# JPEG minimo valido (quadro SOF 1x1 e varredura SOS), o mesmo dos testes da API.
$Jpeg = Join-Path $Pasta 'sala-vazia.jpg'
[System.IO.File]::WriteAllBytes($Jpeg, [byte[]](0xFF,0xD8,0xFF,0xC0,0x00,0x0B,0x08,0x00,0x01,0x00,0x01,0x01,0x01,0x11,0x00,0xFF,0xDA,0x00,0x08,0x01,0x01,0x00,0x00,0x3F,0x00,0x00,0xFF,0xD9))
# Mesmo JPEG com um segmento APP1 simulando EXIF com localizacao (texto GPS-TESTE).
$Exif = Join-Path $Pasta 'com-gps.jpg'
$app1 = [System.Text.Encoding]::ASCII.GetBytes("EXIF`0`0GPS-TESTE")
[System.IO.File]::WriteAllBytes($Exif, [byte[]](@(0xFF,0xD8,0xFF,0xE1,0x00,($app1.Length + 2)) + $app1 + @(0xFF,0xC0,0x00,0x0B,0x08,0x00,0x01,0x00,0x01,0x01,0x01,0x11,0x00,0xFF,0xDA,0x00,0x02,0x01,0xFF,0xD9)))
$Script = Join-Path $Pasta 'shell.aspx'
[System.IO.File]::WriteAllText($Script, '<%@ Page Language="C#" %><% Response.Write("teste inofensivo"); %>', $Utf8)

'Cenarios de ataque - Termometro da Sala'
'Data: {0}' -f (Get-Date -Format o)
'API: {0}' -f $Base
''
'S1 - SQL Injection no id de GET /ambientes/{id}/leituras (R3, controle C2)'
$agora = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$r = Leitura 'sala-01' 31.0 50 $agora
if ($r.Status -ge 300 -and $r.Status -lt 400) { throw "A API respondeu $($r.Status) (redirecionamento): use a URL https:// da API." }
# medidoEm 60 s atras: a forja do S3.1, com o horario atual, continua posterior e nao cai em reenvio.
$r4 = Leitura 'sala-04' 30.5 55 ($agora - 60)
Passo 'S1.1' 'Dado de referencia: leituras assinadas na sala-01 (que a injecao tentaria listar) e na sala-04' '201 e 201' ('{0} e {1}' -f $r.Status, $r4.Status) ($r.Status -eq 201 -and $r4.Status -eq 201)
$r = Pedir @("$Base/ambientes/sala-04/leituras")
$n = Contar $r.Texto
Passo 'S1.2' 'Linha de base: GET /ambientes/sala-04/leituras' '200 com as leituras so da sala-04 (ao menos a do S1.1)' ('{0}, {1} leituras' -f $r.Status, $n) ($r.Status -eq 200 -and $n -ge 1 -and $r.Texto -notmatch 'esp32-sala-01')
foreach ($c in @(
    @('S1.3', "sala-04' OR 1=1 --", 'sala-04%27%20OR%201%3D1%20--'),
    @('S1.4', "x' UNION SELECT Id, Segredo FROM Dispositivos --", 'x%27%20UNION%20SELECT%20Id%2C%20Segredo%20FROM%20Dispositivos%20--'),
    @('S1.5', "sala-04'; DROP TABLE Leituras; --", 'sala-04%27%3B%20DROP%20TABLE%20Leituras%3B%20--'))) {
    $r = Pedir @("$Base/ambientes/$($c[2])/leituras")
    Passo $c[0] ("Injecao no id: {0}" -f $c[1]) '404, sem leituras de outra sala nem segredo' ('{0}, {1} bytes' -f $r.Status, $r.Bytes.Length) ($r.Status -eq 404 -and $r.Texto -notmatch 'esp32-sala-01|segredo')
}
$r = Pedir @("$Base/ambientes/sala-04/leituras?formato=csv%27%20OR%20%271%27%3D%271")
Passo 'S1.6' "Injecao no parametro formato: csv' OR '1'='1" ('200 em JSON com as mesmas {0} leituras da sala-04' -f $n) ('{0}, {1} leituras' -f $r.Status, (Contar $r.Texto)) ($r.Status -eq 200 -and (Contar $r.Texto) -eq $n -and $r.Texto -notmatch 'esp32-sala-01')
$r = Pedir @("$Base/ambientes/sala-01/leituras")
Passo 'S1.7' 'Depois do DROP TABLE: GET /ambientes/sala-01/leituras' '200 com a leitura do S1.1 (tabela intacta)' ('{0}, {1} leituras' -f $r.Status, (Contar $r.Texto)) ($r.Status -eq 200 -and (Contar $r.Texto) -ge 1)
''
'S2 - LFI e upload malicioso (R4, R5, controle C3 e C4)'
$r = Pedir @("$Base/appsettings.json")
Passo 'S2.1' 'GET /appsettings.json (configuracao fora de wwwroot)' '404, sem ConnectionStrings' ('{0}, {1} bytes' -f $r.Status, $r.Bytes.Length) ($r.Status -eq 404 -and $r.Texto -notmatch 'ConnectionStrings')
$r = Pedir @("$Base/../appsettings.json")
Passo 'S2.2' 'GET /../appsettings.json (travessia na URL, sem normalizar no cliente)' '4xx, sem ConnectionStrings' ('{0}, {1} bytes' -f $r.Status, $r.Bytes.Length) ($r.Status -ge 400 -and $r.Status -lt 500 -and $r.Texto -notmatch 'ConnectionStrings')
foreach ($c in @(
    @('S2.3', '..%2F..%2Fappsettings.json'),
    @('S2.4', 'C%3A%5CWindows%5Cwin.ini'),
    @('S2.5', [guid]::NewGuid().ToString()))) {
    $r = Pedir @('-H', "X-Chave-Coordenacao: $env:CHAVE_COORDENACAO", "$Base/ocorrencias/$($c[1])/foto")
    # 404 vem da rota {id:guid}; o HTTP.sys (IIS) pode barrar a travessia antes, com 400 ou 403.
    Passo $c[0] ("GET /ocorrencias/{0}/foto" -f $c[1]) '4xx (404 na API), sem bytes de arquivo do servidor' ('{0}, {1} bytes' -f $r.Status, $r.Bytes.Length) ($r.Status -ge 400 -and $r.Status -lt 500 -and $r.Texto -notmatch 'ConnectionStrings|\[fonts\]')
}
foreach ($c in @(
    @('S2.6', '../../wwwroot/index.html'),
    @('S2.7', '..\..\web.config'))) {
    $r = Ocorrencia $Jpeg $c[1]
    Passo $c[0] ("Upload de JPEG valido com nome {0}" -f $c[1]) '400 nome-invalido' ('{0} {1}' -f $r.Status, (Motivo $r)) ($r.Status -eq 400 -and (Motivo $r) -eq 'nome-invalido')
}
$r = Ocorrencia $Script 'shell.jpg'
Passo 'S2.8' 'Upload de pagina ASPX declarada como image/jpeg' '400 nao-jpeg' ('{0} {1}' -f $r.Status, (Motivo $r)) ($r.Status -eq 400 -and (Motivo $r) -eq 'nao-jpeg')
$r = Ocorrencia $Jpeg 'foto.jpg'
$id = if ($r.Texto -match '"id":"([0-9a-f-]{36})"') { $Matches[1] } else { '' }
Passo 'S2.9' 'Upload legitimo (JPEG de sala vazia)' '201 com id GUID gerado no servidor' ('{0} id={1}' -f $r.Status, $id) ($r.Status -eq 201 -and $id -ne '')
$r = Pedir @("$Base/ocorrencias/$id/foto")
Passo 'S2.10' 'GET da foto pelo id, sem nenhuma chave (R12 corrigido)' '401 chave-invalida' ('{0}, {1} bytes' -f $r.Status, $r.Bytes.Length) ($r.Status -eq 401 -and (Motivo $r) -eq 'chave-invalida')
''
'S3 - OSINT: vetor de teste publico e metadados de foto (R6, R7, R10, controle C6, C9 e C10)'
$agora = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$r = Leitura 'sala-04' 33.4 58 $agora
Passo 'S3.1' 'Leitura forjada com o segredo-de-teste-sala-04 copiado do repositorio publico' '201 (vetor de teste vale ate a revogacao: risco R7 real)' $r.Status ($r.Status -eq 201)
$r = Ocorrencia $Exif 'foto.jpg'
$idExif = if ($r.Texto -match '"id":"([0-9a-f-]{36})"') { $Matches[1] } else { '' }
Passo 'S3.2' 'Upload de foto com APP1/EXIF contendo GPS-TESTE' '201' $r.Status ($r.Status -eq 201)
$r = Pedir @('-H', "X-Chave-Coordenacao: $env:CHAVE_COORDENACAO", "$Base/ocorrencias/$idExif/foto")
$temGps = $r.Texto.Contains('GPS-TESTE')
Passo 'S3.3' 'Download da foto: procurar GPS-TESTE nos bytes' '200 sem GPS-TESTE (APP1 removido)' ('{0}, GPS-TESTE presente: {1}' -f $r.Status, $temGps) ($r.Status -eq 200 -and -not $temGps)
$r = Pedir @('-X', 'POST', '-H', 'Content-Length: 0', "$Base/dispositivos/esp32-sala-04/revogar")
Passo 'S3.4' 'Atacante tenta revogar o sensor sem a chave da coordenacao' '401 chave-invalida' ('{0} {1}' -f $r.Status, (Motivo $r)) ($r.Status -eq 401 -and (Motivo $r) -eq 'chave-invalida')
if ($env:CHAVE_COORDENACAO) {
    $r = Pedir @('-X', 'POST', '-H', 'Content-Length: 0', '-H', "X-Chave-Coordenacao: $env:CHAVE_COORDENACAO", "$Base/dispositivos/esp32-sala-04/revogar")
    Passo 'S3.5' 'Coordenacao revoga esp32-sala-04 (resposta ao vazamento)' '200 com ativo false' ('{0} {1}' -f $r.Status, $r.Texto) ($r.Status -eq 200 -and $r.Texto -match '"ativo":false')
    $r = Leitura 'sala-04' 33.4 58 ($agora + 1)
    Passo 'S3.6' 'Mesma forja depois da revogacao' '400 dispositivo-revogado' ('{0} {1}' -f $r.Status, (Motivo $r)) ($r.Status -eq 400 -and (Motivo $r) -eq 'dispositivo-revogado')
} else {
    'S3.5 e S3.6 nao executados: defina $env:CHAVE_COORDENACAO para registrar a revogacao.'
}
''
'Passos que divergem do esperado: {0}' -f $Falhas
Remove-Item -LiteralPath $Pasta -Recurse -Force
exit $Falhas
