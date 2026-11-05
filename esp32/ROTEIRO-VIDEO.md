# Vídeo de Physical Computing — até 4 minutos

Meta: **3 min 50 s**. O circuito é o sensor da porta da sala na solução Termômetro da Sala, não um projeto à parte. Grave o Wokwi, o Serial Monitor e, no trecho da integração, a sala 3 no app (ou o JSON no tópico MQTT ao lado do contrato). Não mostre `secrets.h` aberto nem a chave em zoom.

Antes de gravar: simulação parada, DHT22 em 33 °C e 55 %, Serial em 115200, app aberto na sala 3 (modo simulado ou API). Se a API estiver no ar, o assinante MQTT em `broker.hivemq.com:1883`, tópico `fiap-gs/termometro-da-sala/leituras`.

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:25 | Nome do projeto, equipe e a frase do problema: a sala esquenta e a escola só reage quando alguém passa mal. Aponte professor (app) e coordenação (desktop). | O ESP32 fica na porta dessa mesma sala. Ele não mede a rua; a rua vem do INMET. |
| 0:25–0:55 | Diagrama: DHT22, três LEDs e o fluxo numa frase (sensor → LED local → leitura assinada → API → previsão de 2 h no app). | Sem internet o LED continua. A API é quem oficializa o índice e a previsão. |
| 0:55–1:25 | Ligações: DHT22 em 3,3 V, DATA em D15 com pull-up de 10 kΩ; verde D18, amarelo D19 e vermelho D21, cada um com 220 Ω e cátodo no GND. Atributos 33 °C / 55 %. | Os pinos são os do DevKit no Wokwi (`D15`, `D18`, `D19`, `D21`), os mesmos GPIOs do sketch. |
| 1:25–2:05 | Dar play e o Serial. Esperar a linha do HMAC `2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0` e “Vetores de calor OK”. | É o vetor do contrato (`sala-03`, 33.4 °C, 58 %, o segredo de teste). Se falhar, o assert para o arranque. |
| 2:05–2:35 | Por volta de 10 s: temperatura, umidade, índice e nível no Serial; LED vermelho fixo (cuidado). Depois “Leitura publicada”. | Cada envio bem-sucedido imprime “Leitura publicada”. Sem NTP o LED acende e a leitura não sai. |
| 2:35–3:20 | O JSON publicado (ambiente `sala-03`, dispositivo `esp32-sala-03`) e a mesma sala no app: nível, previsão de 2 h e ocorrências. Subir o slider 33 → 37 → 41, esperando uma publicação entre os passos, e mostrar o LED indo ao vermelho piscando. | A leitura entra na sala que o professor já vê. O delta da previsão usa esse histórico, não um sensor paralelo. |
| 3:20–3:50 | Mostre no Serial, antes do NTP, “Aguardando NTP; LED local ativo” com o LED já aceso. Se o NTP chegar antes dos 10 s, troque o SSID do `WiFi.begin` para um nome inválido, reinicie e varie o DHT22: o LED muda e não aparece “Leitura publicada”. Feche com a limitação: chave de demonstração, visível no link do Wokwi, revogada depois da avaliação; QoS 0 pode perder leitura. | O aparelho da porta segue útil sozinho. A decisão continua com a coordenação. |
