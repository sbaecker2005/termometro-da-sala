# Vídeo de Physical Computing — até 4 minutos

Meta: **3 min 30 s** (o limite é 4 min; a folga evita perder ponto por estouro). O circuito é o sensor da porta da sala na solução Termômetro da Sala, não um projeto à parte. O vídeo tem de provar a integração: o número que aparece na tela da sala 3 vem do ESP32 do Wokwi, pela API real. **Não usar o modo simulado do app** (`EXPO_PUBLIC_API_MOCK` diferente de `0`): nele a sala 3 não vem do sensor. Não mostrar `secrets.h` aberto nem a chave em zoom.

## Antes de gravar

1. API real no ar com o assinante MQTT ligado (sem `Mqtt__Desligado`) e o banco migrado: `dotnet run --project TermometroDaSala.Api --launch-profile http` dentro de `api/` (ver `api/README.md`). Painel web em `http://localhost:5080/`.
2. Na tela da integração, deixar aberto o painel web na sala 3 **ou** o app com `EXPO_PUBLIC_API_MOCK=0 EXPO_PUBLIC_API_URL=http://IP_DO_COMPUTADOR:5080` (ver `app/README.md`). Numa aba ao lado, `http://localhost:5080/ambientes/sala-03/leituras` para mostrar a leitura gravada.
3. Ensaio de aquecimento: rodar o Wokwi uma vez com o DHT22 em **24 °C / 40 %** até aparecer “Leitura publicada” e parar a simulação. A API marca como `suspeita` um salto de mais de 5 °C em relação à leitura anterior do dispositivo (CONTRATO.md, tabela de rejeições); começando de 24 °C, nenhum passo do vídeo passa de 5 °C.
4. Simulação parada, Serial em 115200, janela com Wokwi e Serial lado a lado.

## Os quatro estados

| Passo | DHT22 | Índice | Nível | LED | Salto |
|---|---|---|---|---|---|
| 1 | 24 °C / 40 % | 23,5 °C | Normal | verde | — |
| 2 | 29 °C / 50 % | 29,7 °C | Atenção | amarelo | 5 °C |
| 3 | 33 °C / 55 % | 37,8 °C | Cuidado extremo | vermelho fixo | 4 °C |
| 4 | 37 °C / 55 % | 48,5 °C | Perigo | vermelho piscando (500 ms) | 4 °C |

O par 30/50 do contrato também dá Atenção (31,0 °C), mas de 24 para 30 o salto é de 6 °C e a leitura entraria como suspeita; por isso o vídeo usa 29/50. Entre um passo e outro, esperar a linha “Leitura publicada” (cerca de 10 s).

## Roteiro

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:20 | Nome do projeto, equipe e a frase do problema: a sala esquenta e a escola só reage quando alguém passa mal. Professor (app) e coordenação (desktop e painel). | O ESP32 fica na porta dessa mesma sala. Ele mede a sala; a rua vem do INMET. |
| 0:20–0:45 | Fluxo numa frase: sensor → LED, LCD e buzzer na porta → leitura assinada por MQTT → API → previsão de 2 h → app e desktop. | Sem internet o aviso na porta continua. A API valida a assinatura e é quem oficializa o índice e a previsão. |
| 0:45–1:10 | Circuito: DHT22 em 3,3 V, DATA em D15 com pull-up de 10 kΩ; verde D18, amarelo D19 e vermelho D4, cada um com 220 Ω e cátodo no GND; LCD 16x2 I2C em D21 (SDA) e D22 (SCL); buzzer em D26; botão “Ciente” em D27. | Os pinos do Wokwi são os mesmos GPIOs do sketch. O LCD escreve o nível por extenso: quem não distingue as cores também entende. |
| 1:10–1:35 | Play. Logo em seguida, clicar no DHT22 e pôr 24 °C / 40 %. No Serial, a linha do HMAC `2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0` e “Vetores de calor OK”. Se a primeira medição sair antes do NTP, aparece “Aguardando NTP; LED local ativo” com o LED já aceso. | O ESP32 confere sozinho o vetor do contrato e os quatro casos de calor. Se algo falhar, ele não liga. Sem relógio, o alerta local funciona e a leitura não sai com data inventada. |
| 1:35–2:00 | Passo 1: LED verde e LCD `Normal`, Serial com “nivel Normal” e “Leitura publicada”. Trocar para o painel ou o app: sala 3 em Normal, índice 23,5 °C. | Esse número é o do Wokwi, recebido pela API real, na sala que o professor já acompanha. |
| 2:00–2:50 | Passos 2, 3 e 4, esperando “Leitura publicada” entre eles: amarelo, vermelho fixo e vermelho piscando, com o LCD acompanhando. No passo 4 o buzzer bipa; apertar “Ciente”: o bip para e o LCD mostra `Perigo (ciente)`. Depois, atualizar o painel ou o app: sala 3 em Perigo e a previsão de 2 h. Mostrar a última linha de `/ambientes/sala-03/leituras` com `"suspeita": false`. | Cada nível tem uma ação sugerida: água e janelas; pausa ou troca de sala; tirar a turma. O “Ciente” cala o buzzer, mas o LED segue piscando até a sala esfriar. A previsão usa esse histórico, não um sensor paralelo. |
| 2:50–3:15 | Voltar o DHT22 para 33 °C / 55 % (vermelho fixo). No desktop ou no painel, a coordenação vê a sala 3 e registra a ação tomada. | O aparelho avisa na porta; quem decide e registra é a coordenação. |
| 3:15–3:30 | Fechar com as limitações: chave de demonstração, visível no link do Wokwi, revogada depois da avaliação; QoS 0 pode perder uma leitura e a API mostra “sem leitura recente”. | O sensor do Wokwi é simulado; um aparelho real precisaria de calibração com termo-higrômetro de referência. |

Se o broker público ou o NTP falharem no dia, “Leitura publicada” não aparece e o vídeo não prova a integração. Gravar com antecedência e guardar os prints do ensaio como plano B.
