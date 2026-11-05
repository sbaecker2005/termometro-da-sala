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

## Abrir no Wokwi

1. Criar um projeto Arduino ESP32 DevKit no Wokwi. Colar `sketch.ino`,
   `diagram.json` e `libraries.txt` nas abas correspondentes.
2. Criar a aba `secrets.h` copiando o conteúdo de `secrets.exemplo.h`.
   Não versionar `secrets.h`. O exemplo contém a chave pública do vetor de teste.
3. Iniciar a simulação e abrir o Serial (115200 baud). Deve aparecer o HMAC
   `2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0`
   e “Vetores de calor OK”. Falha nos vetores interrompe o início por assert.
4. Após 10 s, conferir temperatura 33.0 °C/55% e LED vermelho fixo.
   Aguardar NTP e “Leitura publicada”. Wi-Fi: `Wokwi-GUEST`, sem senha.
5. Na API ou cliente MQTT da equipe, assinar
   `fiap-gs/termometro-da-sala/leituras` em `broker.hivemq.com:1883`.
   Comparar JSON, HMAC e horário com [CONTRATO.md](../CONTRATO.md).

Dependências listadas para o gerenciador do Wokwi: DHT sensor library,
Adafruit Unified Sensor e PubSubClient. Wi-Fi, FreeRTOS, NTP e mbedTLS vêm
do core Arduino ESP32. Nenhuma biblioteca extra de JSON ou criptografia.

## Circuito e LEDs

ESP32 DevKit V1, DHT22 em 3,3 V, DATA GPIO 15 com pull-up de 10 kΩ;
LEDs em GPIO 18 (verde), 19 (amarelo), 21 (vermelho), cada um com resistor
de 220 Ω em série e cátodo no GND. DHT22 inicia em 33 °C e 55%.

| Índice de calor | LED | Significado |
|---|---|---|
| <27 °C | verde | Normal |
| ≥27 e <32 °C | amarelo | Atenção: água, janelas, ventiladores |
| ≥32 e <41 °C | vermelho fixo | Cuidado extremo: pausa/troca de sala/adiar Educação Física |
| ≥41 °C | vermelho piscando a cada 500 ms | Perigo: tirar a turma da sala |

Todos apagados: ainda não mediu ou sensor inválido (ver Serial), não é Normal.
Leitura e LED a cada 10 s, mesmo sem Wi-Fi. Conexões MQTT ficam em tarefa
separada para não bloquear medição nem pisca. Sem NTP, só o alerta local funciona.
O intervalo NTP é 60 s, configurado antes de iniciar a sincronização.

## Ensaio e limitações

- Testar os quatro pares temperatura/umidade do contrato; os quatro asserts
  também rodam ao ligar. Índices e níveis devem coincidir dentro de 0,1 °C.
- Para a gravação, começar em 33 °C/55%; aumentar 33 → 37 → 41, esperando
  uma publicação entre passos. Salto de 12 °C só será evidência de “suspeita”
  se a API implementar a plausibilidade.
- Desligar o Wi-Fi e variar o sensor: o LED continua mudando e, em Perigo,
  piscando. Reconectar e verificar retomada das leituras.
- Manter ligado por 15 min e conferir que a API não registra `atrasada`.
  Conferir também rejeição de assinatura alterada, replay e dispositivo revogado.
- PubSubClient publica em **QoS 0**, sem retenção nem histórico de retransmissão;
  só a leitura mais recente aguarda conexão. Uma queda pode perder leituras;
  a API deve mostrar “sem leitura recente”. QoS 1 exigiria outro cliente.
- O broker e tópico são públicos, sem TLS: HMAC protege integridade/autoria,
  não sigilo nem disponibilidade. A chave do Wokwi é de **demonstração**,
  visível a quem tem o link, e o dispositivo será **revogado após a avaliação**.
  Nunca usar chave real neste projeto público.
- O sensor do Wokwi é simulado. O histórico de 3 h vem do gerador sintético
  descrito no contrato, não de medições reais. O delta da previsão assume
  diferença constante nas próximas 2 h e pode superestimar no fim da tarde.
  Equipamento real exige validação/calibração com termo-higrômetro de referência.

Entregar o link do projeto, vídeo integrado à solução (até 4 min) e
`integrantes.txt`. Preencher o RM de Samuel Baecker, os demais integrantes,
o bairro, a entrevista e o link do repositório antes do envio.
O sketch compila com arduino-cli no fqbn `esp32:esp32:esp32` (core 3.3.12) e as bibliotecas de `libraries.txt`. Serial, LEDs e MQTT ainda dependem do ensaio no Wokwi.
