# Termômetro da Sala — ESP32

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
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

## Como o circuito entra na solução

O ESP32 é o sensor da porta da sala. Não é um projeto à parte: a leitura dele
é a mesma que o professor vê no app e que a coordenação usa no desktop para
decidir e registrar a ação.

```text
DHT22 ──> ESP32 ──> na porta: LED (verde / amarelo / vermelho fixo / vermelho piscando),
            │        LCD com índice e nível por extenso, buzzer em Perigo e botão "Ciente"
            │        funciona mesmo sem internet
            │
            └──> leitura assinada (HMAC-SHA256) ──MQTT──> broker.hivemq.com:1883
                                                              │ tópico fiap-gs/termometro-da-sala/leituras
                                                              v
                     API (ASP.NET Core): confere assinatura, ordem, horário e faixa;
                     recalcula o índice e grava a leitura na sala-03
                                                              │
                         ┌────────────────────┬───────────────┴──────────────┐
                         v                    v                              v
             previsão de 2 h           app do professor            desktop e painel web
          (rua do INMET + delta       (nível e previsão da        (coordenação vê todas as
             desta sala)                   sala 3)                 salas e registra a ação)
```

- O LED dá o aviso imediato na porta, mesmo com a rede fora. O LCD escreve o
  nível, para quem não distingue as cores, e o buzzer chama a atenção em Perigo
  até o professor apertar "Ciente".
- A API é a fonte oficial: recalcula o índice com a mesma fórmula e recusa
  leitura com assinatura errada, reenviada, atrasada ou de dispositivo revogado
  ([CONTRATO.md](../CONTRATO.md), seção 2).
- O histórico da sala alimenta a previsão de 2 h que aparece no app e no desktop.

## Componentes

| Qtd. | Componente (Wokwi) | Função |
|---|---|---|
| 1 | ESP32 DevKit V1 (`wokwi-esp32-devkit-v1`) | Mede, calcula o nível, acende os LEDs e publica por Wi-Fi/MQTT |
| 1 | Sensor de temperatura e umidade DHT22 (`wokwi-dht22`) | Temperatura e umidade da sala |
| 1 | Resistor de 10 kΩ | Pull-up da linha de dados do DHT22 |
| 3 | LEDs: verde, amarelo e vermelho (`wokwi-led`) | Nível de calor na porta da sala |
| 3 | Resistores de 220 Ω | Limitam a corrente de cada LED |
| 1 | LCD 16x2 com módulo I2C (`wokwi-lcd1602`, `pins: i2c`, endereço 0x27) | Índice e nível por extenso; não depende da cor |
| 1 | Buzzer (`wokwi-buzzer`) | Bip curto a cada 2 s em Perigo |
| 1 | Botão "Ciente" (`wokwi-pushbutton`) | O professor confirma que viu o Perigo e o buzzer silencia |

Bibliotecas (`libraries.txt`): DHT sensor library, Adafruit Unified Sensor,
PubSubClient e LiquidCrystal I2C. Wi-Fi, FreeRTOS, NTP, I2C (Wire), `tone()` e
mbedTLS (HMAC) vêm do core Arduino ESP32.

## Pinagem

| Componente | Pino do componente | Pino do ESP32 (Wokwi) | GPIO no sketch |
|---|---|---|---|
| DHT22 | VCC | 3V3 | — |
| DHT22 | SDA (dados) | D15, com pull-up de 10 kΩ para 3V3 | 15 (`PIN_DHT`) |
| DHT22 | GND | GND | — |
| LED verde | ânodo, via 220 Ω | D18 | 18 (`VERDE`) |
| LED amarelo | ânodo, via 220 Ω | D19 | 19 (`AMARELO`) |
| LED vermelho | ânodo, via 220 Ω | D4 | 4 (`VERMELHO`) |
| LEDs | cátodo | GND | — |
| LCD 16x2 I2C | VCC | VIN (5 V) | — |
| LCD 16x2 I2C | SDA | D21 | 21 (I2C padrão do `Wire`) |
| LCD 16x2 I2C | SCL | D22 | 22 (I2C padrão do `Wire`) |
| LCD 16x2 I2C | GND | GND | — |
| Buzzer | + (pino 2) | D26 | 26 (`BUZZER`) |
| Buzzer | − (pino 1) | GND | — |
| Botão "Ciente" | 1.l | D27, com pull-up interno | 27 (`BOTAO`) |
| Botão "Ciente" | 2.l | GND | — |

O vermelho saiu do D21 porque o LCD usa o I2C padrão do ESP32 (D21 e D22).
O DHT22 começa em 33 °C e 55 % (`diagram.json`); durante a simulação, os
valores mudam clicando no sensor.

Para conferir o circuito sem abrir o Wokwi: `python esp32/conferir_circuito.py`
(na raiz do repositório). O script lê o `diagram.json`, o `sketch.ino` e o
`libraries.txt` e confere peças, pinos, cada GPIO do sketch ligado à peça
certa, retorno pelo GND, ausência de curto entre GPIOs e as bibliotecas. A
compilação continua sendo feita pelo Wokwi.

## Níveis, LEDs, LCD e buzzer

| Índice de calor | Nível | LED | LCD (2ª linha) | Buzzer | Ação sugerida |
|---|---|---|---|---|---|
| < 27 °C | Normal | verde | `Normal` | mudo | nenhuma |
| ≥ 27 e < 32 °C | Atenção | amarelo | `Atencao` | mudo | água, janelas abertas, ventiladores |
| ≥ 32 e < 41 °C | Cuidado extremo | vermelho fixo | `Cuidado extremo` | mudo | pausa, troca de sala, adiar Educação Física |
| ≥ 41 °C | Perigo | vermelho piscando (500 ms) | `Perigo` | bip de 150 ms a cada 2 s | tirar a turma da sala |

A 1ª linha do LCD mostra a temperatura e o índice (`33.0C ind 37.8C`). O LCD
não tem acentos, por isso `Atencao`. Em Perigo, o botão "Ciente" silencia o
buzzer e o LCD passa a `Perigo (ciente)`; o LED continua piscando. Quando o nível sai de
Perigo, o "ciente" é zerado e um novo Perigo volta a bipar. O botão só vale em
Perigo e é local: não entra na leitura assinada nem muda o contrato com a API.

Todos apagados: ainda não mediu ou o sensor deu valor inválido (ver Serial);
apagado não é Normal. Sensor inválido: o LCD mostra `Sem leitura` /
`sensor invalido`; antes da primeira medição, `Medindo...`. O índice segue a
fórmula NOAA/Rothfusz, a mesma da API.
Medição e LED a cada 10 s, mesmo sem Wi-Fi. A conexão MQTT roda numa tarefa
FreeRTOS separada para não travar a medição nem o pisca. Sem NTP, só o alerta
local funciona: a leitura não é publicada com data inventada.

## Abrir no Wokwi

1. Criar um projeto ESP32 (DevKit V1) no Wokwi e colar `sketch.ino`,
   `diagram.json` e `libraries.txt` nas abas de mesmo nome.
2. Criar a aba `secrets.h` copiando `secrets.exemplo.h`. Ele traz só a chave
   pública de teste do dispositivo de demonstração; `secrets.h` não vai para o git.
3. Iniciar a simulação e abrir o Serial (115200). Ao ligar, o ESP32 imprime o
   HMAC `2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0` e
   “Vetores de calor OK”. Se um vetor falhar, o assert para o arranque.
4. A cada 10 s: temperatura, umidade, índice e nível no Serial, o LED do nível
   e o LCD atualizado (antes da primeira medição, `Medindo...`).
   Depois do NTP, “Leitura publicada”. Wi-Fi `Wokwi-GUEST`, sem senha.
5. Com a API rodando (`api/README.md`), a leitura aparece em
   `GET /ambientes/sala-03/leituras`, no painel web (`http://localhost:5080/`)
   e no app com `EXPO_PUBLIC_API_MOCK=0`.

## Ensaio

Roteiro do vídeo: [ROTEIRO-VIDEO.md](ROTEIRO-VIDEO.md). Prints em `evidencias/`.

| Teste | Esperado | Resultado observado |
|---|---|---|
| Ligar a simulação | HMAC do vetor e “Vetores de calor OK” no Serial; LCD `Termometro Sala` / `Medindo...` | [preencher] |
| 24 °C / 40 % | índice 23,5 °C, Normal, LED verde, LCD `24.0C ind 23.5C` / `Normal` | [preencher] |
| 29 °C / 50 % | índice 29,7 °C, Atenção, LED amarelo, LCD `Atencao` | [preencher] |
| 33 °C / 55 % | índice 37,8 °C, Cuidado extremo, LED vermelho fixo, LCD `Cuidado extremo`, buzzer mudo | [preencher] |
| 37 °C / 55 % | índice 48,5 °C, Perigo, LED vermelho piscando, LCD `Perigo`, bip a cada 2 s | [preencher] |
| Apertar "Ciente" em Perigo | buzzer para, LCD `Perigo (ciente)`, Serial “Professor ciente do Perigo”, LED segue piscando | [preencher] |
| Voltar a 33 °C e subir a 37 °C | o bip volta no novo Perigo | [preencher] |
| Depois do NTP | “Leitura publicada” a cada 10 s | [preencher] |
| API real | leitura na sala-03 com `"suspeita": false` e o mesmo índice | [preencher] |
| Sem Wi-Fi (SSID inválido no `WiFi.begin`) | LED segue o sensor e nada é publicado | [preencher] |

Os índices esperados foram calculados com a fórmula do contrato
(`ferramentas/indice_calor.py`). Passos de no máximo 5 °C evitam que a API
marque a leitura como `suspeita` (salto acima de 5 °C).

## Limitações

- MQTT em **QoS 0**, sem retenção: só a leitura mais recente espera a conexão.
  Uma queda pode perder leituras, e a API mostra “sem leitura recente”.
- Broker e tópico públicos, sem TLS: o HMAC garante integridade e autoria,
  não sigilo nem disponibilidade.
- A chave do Wokwi é de **demonstração** e fica visível a quem tem o link. O
  dispositivo `esp32-sala-03` será **revogado após a avaliação**
  (`POST /dispositivos/esp32-sala-03/revogar`). Nunca usar chave real aqui.
- O sensor do Wokwi é simulado e o histórico de 3 h vem do gerador sintético
  descrito no contrato. A previsão assume diferença constante entre sala e rua
  nas próximas 2 h e pode superestimar no fim da tarde. Um aparelho real
  precisa de calibração com termo-higrômetro de referência.
- O "Ciente" fica só no aparelho: a coordenação não vê quem apertou nem quando.
  Levar isso à API exige um campo novo no contrato (seção 2) e um novo vetor.
- No Wokwi o LCD é ligado ao VIN (5 V). Num aparelho real, um módulo I2C com
  pull-up para 5 V pede conversor de nível no SDA e no SCL do ESP32 (3,3 V).
