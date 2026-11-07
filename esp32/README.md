# Termômetro da Sala — ESP32

O mesmo sensor avisa calor no verão (escolas) e frio no inverno (dormitórios e abrigos).

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Quatro sensores no mesmo ESP32 medem janela, fundo, porta e centro; LCD e LEDs avisam localmente. A coordenacao acompanha o mapa, decide e registra a acao; o professor acompanha pelo app |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Links da entrega

| Item | Link |
|---|---|
| Projeto no Wokwi (público) | [preencher] |
| Vídeo demonstrativo (até 4 min, não listado) | [preencher] |
| Integrantes | [`integrantes.txt`](integrantes.txt) |

## Como o circuito entra na solucao

Um ESP32 mede quatro DHT22: janela, fundo, porta e centro. Cada ponto publica
um JSON separado a cada 10 s no mesmo topico do [contrato](../CONTRATO.md).
O campo `dispositivo` ja identifica o sensor na API e no mapa; nao foi criado
um campo novo. HMAC-SHA256, formato canonico, janela de tempo e antirreenvio
continuam iguais. Cada sensor tem sua propria ordem, mesmo quando os quatro
usam o mesmo segundo Unix. Mensagens antigas continuam aceitas.

O exemplo usa Sala 7 e os quatro dispositivos posicionados da M1. Na API em
ambiente `Demo`, execute **antes de ligar o circuito**:

```powershell
Invoke-RestMethod -Method Post http://localhost:5080/demo/mapa
```

Isso cria a planta 6 x 4 m, sensores e lugares; tambem insere leituras
simuladas. Aguarde as primeiras publicacoes para ver os valores do circuito
em `GET /ambientes/sala-07/mapa` e `GET /ambientes/sala-07/leituras`.
Depois de receber leituras do circuito, repetir o POST retorna 409 para
preservar os dados. Fora de Demo, os dispositivos precisam estar previamente
cadastrados no ambiente com IDs, posicoes e segredo correspondentes.

| Ponto | `dispositivo` do exemplo | Posicao (m) | GPIO | Inicial |
|---|---|---|---|---|
| janela | demo-sala-07-janela | (0, 2) | 15 | 35 C / 60% |
| fundo | demo-sala-07-fundo | (6, 4) | 32 | 24 C / 60% |
| porta | demo-sala-07-porta | (6, 0) | 33 | 24 C / 60% |
| centro | demo-sala-07-centro | (3, 2) | 25 | 24 C / 60% |

## Componentes e pinagem

| Quantidade | Componente | Ligacao |
|---|---|---|
| 1 | ESP32 DevKit V1 | Wi-Fi, calculo e publicacao |
| 4 | DHT22 | VCC em 3V3, GND em GND; SDA nos GPIOs da tabela acima |
| 4 | Resistor 10 kOhm | Um pull-up entre cada SDA e 3V3 |
| 3 | LEDs e resistores 220 Ohm | Verde D18, amarelo D19, vermelho D4; resistor em serie no anodo, catodo no GND |
| 1 | LCD 16x2 I2C, 0x27 | VCC VIN, GND, SDA D21, SCL D22 |
| 1 | Buzzer | positivo D26, negativo GND |
| 1 | Botao Ciente | 1.l D27 (pull-up interno), 2.l GND |

Bibliotecas em `libraries.txt`: DHT sensor library, Adafruit Unified Sensor,
PubSubClient e LiquidCrystal I2C. Wi-Fi, FreeRTOS, NTP e HMAC vem do core ESP32.
Os quatro GPIOs de dados sao distintos e nao conflitam com LCD, LEDs, buzzer
ou botao. Confira sem abrir o Wokwi: `python esp32/conferir_circuito.py`.

## LCD e alertas locais

O LCD troca de pagina a cada 3 s. Para cada ponto mostra rotulo e temperatura,
depois rotulo e indice (`I`), mantendo o nivel por extenso na segunda linha.
Depois mostra `Quente <ponto>` e `Frio <ponto>` com as temperaturas extremas.
O ciclo completo leva 30 s. Empates usam a ordem janela, fundo, porta, centro.
Sensores invalidos aparecem como `Sem leitura` e nao participam dos extremos;
sem nenhum valido, aparece `Sem extremos`. A medicao ocorre a cada 10 s.

O alerta coletivo prioriza perigo, cuidado, atencao, frio e normal, nessa ordem.
Assim, um ponto normal nao apaga um aviso de frio ou calor de outro ponto.

| Indice | Nivel | LED | Buzzer |
|---|---|---|---|
| < 18 C | FRIO | amarelo piscando, 500 ms | mudo |
| >= 18 e < 27 C | Normal | verde | mudo |
| >= 27 e < 32 C | Atencao | amarelo fixo | mudo |
| >= 32 e < 41 C | Cuidado extremo | vermelho fixo | mudo |
| >= 41 C | Perigo | vermelho piscando, 500 ms | bip de 150 ms a cada 2 s |

Sem sensores validos, LEDs apagados; apagado nao significa normal.
O botao Ciente silencia o perigo coletivo e as paginas dos pontos em perigo
mostram `Perigo (ciente)`. O reconhecimento e local; zera quando nenhum ponto
esta em perigo. O indice usa a mesma formula NOAA/Rothfusz da API e as mesmas
entradas arredondadas. Sem NTP, os alertas funcionam e nada e publicado.
A tarefa MQTT fica separada para DNS ou broker lentos nao travarem os alertas.

## Abrir no Wokwi

1. Criar projeto ESP32 DevKit V1 e copiar `sketch.ino`, `diagram.json` e
   `libraries.txt` para abas de mesmo nome.
2. Criar `secrets.h` copiando **o exemplo atualizado** `secrets.exemplo.h`.
   `DISPOSITIVO` identifica a conexao MQTT; `DISPOSITIVOS` contem os quatro IDs
   assinados, na ordem da tabela. O segredo e publico e so de demonstracao.
3. Preparar a API Demo pelo POST acima, iniciar a simulacao e abrir Serial
   (115200). O arranque confere o vetor HMAC publico do contrato, limites,
   LCD, extremos, prioridade de alertas e falhas de sensores com asserts.
   Esperado: `Vetores de calor OK`. Um assert falho interrompe o arranque.
4. A cada 10 s, conferir quatro linhas com rotulo, ID, temperatura, umidade,
   indice e nivel; depois do NTP, quatro `Leitura publicada`.
5. Conferir os quatro pontos no mapa e o LCD durante um ciclo completo.

## Ensaio

| Teste | Esperado | Observado |
|---|---|---|
| Inicial, janela 35 C, demais 24 C | quente janela, frio fundo (empate); quatro IDs no mapa | [preencher] |
| Fundo 15 C, demais 22 C, todos 60% | frio fundo; LED amarelo piscando; pagina FRIO | [preencher] |
| Todos 24 C / 40% | indice 23,5 C, normal; verde | [preencher] |
| Um ponto 37 C / 55% | indice 48,5 C; perigo, vermelho piscando e bip | [preencher] |
| Ciente durante perigo; baixar todos, voltar ao perigo | silencia; novo perigo volta a bipar | [preencher] |
| Um DHT22 invalido | pagina Sem leitura; demais seguem publicados e nos extremos | [preencher] |
| Sem Wi-Fi ou NTP | LCD e alertas seguem; sem NTP nao publica | [preencher] |
| Repetir mensagem assinada | API recusa reenvio so daquele ID | [preencher] |

Para repetir o cenario da madrugada, altere os quatro DHT22 no simulador;
nao repita a semeadura sobre leituras do circuito. Saltos acima de 5 C sao
marcados como suspeitos pela API. Roteiro anterior: [ROTEIRO-VIDEO.md](ROTEIRO-VIDEO.md);
para quatro pontos, use o ensaio desta pagina.

## Limitacoes

- MQTT QoS 0, sem retencao: apenas o lote mais recente dos quatro sensores
  espera a conexao; quedas podem perder leituras. A API indica falta de leitura
  recente depois de 15 minutos.
- Broker e topico publicos, sem TLS: HMAC garante integridade, sem sigilo ou
  garantia de disponibilidade. A chave publica do exemplo so serve para Demo.
  Revogar os quatro IDs com `POST /dispositivos/{id}/revogar` apos a avaliacao.
- Sensores simulados; aparelho real requer calibracao com termo-higrometro
  de referencia. Previsao continua com as limitacoes descritas no contrato.
- Ciente vale para o perigo coletivo, sem registro remoto de quem apertou.
- LCD em VIN (5 V) no Wokwi; hardware com pull-ups I2C em 5 V requer conversor
  de nivel para os GPIOs de 3,3 V.
