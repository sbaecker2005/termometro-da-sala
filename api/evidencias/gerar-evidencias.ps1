#requires -Version 5.1
param([string]$Foto)
$ErrorActionPreference = 'Stop'
$raiz = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$saida = Join-Path $PSScriptRoot 'saidas'
$seguranca = Join-Path $raiz 'seguranca/evidencias/saidas'
$projeto = Join-Path $raiz 'api/TermometroDaSala.Api'
$urlBase = 'http://localhost:5080'
$conexao = 'Server=localhost\SQLEXPRESS;Database=TermometroDaSala;Trusted_Connection=True;TrustServerCertificate=True'
$utf8 = New-Object Text.UTF8Encoding($false)
$resultados = [ordered]@{}
$arquivos = @{}
$respostas = @{}
$processo = $null
$ambiente = 'Development'
$chave = [guid]::NewGuid().ToString('N')
$temporario = Join-Path $saida ('temporario-' + [guid]::NewGuid().ToString('N'))
$variaveis = @('ConnectionStrings__Sala','ChaveCoordenacao','Mqtt__Desligado','Banco__Migrar','DOTNET_ENVIRONMENT','Fotos__Pasta','Logging__EventLog__LogLevel__Default')
$anteriores = @{}
foreach ($nome in $variaveis) { $anteriores[$nome] = [Environment]::GetEnvironmentVariable($nome, 'Process') }

function Redigir([string]$texto) {
    $texto = $texto.Replace($conexao, '***').Replace($chave, '***')
    $texto = $texto -replace 'segredo-de-teste-sala-0[1-4]', '***'
    $texto = $texto -replace '(?im)(X-Chave-Coordenacao\s*:\s*)[^\r\n]+', '$1***'
    $texto = $texto -replace '(?im)(Server=)[^\r\n"'']+', '***'
    return $texto
}
function Gravar([string]$caminho, [string]$texto) {
    [IO.File]::WriteAllText($caminho, (Redigir $texto), $utf8)
}
function Resultado([string]$id, [string]$arquivo, [bool]$ok, [string]$motivo) {
    $arquivos[$id] = $arquivo
    if ($ok) { $resultados[$id] = 'OK' } else { $resultados[$id] = "DIVERGE: $motivo" }
    Add-Content -LiteralPath (Join-Path $saida $arquivo) -Encoding UTF8 -Value "`r`nConferencia: $($resultados[$id])"
}
function Sql([string]$consulta) {
    # A biblioteca do Windows evita instalar sqlcmd ou depender do modulo SQLPS.
    $db = New-Object Data.SqlClient.SqlConnection $conexao
    try {
        $db.Open()
        $comando = $db.CreateCommand()
        $comando.CommandText = $consulta
        $comando.CommandTimeout = 15
        $adapter = New-Object Data.SqlClient.SqlDataAdapter $comando
        $dados = New-Object Data.DataSet
        [void]$adapter.Fill($dados)
        return ,$dados
    } finally { $db.Dispose() }
}
function Banco([string]$id, [string]$consulta, [scriptblock]$conferir) {
    $arquivo = "$id.txt"
    try {
        $dados = Sql $consulta
        $texto = $dados.Tables[0] | Format-Table -AutoSize | Out-String -Width 240
        Gravar (Join-Path $saida $arquivo) "$(Get-Date -Format o)`r`nSQL Server Express local; autenticacao Windows; conexao: ***`r`nSQL: $consulta`r`n$($texto -join "`r`n")"
        $ok = & $conferir $dados
        Resultado $id $arquivo $ok 'conteudo SQL diferente do esperado (ver resultado acima)'
        return $dados
    } catch {
        Gravar (Join-Path $saida $arquivo) "$(Get-Date -Format o)`r`nSQL: $consulta`r`nERRO REAL: $($_.Exception.Message)"
        Resultado $id $arquivo $false 'SQL Server indisponivel nesta execucao; ver erro gravado'
    }
}
function Parar {
    if ($script:processo) {
        if (!$script:processo.HasExited) {
            # Handle do processo criado aqui: nao depende de enumerar/matar processos de outras sessoes.
            $script:processo.Kill()
            if (!$script:processo.WaitForExit(10000)) { throw 'API nao encerrou em dez segundos.' }
        }
        $log = $script:stdout.GetAwaiter().GetResult() + $script:stderr.GetAwaiter().GetResult()
        $log += "`r`nColeta: processo PID $($script:processo.Id) encerrado; HasExited=$($script:processo.HasExited); $(Get-Date -Format o)`r`n"
        $arquivo = Join-Path $saida ("api-$script:ambiente.log")
        Gravar $arquivo $log
        if ($script:ambiente -eq 'Development') {
            Gravar (Join-Path $saida 'B4.txt') "$(Get-Date -Format o)`r`nPerfil http de Properties/launchSettings.json aplicado ao binario compilado (mesma URL e ambiente; processo direto para encerramento pelo handle).`r`ndotnet bin/Debug/net8.0/TermometroDaSala.Api.dll --urls http://localhost:5080`r`nDOTNET_ENVIRONMENT=Development; Mqtt__Desligado=true; Banco__Migrar=false (migracao em B1)`r`n$log"
            Resultado B4 'B4.txt' ($log -match 'Now listening on: http://localhost:5080' -and $resultados['B1'] -eq 'OK') 'listener HTTP iniciado, mas conexao SQL/migracao nao comprovada (B1)'
        }
        $script:processo.Dispose()
        $script:processo = $null
    }
}
function Iniciar([string]$nome) {
    Parar
    $script:ambiente = $nome
    $env:DOTNET_ENVIRONMENT = $nome
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = (Get-Command dotnet.exe).Source
    $perfil = (Get-Content (Join-Path $projeto 'Properties/launchSettings.json') -Raw -Encoding UTF8 | ConvertFrom-Json).profiles.http
    if ($perfil.applicationUrl -ne $urlBase) { throw 'URL do perfil http mudou; revisar roteiro.' }
    $info.Arguments = 'bin/Debug/net8.0/TermometroDaSala.Api.dll --urls ' + $perfil.applicationUrl
    $info.WorkingDirectory = $projeto
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $script:processo = [Diagnostics.Process]::Start($info)
    $script:stdout = $script:processo.StandardOutput.ReadToEndAsync()
    $script:stderr = $script:processo.StandardError.ReadToEndAsync()
    for ($tentativa = 0; $tentativa -lt 100; $tentativa++) {
        if ($script:processo.HasExited) { Parar; throw "API nao iniciou em $nome; veja o log" }
        $tcp = [Net.Sockets.TcpClient]::new()
        try {
            $tcp.Connect('localhost',5080)
            return
        } catch { Start-Sleep -Milliseconds 200 } finally { $tcp.Dispose() }
    }
    throw 'API nao abriu a porta 5080 em tempo'
}
function Leitura([string]$sala, [double]$temperatura = 33.4, [int]$umidade = 58, [long]$instante = 0) {
    if (!$instante) { $instante = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds() }
    $termometro = $temperatura.ToString('F1', [Globalization.CultureInfo]::InvariantCulture)
    $hmac = New-Object Security.Cryptography.HMACSHA256
    try {
        $hmac.Key = $utf8.GetBytes("segredo-de-teste-$sala")
        $assinatura = -join ($hmac.ComputeHash($utf8.GetBytes("$sala|esp32-$sala|$termometro|$umidade|$instante")) | ForEach-Object { $_.ToString('x2') })
    } finally { $hmac.Dispose() }
    return [ordered]@{ ambienteId=$sala; dispositivo="esp32-$sala"; temperaturaC=$temperatura; umidadePct=$umidade; medidoEm=$instante; assinatura=$assinatura }
}
function Chamar([string]$id, [string]$metodo, [string]$rota, [int]$esperado, [string]$corpo = '', [bool]$autorizada = $false, [hashtable]$formulario = $null, [string]$padrao = '', [string]$impedir = '') {
    $arquivo = "$id-resposta.txt"
    $request = New-Object Net.Http.HttpRequestMessage ([Net.Http.HttpMethod]::new($metodo)), ($urlBase + $rota)
    $envio = $corpo
    if ($autorizada) { $request.Headers.Add('X-Chave-Coordenacao', $chave) }
    if ($id -eq 'E15-chave-errada') { $request.Headers.Add('X-Chave-Coordenacao', ($chave+'-invalida')) }
    try {
        if ($formulario) {
            $multi = New-Object Net.Http.MultipartFormDataContent
            foreach ($campo in $formulario.Keys) {
                if ($campo -eq 'foto') {
                    $conteudo = [Net.Http.ByteArrayContent]::new([IO.File]::ReadAllBytes($formulario[$campo]))
                    $conteudo.Headers.ContentType = [Net.Http.Headers.MediaTypeHeaderValue]::new('image/jpeg')
                    $nomeFoto = [IO.Path]::GetFileName($formulario[$campo])
                    if ($formulario.ContainsKey('nomeFoto')) { $nomeFoto = $formulario.nomeFoto }
                    $multi.Add($conteudo, 'foto', $nomeFoto)
                } elseif ($campo -ne 'nomeFoto') { $multi.Add([Net.Http.StringContent]::new([string]$formulario[$campo], $utf8), $campo) }
            }
            $request.Content = $multi
            $envio = ($formulario | ConvertTo-Json -Compress) + '; foto: bytes do JPEG/arquivo indicado, sem transcricao binaria'
        } elseif ($corpo) { $request.Content = [Net.Http.StringContent]::new($corpo, $utf8, 'application/json') }
        $cabecalhosEnvio = $request.Headers.ToString()
        if ($request.Content) { $cabecalhosEnvio += $request.Content.Headers.ToString() }
        $texto = "$(Get-Date -Format o)`r`nAmbiente: $script:ambiente; MQTT desligado; conexao: ***`r`n$metodo $urlBase$rota`r`n$($cabecalhosEnvio)`r`nCorpo enviado: $envio`r`nEsperado: HTTP $esperado; $padrao`r`n"
        if ($impedir) {
            Gravar (Join-Path $saida $arquivo) ($texto + "NAO EXECUTADO: $impedir")
            Resultado $id $arquivo $false "nao executado: $impedir"
            return
        }
        $response = $cliente.SendAsync($request).GetAwaiter().GetResult()
        try {
            $status = [int]$response.StatusCode
            $headers = $response.Headers.ToString() + $response.Content.Headers.ToString()
            $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
            $tipo = [string]$response.Content.Headers.ContentType
            if ($tipo -like 'image/*') {
                $resposta = 'Base64 (corpo binario integral): ' + [Convert]::ToBase64String($bytes)
            } else { $resposta = $utf8.GetString($bytes) }
            Gravar (Join-Path $saida $arquivo) ($texto + "HTTP $status $($response.ReasonPhrase)`r`n$headers`r`n$resposta")
            $ok = $status -eq $esperado -and (!$padrao -or ($resposta + $headers) -match $padrao)
            Resultado $id $arquivo $ok "HTTP $status (esperado $esperado); conferir corpo/cabecalhos: $padrao"
            $script:respostas[$id] = @{ Status=$status; Corpo=$resposta; Headers=$headers; Bytes=$bytes }
        } finally { $response.Dispose() }
    } catch {
        Gravar (Join-Path $saida $arquivo) ($texto + "ERRO REAL: $($_.Exception.Message)")
        Resultado $id $arquivo $false 'erro de transporte; sem status HTTP'
    } finally { $request.Dispose() }
}
function Colecao($requisicao, [string]$id, [int]$esperado, [string]$padrao = '', [string]$impedir = '') {
    $rota = $requisicao.url.Replace('{{ _.base_url }}', '').Replace('{{ _.dia }}', (Get-Date -Format 'yyyy-MM-dd')).Replace('{{ _.ocorrencia_id }}', $script:ocorrencia)
    $corpo = [string]$requisicao.body.text
    switch ($id) {
        '01' { $script:aceita = Leitura 'sala-03'; $corpo = $script:aceita | ConvertTo-Json -Compress }
        '02' { $corpo = $script:aceita | ConvertTo-Json -Compress }
        '03' { $l = Leitura 'sala-02'; $l.assinatura = '00'; $corpo = $l | ConvertTo-Json -Compress }
        '04' { $corpo = Leitura 'sala-04' 70 | ConvertTo-Json -Compress }
        '06' { $corpo = Leitura 'sala-01' | ConvertTo-Json -Compress }
    }
    $auth = @($requisicao.headers | Where-Object name -eq 'X-Chave-Coordenacao').Count -gt 0
    $form = $null
    if ($id -eq '16') {
        $form = @{}
        foreach ($p in $requisicao.body.params) { $form[$p.name] = $p.value }
        $form.foto = $Foto
    }
    Chamar $id $requisicao.method $rota $esperado $corpo $auth $form $padrao $impedir
}

New-Item -ItemType Directory -Force $saida,$seguranca,$temporario | Out-Null
Add-Type -AssemblyName System.Data,System.Net.Http,System.Drawing
$vetor = New-Object Security.Cryptography.HMACSHA256
try {
    $vetor.Key = $utf8.GetBytes('Jefe')
    $hash = -join ($vetor.ComputeHash($utf8.GetBytes('what do ya want for nothing?')) | ForEach-Object { $_.ToString('x2') })
    if ($hash -ne '5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843') { throw 'Autoverificacao HMAC falhou.' }
} finally { $vetor.Dispose() }
if ((Leitura 'sala-03' 33.4 58 1794331200).assinatura -ne '2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0') { throw 'Autoverificacao do canonico falhou.' }
if ((Redigir "X-Chave-Coordenacao: $chave`r`n$conexao`r`nsegredo-de-teste-sala-03") -match 'segredo-de-teste|Server=|Trusted_Connection|'+$chave) { throw 'Autoverificacao de redacao falhou.' }
$handler = [Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$handler.AllowAutoRedirect = $false
$cliente = [Net.Http.HttpClient]::new($handler)
$cliente.Timeout = [TimeSpan]::FromSeconds(20)
$estado = $null
try {
    $listener = Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue
    if ($listener) { throw 'Porta 5080 ocupada: nenhum processo existente sera encerrado.' }
    $env:ConnectionStrings__Sala = $conexao
    $env:ChaveCoordenacao = $chave
    $env:Mqtt__Desligado = 'true'
    $env:Banco__Migrar = 'false'
    # Console guarda os logs; EventLog do Windows nao e gravavel no executor restrito.
    $env:Logging__EventLog__LogLevel__Default = 'None'
    $env:Fotos__Pasta = Join-Path $projeto 'fotos'
    # Restauracao somente pelo cache instalado: nenhuma fonte de rede e nenhum pacote novo.
    $build = & dotnet restore $projeto --source $temporario --verbosity minimal 2>&1 | Out-String
    if ($LASTEXITCODE) { throw "Restore offline falhou: $build" }
    $build += & dotnet build $projeto --no-restore --verbosity minimal 2>&1 | Out-String
    Gravar (Join-Path $saida 'build.txt') $build
    if ($LASTEXITCODE) { throw 'Build falhou; veja build.txt' }
    Push-Location (Join-Path $raiz 'api')
    try {
        $ef = & dotnet ef --version 2>&1 | Out-String
        if ($LASTEXITCODE -eq 0) {
            $migracao = & dotnet ef database update --no-build --project TermometroDaSala.Api 2>&1 | Out-String
            $migrado = $LASTEXITCODE -eq 0
            Gravar (Join-Path $saida 'B1.txt') "$(Get-Date -Format o)`r`ndotnet ef database update --no-build --project TermometroDaSala.Api`r`n$ef`r`n$migracao"
            Resultado B1 'B1.txt' $migrado 'migracao falhou; ver erro real'
        } else {
            $env:Banco__Migrar = 'true'
            try { Iniciar 'Development' } catch { Write-Warning $_.Exception.Message } finally { Parar; $env:Banco__Migrar = 'false' }
            $log = Get-Content (Join-Path $saida 'api-Development.log') -Raw -Encoding UTF8
            Gravar (Join-Path $saida 'B1.txt') "dotnet-ef indisponivel; fallback Banco__Migrar=true`r`n$log"
            Resultado B1 'B1.txt' ($log -match 'Now listening on:') 'fallback de migracao falhou'
        }
    } finally { Pop-Location }
    $null = Banco B2 'SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId' {
        param($d)
        ($d.Tables[0].Rows.MigrationId -join ',') -eq '20261105025544_Inicial,20261105033613_Enums,20261105050515_SegredosPorDispositivo'
    }
    $estado = Banco B3 'SELECT * FROM Ambientes ORDER BY Id; SELECT Id,AmbienteId,Ativo,UltimoMedidoEm FROM Dispositivos ORDER BY Id; SELECT * FROM NiveisAcao ORDER BY IndiceMin' {
        param($d)
        $d.Tables[0].Rows.Count -eq 4 -and $d.Tables[1].Rows.Count -eq 4 -and $d.Tables[2].Rows.Count -eq 5
    }
    if ($estado) {
        if (@($estado.Tables[0].Rows | Where-Object Id -eq 'sala-05').Count) { throw 'sala-05 ja existe; interrompido para nao alterar dados anteriores.' }
        if (@($estado.Tables[1].Rows | Where-Object { !$_.Ativo }).Count) { throw 'Dispositivo previamente revogado; use banco de demonstracao provisionado pelo coordenador.' }
    }
    if (!$Foto) {
        # shortcut: JPEG sintetico de sala vazia, fornecer -Foto com foto real antes dos prints da entrega.
        $Foto = Join-Path $temporario 'sala-vazia-sintetica.jpg'
        $bitmap = [Drawing.Bitmap]::new(320,200)
        $g = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $g.Clear([Drawing.Color]::WhiteSmoke)
            $g.DrawRectangle([Drawing.Pens]::Black,40,30,240,100)
            $g.DrawRectangle([Drawing.Pens]::Black,60,145,70,30)
            $g.DrawRectangle([Drawing.Pens]::Black,190,145,70,30)
            $g.DrawString('SALA VAZIA - IMAGEM SINTETICA', [Drawing.SystemFonts]::DefaultFont, [Drawing.Brushes]::Black, 45,65)
            $bitmap.Save($Foto, [Drawing.Imaging.ImageFormat]::Jpeg)
        } finally { $g.Dispose(); $bitmap.Dispose() }
    }
    $Foto = [IO.Path]::GetFullPath($Foto)
    if (!(Test-Path -LiteralPath $Foto)) { throw 'JPEG informado nao existe.' }
    $colecao = (Get-Content (Join-Path $raiz 'api/insomnia.json') -Raw -Encoding UTF8 | ConvertFrom-Json).resources | Where-Object _type -eq 'request' | Sort-Object metaSortKey
    Iniciar 'Development'
    $script:ocorrencia = [guid]::Empty.ToString()
    $status = @(201,400,400,400,200,400,201,409,401,200,200,409,204,201,200,201,200,401,200,200,200,200,200,200)
    $padroes = @{ '01'='"id".*"indiceCalor".*"nivel"'; '02'='"motivo":"reenvio"'; '03'='assinatura-invalida'; '04'='fora-da-faixa'; '05'='"ativo":false'; '06'='dispositivo-revogado'; '07'='Location: /ambientes/sala-05'; '08'='ambiente-existente'; '12'='ambiente-com-leituras'; '14'='Location: /acoes\?dia='; '19'='image/jpeg'; '22'='text/csv'; '23'='"ruaDisponivel":false' }
    foreach ($r in $colecao | Where-Object metaSortKey -le 24) {
        $n = [int]$r.metaSortKey
        $id = '{0:00}' -f $n
        $impedir = ''
        if ($n -eq 12 -and (!$respostas['01'] -or $respostas['01'].Status -ne 201)) { $impedir = '01 nao gravou leitura; DELETE poderia apagar sala-03' }
        if ($n -in 11,13 -and (!$respostas['07'] -or $respostas['07'].Status -ne 201)) { $impedir = '07 nao criou sala-05 nesta coleta' }
        Colecao $r $id $status[$n-1] $padroes[$id] $impedir
        if ($n -eq 9) {
            try {
                $semChave = Sql "SELECT COUNT(*) AS N FROM Ambientes WHERE Id='sala-06'"
                if ($semChave.Tables[0].Rows[0].N -ne 0) { Resultado '09' $arquivos['09'] $false 'sala-06 existe apos tentativa sem chave' }
            } catch { Resultado '09' $arquivos['09'] $false '401 recebido; SQL indisponivel para conferir ausencia de gravacao' }
        }
        if ($n -eq 18 -and $ocorrencia -eq [guid]::Empty.ToString()) { Resultado '18' $arquivos['18'] $false '401 recebido, mas 16 nao persistiu ocorrencia; id substituto vazio' }
        if ($n -eq 16 -and $respostas[$id].Status -eq 201) { $script:ocorrencia = ($respostas[$id].Corpo | ConvertFrom-Json).id }
        if ($n -eq 11 -and !$impedir) { Chamar '11-verificacao' GET '/ambientes/sala-05' 200 '' $false $null 'Sala 5 \(reformada\)' }
        if ($n -eq 13 -and !$impedir) { Chamar '13-verificacao' GET '/ambientes/sala-05' 404 }
    }
    Colecao ($colecao | Where-Object metaSortKey -eq 25) '25a' 404
    Iniciar 'Demo'
    # sala-02 nao recebe leituras validas no roteiro; nao apaga leituras para fabricar o 409.
    Chamar '25b' POST '/demo/semear?ambienteId=sala-02' 409 '' $false $null 'ligue o sensor primeiro'
    Colecao ($colecao | Where-Object metaSortKey -eq 25) '25c' 201 '"inseridas":'
    Chamar '23-demo' GET '/ambientes/sala-03/previsao' 200 '' $false $null '"indiceRuaPrevisto":'
    # 31 escritas sem chave nao gravam acoes e excedem a cota por IP.
    for ($i=1; $i -le 31; $i++) { Chamar ('26-{0:00}' -f $i) POST '/acoes' $(if ($i -le 28) {401} else {429}) '{}' }
    $lote = (1..31 | ForEach-Object { Get-Content (Join-Path $saida ('26-{0:00}-resposta.txt' -f $_)) -Raw -Encoding UTF8 }) -join "`r`n"
    Gravar (Join-Path $saida '26-limite.txt') $lote
    Resultado '26' '26-limite.txt' (@($respostas.Keys | Where-Object { $_ -like '26-*' -and $respostas[$_].Status -eq 429 }).Count -gt 0) 'nao houve HTTP 429 em 31 escritas'
    Chamar '27' GET '/' 200 '' $false $null 'X-Content-Type-Options: nosniff'
    # Reiniciar limpa apenas a cota em memoria, preservando a evidencia do rate limit.
    Iniciar 'Demo'
    $null = Banco B5 'SELECT TOP 5 * FROM Leituras ORDER BY Id DESC' { param($d) $d.Tables[0].Rows.Count -gt 0 }
    Chamar 'E03-reinicio' POST '/leituras' 400 ($aceita | ConvertTo-Json -Compress) $false $null 'reenvio'
    $duplicada = Leitura 'sala-03' | ConvertTo-Json -Compress
    $tarefas = @()
    $conteudos = @()
    try {
        foreach ($copia in 1..2) {
            $conteudo = [Net.Http.StringContent]::new($duplicada,$utf8,'application/json')
            $conteudos += $conteudo
            $tarefas += $cliente.PostAsync("$urlBase/leituras",$conteudo)
        }
        $concorrencia = "$(Get-Date -Format o)`r`nDemo; duas copias simultaneas; POST $urlBase/leituras`r`nContent-Type: application/json`r`nCorpo: $duplicada`r`n"
        foreach ($tarefa in $tarefas) {
            try {
                $r = $tarefa.GetAwaiter().GetResult()
                try { $concorrencia += "HTTP $([int]$r.StatusCode)`r`n$($r.Headers)$($r.Content.Headers)`r`n$($r.Content.ReadAsStringAsync().GetAwaiter().GetResult())`r`n" } finally { $r.Dispose() }
            } catch { $concorrencia += "ERRO REAL: $($_.Exception.Message)`r`n" }
        }
        Gravar (Join-Path $saida 'E03-concorrencia.txt') $concorrencia
        $arquivos['E03-concorrencia'] = 'E03-concorrencia.txt'
    } finally { foreach ($conteudo in $conteudos) { $conteudo.Dispose() } }
    foreach ($offset in @(-660,120)) {
        Chamar "E04-tempo-$offset" POST '/leituras' 400 (Leitura 'sala-02' 33.4 58 ([DateTimeOffset]::UtcNow.ToUnixTimeSeconds()+$offset) | ConvertTo-Json -Compress) $false $null 'atrasada'
    }
    Chamar 'E05-normal' GET '/ambientes/sala-03/leituras' 200
    Chamar 'E05-sqli' GET '/ambientes/sala-03%27%20OR%201%3D1%20--/leituras' 404
    Chamar 'E06-lfi' GET '/ocorrencias/..%2F..%2FWindows%2Fwin.ini/foto' 404
    Chamar 'E06-absoluto' GET '/ocorrencias/C%3A%5CWindows%5Cwin.ini/foto' 404
    $falsa = Join-Path $temporario 'falsa.jpg'
    [IO.File]::WriteAllText($falsa,'isto nao e uma imagem',$utf8)
    $grande = Join-Path $temporario 'grande.jpg'
    $fluxo = [IO.File]::Create($grande)
    try { $fluxo.SetLength(5MB+1) } finally { $fluxo.Dispose() }
    $antes = $null
    try { $antes = Sql 'SELECT COUNT(*) AS N FROM Ocorrencias; SELECT COUNT(*) AS N FROM Leituras'; } catch { }
    Chamar 'E07-conteudo' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao='Teste de arquivo invalido';foto=$falsa} 'nao-jpeg'
    Chamar 'E07-nome' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao='Teste de nome invalido';foto=$falsa;nomeFoto='../../teste.jpg'} 'nome-invalido'
    Chamar 'E07-tamanho' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao='Teste de limite de tamanho';foto=$grande} 'arquivo'
    Chamar 'E08-curta' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao='curta';foto=$Foto} 'descricao'
    Chamar 'E08-longa' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao=('a'*501);foto=$Foto} 'descricao'
    Chamar 'E08-sem-foto' POST '/ocorrencias' 400 '' $false @{ambienteId='sala-03';descricao='Ventilador sem funcionar'} 'arquivo'
    Chamar 'E08-sala' POST '/ocorrencias' 404 '' $false @{ambienteId='sala-inexistente';descricao='Ventilador sem funcionar';foto=$Foto}
    foreach ($par in @(@(-11,58),@(61,58),@(33.4,-1),@(33.4,101))) {
        Chamar "E08-faixa-$($par -join '_')" POST '/leituras' 400 (Leitura 'sala-02' $par[0] $par[1] | ConvertTo-Json -Compress) $false $null 'fora-da-faixa'
    }
    $contagem = 'Contagem indisponivel: conexao SQL falhou.'
    try {
        $depois = Sql 'SELECT COUNT(*) AS N FROM Ocorrencias; SELECT COUNT(*) AS N FROM Leituras'
        $contagem = "Antes/depois dos uploads invalidos e faixas: Ocorrencias $($antes.Tables[0].Rows[0].N)/$($depois.Tables[0].Rows[0].N); Leituras $($antes.Tables[1].Rows[0].N)/$($depois.Tables[1].Rows[0].N)"
    } catch { }
    Chamar 'E15-acoes' POST '/acoes' 401 '{}'
    Chamar 'E15-revogar' POST '/dispositivos/esp32-sala-04/revogar' 401
    Chamar 'E15-chave-errada' POST '/ambientes' 401 '{}'
    Chamar 'E14-painel' GET '/' 200
    Iniciar 'Production'
    Chamar 'E09-production' POST '/demo/semear?ambienteId=sala-03' 404
    Parar

    # Somente leitura do git: imprime metadados dos commits, nunca valores das linhas encontradas.
    Push-Location $raiz
    try {
        $historico = @("Data da busca: $(Get-Date -Format o)", 'Escopo: historico local de todos os refs, sem rede; sem ler .env* ou contas/.', (& git log -1 --format='Versao: %H %cI'))
        foreach ($padraoGit in @('segredo-de-teste','ChaveCoordenacao','password=|pwd=|User Id=')) {
            $historico += "`r`ngit log --all --oneline -i -G '$padraoGit' -- . :(exclude)**/.env* :(exclude)**/contas/**"
            $linhas = @(& git log --all --oneline -i -G $padraoGit -- . ':(exclude)**/.env*' ':(exclude)**/contas/**' 2>&1)
            if ($LASTEXITCODE) { throw 'Busca no historico falhou.' }
            if ($linhas.Count) { $historico += $linhas } else { $historico += '(nenhuma linha)' }
        }
        $historico += "`r`ngit log --all --oneline -- esp32/secrets.h (somente metadados)"
        $linhas = @(& git log --all --oneline -- esp32/secrets.h)
        if ($linhas.Count) { $historico += $linhas } else { $historico += '(nenhuma linha)' }
        $historico += "`r`ngit log --all --diff-filter=A --format= --name-only; filtro de nomes .env, secrets.h, pfx, pem, key"
        $nomes = @(& git log --all --diff-filter=A --format= --name-only | Where-Object { $_ -match '(^|/)\.env|secrets\.h$|\.pfx$|\.pem$|\.key$' } | Sort-Object -Unique)
        if ($nomes.Count) { $historico += $nomes } else { $historico += '(nenhuma linha)' }
        $historico += "`r`nLimite: metadados identificam commits a revisar; nao atestam ausencia de credenciais reais no conteudo. Wokwi nao acessado."
        Gravar (Join-Path $seguranca 'osint-historico.txt') ($historico -join "`r`n")
        $osint = Join-Path $raiz 'seguranca/evidencias/osint.txt'
        $conteudoOsint = Get-Content $osint -Raw -Encoding UTF8
        $conteudoOsint = [regex]::Replace($conteudoOsint, '(?s)\r?\n8a\. Recoleta local.*?(?=\r?\n9\.)', '')
        $adendo = "`r`n8a. Recoleta local em $(Get-Date -Format o)`r`n" + ($historico -join "`r`n") + "`r`nSaida integral desta busca: saidas/osint-historico.txt`r`n"
        $conteudoOsint = $conteudoOsint.Replace('9. Link do Wokwi', $adendo + "`r`n9. Link do Wokwi")
        [IO.File]::WriteAllText($osint, $conteudoOsint, $utf8)
    } finally { Pop-Location }

    $fontes = @('api/TermometroDaSala.Api/Program.cs','api/TermometroDaSala.Api/Dominio/Contrato.cs','api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs','api/TermometroDaSala.Api/Servicos/AssinanteMqtt.cs','api/TermometroDaSala.Api/Controllers/LeiturasController.cs','api/TermometroDaSala.Api/Controllers/OcorrenciasController.cs','api/TermometroDaSala.Api/Controllers/DemoController.cs','api/TermometroDaSala.Api/Filtros/ExigeChaveCoordenacaoAttribute.cs')
    $trechos = foreach ($fonte in $fontes) {
        $linha=0
        "`r`n$fonte"
        Get-Content (Join-Path $raiz $fonte) -Encoding UTF8 | ForEach-Object { $linha++; '{0}: {1}' -f $linha,$_ }
    }
    Gravar (Join-Path $seguranca 'E01.txt') "$(Get-Date -Format o)`r`nBase informada: main 47a92d3; worktree gs/evidencias; sem alterar codigo da API.`r`nWindows $([Environment]::OSVersion.VersionString); PowerShell $($PSVersionTable.PSVersion); SDK $(& dotnet --version)`r`nDevelopment e Demo em HTTP local; SQL Server 2022 Express; MQTT desligado.`r`nC7/C8 (VM), C12 (TLS MQTT) nao comprovados localmente.`r`n$($trechos -join "`r`n")"
    $mapa = [ordered]@{
        E02=@('01','21'); E03=@('02','03','E03-*'); E04=@('05','06','E04-tempo-*');
        E05=@('E05-*'); E06=@('16','18','19','E06-*'); E07=@('E07-*'); E08=@('E08-*');
        E09=@('25a','25b','25c','E09-*'); E10=@('05','06'); E14=@('16','17','19','E14-*'); E15=@('09','18','19','E15-*')
    }
    foreach ($id in $mapa.Keys) {
        $partes = foreach ($padrao in $mapa[$id]) {
            foreach ($item in $arquivos.Keys | Where-Object { $_ -like $padrao } | Sort-Object) {
                Get-Content (Join-Path $saida $arquivos[$item]) -Raw -Encoding UTF8
            }
        }
        $extra = ''
        if ($id -in 'E07','E08') { $extra = $contagem }
        if ($id -eq 'E03') { $extra = 'MQTT e Serial nao coletados; nenhum broker externo acessado. As duas respostas simultaneas nao substituem contagem no SQL; sem SQL acessivel, persistencia e corrida ficam inconclusivas.' }
        if ($id -eq 'E04') { $extra = 'Roteiro Insomnia revoga sala-01; ensaio temporal sala-02. ESP32 e sincronizacao fisica pendentes.' }
        if ($id -eq 'E09') { $extra = Get-Content (Join-Path $saida 'api-Production.log') -Raw -Encoding UTF8; $extra += "`r`nIIS/HTTPS pendentes; 307, se ocorrer, nao prova 404." }
        if ($id -eq 'E10') { $extra = (Get-Content (Join-Path $raiz '.gitignore') -Raw -Encoding UTF8) + "`r`n" + (Get-Content (Join-Path $seguranca 'osint-historico.txt') -Raw -Encoding UTF8) + "`r`nACL do IIS pendente." }
        if ($id -eq 'E14') {
            $extra = "Imagem sintetica quando -Foto nao informado; revisao visual e detalhes de foto real pendentes.`r`n"
            $extra += (Select-String -LiteralPath (Join-Path $raiz 'app/app/salas/[id]/ocorrencia.tsx') -Pattern 'exif: false','sem pessoas','sem identificar aluno' | Out-String)
        }
        Gravar (Join-Path $seguranca "$id.txt") "$(Get-Date -Format o)`r`n$id - parte local; prints e demais itens do roteiro permanecem pendentes.`r`n$extra`r`n$($partes -join "`r`n")"
    }
} finally {
    Parar
    # Restaura somente revogacao feita nesta coleta; leituras, acoes, ocorrencias e fotos ficam como evidencia.
    if ($estado -and $respostas['05'] -and $respostas['05'].Status -eq 200) {
        try { $null = Sql "UPDATE Dispositivos SET Ativo=1 WHERE Id='esp32-sala-01' AND Ativo=0" } catch { Write-Warning 'Nao foi possivel restaurar sala-01; coordenador deve conferir o estado.' }
    }
    foreach ($nome in $variaveis) { [Environment]::SetEnvironmentVariable($nome,$anteriores[$nome],'Process') }
    $cliente.Dispose(); $handler.Dispose()
    # O diretorio temporario e unico, criado dentro da worktree por esta execucao.
    $alvo = [IO.Path]::GetFullPath($temporario)
    if ($alvo.StartsWith([IO.Path]::GetFullPath($saida) + [IO.Path]::DirectorySeparatorChar)) { Remove-Item -LiteralPath $alvo -Recurse -Force }
}
$resumo = foreach ($id in $resultados.Keys) { "$id`t$($resultados[$id])`t$($arquivos[$id])" }
Gravar (Join-Path $saida 'conferencia.tsv') ($resumo -join "`r`n")
$readme = Join-Path $PSScriptRoot 'README.md'
$texto = Get-Content $readme -Raw -Encoding UTF8
foreach ($id in $resultados.Keys | Where-Object { $_ -match '^(B[1-5]|0[1-9]|1[0-9]|2[0-7][abc]?)$' }) {
    $link = "[saida](saidas/$($arquivos[$id])) - $($resultados[$id])"
    $texto = [regex]::Replace($texto, ('(?m)^(\| '+[regex]::Escape($id)+' \|[^\r\n]*\| )[^|\r\n]*( \|)\r?$'), ('$1'+$link+'$2'))
}
[IO.File]::WriteAllText($readme, $texto, $utf8)
$documento = Join-Path $raiz 'seguranca/EVIDENCIAS.md'
$texto = Get-Content $documento -Raw -Encoding UTF8
foreach ($id in @('E01','E02','E03','E04','E05','E06','E07','E08','E09','E10','E14','E15')) {
    $texto = [regex]::Replace($texto, ('(?m)^(\*\*Print '+$id+':\*\* \[inserir[^\r\n]*?\])[^\r\n]*'), ('$1 [saida local](evidencias/saidas/'+$id+'.txt)'))
}
[IO.File]::WriteAllText($documento, $texto, $utf8)
$principais = @($resultados.Keys | Where-Object { $_ -match '^(B[1-5]|0[1-9]|1[0-9]|2[0-7][abc]?)$' })
$ok = @($principais | Where-Object { $resultados[$_] -eq 'OK' }).Count
Write-Output "Evidencias principais: $ok OK; $($principais.Count-$ok) DIVERGE. Ver saidas/conferencia.tsv."
