# Vídeo de Physical Computing — até 4 minutos

Projeto no Wokwi: https://wokwi.com/projects/481082666157493249

Meta: **3 min 30 s** (o limite do enunciado é 4 min; a folga evita estouro). O enunciado pede três coisas, nesta ordem: apresentar brevemente a solução, explicar como o ESP32 entrou no projeto e demonstrar o funcionamento no Wokwi. O circuito mede a Sala 7 do Termômetro da Sala: quatro DHT22 no mesmo ESP32, com o mesmo tópico MQTT, o mesmo contrato de assinatura (HMAC) e os mesmos identificadores `demo-sala-07-*` que a API usa. Não é um projeto à parte.

**O vídeo se grava só com a tela do Wokwi (circuito e Serial lado a lado) e a narração.** Não depende da API, do banco nem do desktop. A prova de funcionamento é o Serial: vetor HMAC conferido, quatro linhas por ciclo e quatro “Leitura publicada”. Não mostrar `secrets.h` aberto nem a chave em zoom.

## Antes de gravar

1. Abrir o link do Wokwi acima, copiar `secrets.h` do `secrets.exemplo.h` se a aba estiver vazia, e deixar a janela com o circuito e o Serial (115200) lado a lado. Simulação parada.
2. Estado de partida: o diagrama abre com a janela em **35 °C / 60 %** e fundo, porta e centro em **24 °C / 60 %**. É o que aparece no play (janela em Perigo, índice 45,1 °C, LED vermelho piscando).
3. Ensaiar uma vez inteira com cronômetro. Cada mudança de sensor leva cerca de 10 s até as quatro “Leitura publicada”; não esperar o ciclo de 30 s do LCD (troca de página a cada 3 s).
4. Se o NTP ou o broker público demorarem no dia, a linha “Aguardando NTP; alerta local ativo” aparece e o alerta local segue funcionando. Esperar “Leitura publicada” antes de seguir; se não vier, regravar mais tarde.
5. Opcional, só se a API Demo estiver de pé (ver `api/README.md`): 10 a 15 s finais com `GET /ambientes/sala-07/mapa` ou o desktop em modo API mostrando os quatro pontos. Fora isso, o vídeo não pode depender disso.
6. Publicar no YouTube como não listado e abrir o link numa aba anônima antes de enviar.

## Valores reais observados no Wokwi (18/11)

Fundo, porta e centro ficam nos valores de cada passo; só muda o que o roteiro diz.

| Situação | DHT22 | Índice | Nível | LED |
|---|---|---|---|---|
| Partida do diagrama | janela 35 °C / 60 % | 45,1 °C | Perigo | vermelho piscando |
| Todos em Normal | 24 °C / 40 % | 23,5 °C | Normal | verde |
| Passo 2 | janela 29 °C / 50 % | 29,7 °C | Atenção | amarelo fixo |
| Passo 3 | janela 33 °C / 55 % | 37,8 °C | Cuidado extremo | vermelho fixo |
| Passo 4 | janela 37 °C / 55 % | 48,5 °C | Perigo | vermelho piscando (500 ms), com bip |
| Frio | fundo 15 °C / 60 %, demais 22 °C / 60 % | 14,1 °C (fundo), 21,8 °C | Frio no fundo | amarelo |

Os ensaios completos (inclusive DHT22 inválido e “baixar todos, voltar ao perigo”) estão no `esp32/README.md`, seção Ensaio, e ficam fora do vídeo.

## Roteiro

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:30 | Tela de abertura: nome do projeto, equipe e a Sala 7. | Somos a equipe do Termômetro da Sala, da CitizenTech. A sala de aula esquenta, a escola só reage quando alguém passa mal, e o calor não é igual em toda a sala. A solução mede a temperatura e a umidade e calcula o índice de calor. O professor recebe o alerta, a coordenação vê o mapa da sala, decide e registra a ação. |
| 0:30–1:00 | Tela do Wokwi com o circuito parado; apontar os quatro DHT22 e o ESP32. | O ESP32 é o sensor da Sala 7 dentro dessa solução. São quatro DHT22: janela, fundo, porta e centro, porque o calor varia de um canto a outro. Cada ponto publica a sua leitura por MQTT, assinada com HMAC, no mesmo tópico e no mesmo formato que a nossa API confere e grava. Os identificadores demo-sala-07 são os mesmos da API. O circuito não é um projeto paralelo: é a ponta física da solução. |
| 1:00–1:25 | Apontar cada peça: DHT22 em D15, D32, D33 e D25; LEDs verde, amarelo e vermelho em D18, D19 e D4; LCD no D21 e no D22; buzzer no D26; botão Ciente no D27. | Janela no D15, fundo no D32, porta no D33 e centro no D25. Os LEDs ficam no D18, D19 e D4. O LCD 16 por 2 usa o D21 e o D22, o buzzer o D26 e o botão Ciente o D27. No local, o LED mostra o alerta mais grave e o LCD escreve o ponto e o nível. Sem internet, o aviso continua. |
| 1:25–1:55 | Play. No Serial: o vetor HMAC (`2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0`) e “Vetores de calor OK”; depois as quatro linhas com rótulo, ID, temperatura, umidade, índice e nível, e as quatro “Leitura publicada”. Janela em Perigo, 35 °C, índice 45,1 °C, LED vermelho piscando. | Ao ligar, o ESP32 confere o vetor de teste do contrato, os limites, o LCD e os extremos. Se algo falhar, ele não segue. Aqui está: Vetores de calor OK. A cada 10 segundos saem quatro linhas, uma por ponto, e quatro publicações. A janela começa em 35 graus, índice 45,1, Perigo. |
| 1:55–2:15 | Pôr os quatro DHT22 em 24 °C / 40 %. Esperar as quatro linhas: `24.0 C, 40%, indice 23.5 C, Normal`. LED verde. | Coloco os quatro em 24 graus e 40 por cento. Índice 23,5, Normal, LED verde. |
| 2:15–3:00 | Passos 2, 3 e 4, só na janela, esperando as quatro “Leitura publicada” em cada um: 29 °C / 50 % (amarelo fixo), 33 °C / 55 % (vermelho fixo), 37 °C / 55 % (vermelho piscando, bip). No passo 4, apertar Ciente: o bip para, o Serial mostra “Professor ciente do Perigo; buzzer silenciado ate o nivel baixar” e o LCD mostra `Perigo (ciente)`. | Subo só a janela. A 29 graus, Atenção, amarelo, índice 29,7. A 33, Cuidado extremo, vermelho fixo, índice 37,8. A 37 graus e 55 por cento, Perigo, índice 48,5, vermelho piscando e bip. O professor aperta Ciente: o bip para, o LCD mostra Perigo ciente, o LED segue piscando e a leitura publicada não muda. |
| 3:00–3:15 | Voltar os quatro para o frio: fundo 15 °C / 60 %, demais 22 °C / 60 %. LED amarelo; LCD na página `Frio fundo`. | O aparelho também avisa o frio. Com o fundo a 15 graus, o LED fica amarelo e o LCD mostra Frio no fundo. |
| 3:15–3:30 | Voltar ao Serial com as quatro “Leitura publicada”; mostrar o link do Wokwi na tela. Se a API estiver de pé, 10 s do mapa da Sala 7 com os quatro pontos. | Resumindo: o ESP32 mede os quatro pontos da Sala 7, avisa na hora no local e envia cada leitura assinada para a API da solução, onde a coordenação vê o mapa. O projeto está publicado no Wokwi, com o link na entrega. Obrigado. |

Limites para citar se sobrar tempo (ou deixar de fora): MQTT com QoS 0 pode perder um lote; o simulador usa sensor simulado, que em campo pede calibração; os identificadores de demonstração são revogados depois da avaliação.

Se o broker público ou o NTP falharem no dia, “Leitura publicada” não aparece e o vídeo não prova a publicação. Gravar com antecedência e guardar os prints do Serial como plano B.
