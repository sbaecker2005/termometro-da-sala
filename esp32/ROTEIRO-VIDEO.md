# Vídeo de Physical Computing — até 4 minutos

Meta: **3 min 30 s** (o limite é 4 min; a folga evita perder ponto por estouro). O circuito mede a Sala 7 na solução Termômetro da Sala: quatro DHT22 no mesmo ESP32, não um projeto à parte. O vídeo tem de provar a integração: os quatro pontos de `GET /ambientes/sala-07/mapa` vêm do Wokwi, pela API real. **Não usar o modo simulado do app**: nele a sala não vem destes sensores. Não mostrar `secrets.h` aberto nem a chave em zoom.

## Antes de gravar

1. API em ambiente Demo, com o assinante MQTT ligado (sem `Mqtt__Desligado`) e o banco migrado: `dotnet run --project TermometroDaSala.Api --launch-profile http` dentro de `api/` (ver `api/README.md`). Antes de ligar o circuito: `Invoke-RestMethod -Method Post "http://localhost:5080/demo/mapa?periodo=tarde"`. Não repetir o POST depois que o circuito publicar (409).
2. Wokwi e Serial (115200) lado a lado. Noutra janela, `http://localhost:5080/ambientes/sala-07/mapa` e `http://localhost:5080/ambientes/sala-07/leituras`.
3. Passagem antes da tomada: os quatro DHT22 em **24 °C / 40 %** até as quatro linhas “Leitura publicada”, simulação parada. O diagrama abre a janela em 35 °C / 60 % e os outros três em 24 °C / 60 %. A API marca como suspeita um salto acima de 5 °C; sem essa passagem a janela sai de 35 °C para 24 °C. No play, pôr de novo os quatro em 24 °C / 40 % antes da primeira linha de temperatura. Se a janela publicar 35 °C, parar e repetir a passagem.
4. Simulação parada, janela com Wokwi e Serial lado a lado.

## Os quatro estados

Os passos são só o DHT22 da janela (`demo-sala-07-janela`). Fundo, porta e centro ficam em 24 °C / 40 % (índice 23,5 °C, Normal) o vídeo inteiro.

| Passo | Janela | Índice | Nível | LED | Salto |
|---|---|---|---|---|---|
| 1 | 24 °C / 40 % | 23,5 °C | Normal | verde | — |
| 2 | 29 °C / 50 % | 29,7 °C | Atenção | amarelo fixo | 5 °C |
| 3 | 33 °C / 55 % | 37,8 °C | Cuidado extremo | vermelho fixo | 4 °C |
| 4 | 37 °C / 55 % | 48,5 °C | Perigo | vermelho piscando (500 ms) | 4 °C |

O par 30/50 do contrato também dá Atenção (31,0 °C), mas de 24 para 30 o salto da janela é de 6 °C e a leitura sairia como suspeita; por isso o vídeo usa 29/50. Entre um passo e outro, esperar as quatro linhas “Leitura publicada” (cerca de 10 s). O LCD troca de página a cada 3 s e o ciclo completo leva 30 s: não esperar o ciclo para trocar de passo. Com os quatro na mesma temperatura, o empate deixa a janela nos dois extremos. Do passo 2 em diante o extremo quente é a janela e o frio é o fundo.

## Roteiro

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:20 | Nome do projeto, equipe e a Sala 7 com os quatro pontos. | O Termômetro da Sala mede a Sala 7 em quatro pontos: janela, fundo, porta e centro. A sala esquenta e a escola só reage quando alguém passa mal. O professor acompanha pelo app. A coordenação olha o mapa, decide e registra. |
| 0:20–0:45 | Os quatro DHT22, o LCD, os LEDs e o buzzer; em seguida a publicação assinada de cada ponto. | Cada ponto publica a sua leitura assinada. No local, o LCD troca os pontos e os extremos, e o LED mostra o alerta mais grave. Sem internet o aviso continua. A API confere a assinatura de cada um e grava. |
| 0:45–1:10 | Janela D15, fundo D32, porta D33, centro D25, em 3,3 V, cada um com pull-up de 10 kΩ. Verde D18, amarelo D19, vermelho D4, com 220 Ω e cátodo no GND. LCD 16x2 I2C em D21 (SDA) e D22 (SCL). Buzzer D26. Botão Ciente D27. | Janela no D15, fundo no D32, porta no D33 e centro no D25. Os LEDs seguem no D18, no D19 e no D4. O LCD está no D21 e no D22, o buzzer no D26 e o Ciente no D27. O LCD escreve o ponto e o nível por extenso. |
| 1:10–1:35 | Play. Antes da primeira linha de temperatura, pôr os quatro em 24 °C / 40 %. No Serial, o HMAC `2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0` e “Vetores de calor OK”. Se a medição sair antes do NTP: “Aguardando NTP; alerta local ativo”. | O ESP32 confere o vetor do contrato, os limites, o LCD e os extremos. Se algo falhar, não segue. Sem relógio, o alerta local funciona e a leitura não sai com data inventada. |
| 1:35–2:00 | Quatro linhas `24.0 C, 40%, indice 23.5 C, Normal` e quatro “Leitura publicada”. LED verde. LCD `janela 24.0C` ou `janela I23.5C`, segunda linha `Normal`. Mapa da sala 7 com `demo-sala-07-janela`, `demo-sala-07-fundo`, `demo-sala-07-porta` e `demo-sala-07-centro`. | Os quatro em Normal, índice 23,5, LED verde. São quatro publicações. No mapa da sala 7 estão os quatro identificadores, e a leitura do Wokwi entra com suspeita falsa. |
| 2:00–2:50 | Passos 2, 3 e 4, só na janela, esperando as quatro “Leitura publicada”: amarelo fixo, vermelho fixo e vermelho piscando. No passo 4 o buzzer bipa; apertar Ciente: o bip para, a página da janela mostra `Perigo (ciente)` e o LED segue piscando. No extremo, `Quente janela` e `Frio fundo`. A leitura `demo-sala-07-janela` com índice 48,5 °C e `"suspeita": false`. | Subo só a janela, sem passar de 5 graus, e espero as quatro publicações. Atenção no amarelo, cuidado no vermelho fixo, perigo no vermelho piscando, índice 48,5, com bip. O LCD muda a cada 3 segundos; no extremo, Quente janela e Frio fundo. O Ciente cala o bip e não muda a leitura publicada. O LED segue piscando. A leitura da janela fica com suspeita falsa. |
| 2:50–3:15 | Voltar só a janela para 33 °C / 55 % (vermelho fixo, sem bip). Mapa da sala 7 com os quatro pontos. | Volto a janela a 33 graus e 55 por cento: o bip some e o vermelho fica fixo. A coordenação vê os quatro pontos no mapa, decide e registra a ação. Quem avisa é o aparelho; quem registra é a coordenação. |
| 3:15–3:30 | Fechar sem mostrar a chave. | Os quatro IDs de demonstração são revogados depois da avaliação. QoS 0 pode perder o lote; em 15 minutos a API avisa que não há leitura recente. Sensor simulado pede calibração. |

Se o broker público ou o NTP falharem no dia, “Leitura publicada” não aparece e o vídeo não prova a integração. Gravar com antecedência e guardar os prints do ensaio como plano B. Ficam no ensaio, fora destes 3 min 30 s: o arranque do diagrama (janela a 35 °C), o frio (fundo a 15 °C, demais a 22 °C, 60 %), o DHT22 inválido, o reenvio da mensagem assinada, “baixar todos, voltar ao perigo” e “Sem Wi-Fi ou NTP”.
