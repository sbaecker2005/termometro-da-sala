<#
    Envia leituras assinadas para a API, para o painel ter dado na gravação.

    Usa os segredos de demonstração que já estão no repositório
    (api/.../Dados/SalaDb.cs e ferramentas/vetor_hmac.py). Não são chaves de
    produção: servem para demonstrar o caminho ESP32 -> API, não para proteger
    aparelho real. Em campo, cada dispositivo recebe segredo próprio e a troca
    é revogar o dispositivo e cadastrar outro.

    Exemplos:
      .\enviar-leitura.ps1                           # uma leitura em cada sala
      .\enviar-leitura.ps1 -Sala 3 -Temperatura 34.2 -Umidade 62
      .\enviar-leitura.ps1 -Serie 6 -IntervaloSeg 60 # histórico de 6 pontos
#>
[CmdletBinding()]
param(
    [string]$Base = 'https://api.termometro.local',
    [ValidateRange(1, 4)][int[]]$Sala = @(1, 2, 3, 4),
    [double]$Temperatura = 0,
    [int]$Umidade = 0,
    [ValidateRange(1, 50)][int]$Serie = 1,
    [int]$IntervaloSeg = 120
)

$ErrorActionPreference = 'Stop'
$inv = [System.Globalization.CultureInfo]::InvariantCulture

function New-Assinatura([string]$Segredo, [string]$Canonico) {
    $h = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Segredo))
    try { ([BitConverter]::ToString($h.ComputeHash([Text.Encoding]::UTF8.GetBytes($Canonico)))) -replace '-', '' }
    finally { $h.Dispose() }
}

$agora = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$enviadas = 0
$recusadas = 0

# Mais antigo primeiro: a API recusa 'reenvio' se o carimbo não avançar.
foreach ($passo in ($Serie - 1)..0) {
    foreach ($n in $Sala) {
        $ambiente = 'sala-{0:00}' -f $n
        $dispositivo = 'esp32-{0}' -f $ambiente
        $segredo = 'segredo-de-teste-{0}' -f $ambiente

        # Janela aceita: de -10 min a +1 min em relação a agora.
        $medidoEm = $agora - ($passo * $IntervaloSeg)
        if ($medidoEm -lt ($agora - 570)) {
            Write-Warning "$ambiente passo $passo fora da janela de 10 min; reduza -Serie ou -IntervaloSeg"
            continue
        }

        $t = if ($Temperatura -ne 0) { $Temperatura } else { [math]::Round(26 + ($n * 1.7) + ($passo * 0.4), 1) }
        $u = if ($Umidade -ne 0) { $Umidade } else { 52 + ($n * 3) }

        # Canônico: ambiente|dispositivo|temperatura(0.0)|umidade|medidoEm — sempre invariante.
        $tTexto = $t.ToString('0.0', $inv)
        $canonico = "$ambiente|$dispositivo|$tTexto|$u|$medidoEm"
        $assinatura = New-Assinatura $segredo $canonico

        $corpo = "{""ambienteId"":""$ambiente"",""dispositivo"":""$dispositivo""," +
                 """temperaturaC"":$tTexto,""umidadePct"":$u," +
                 """medidoEm"":$medidoEm,""assinatura"":""$assinatura""}"

        try {
            $r = Invoke-RestMethod -Uri "$Base/leituras" -Method Post -ContentType 'application/json' -Body $corpo
            $marca = if ($r.suspeita) { ' [marcada como suspeita]' } else { '' }
            Write-Host ("OK   {0} {1,5:0.0}C {2}% -> indice {3:0.0} nivel {4}{5}" -f $ambiente, $t, $u, $r.indice, $r.nivel, $marca) -ForegroundColor Green
            $enviadas++
        }
        catch {
            $motivo = try { ($_.ErrorDetails.Message | ConvertFrom-Json).motivo } catch { $_.Exception.Message }
            Write-Host ("RECUSADA {0}: {1}" -f $ambiente, $motivo) -ForegroundColor Yellow
            $recusadas++
        }
        Start-Sleep -Milliseconds 400   # respeita o limitador de escrita
    }
}

Write-Host ""
Write-Host "$enviadas aceitas, $recusadas recusadas. Abra $Base para ver o mural."
