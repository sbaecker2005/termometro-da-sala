# Termômetro da Sala: roteiro do pitch

Vídeo de até 4 minutos, enviado em Testing. Tempo total do roteiro: 3min55s.

Antes de gravar: confira os números marcados com **[A CONFERIR]** na seção 7 do `DOCUMENTO.md`. Se um número mudar lá, ele muda aqui e no slide 6.

| Tempo | Bloco | Slide |
|---|---|---:|
| 0:00 a 0:25 | Abertura | 1 e 2 |
| 0:25 a 0:55 | Fala da pessoa entrevistada | 3 |
| 0:55 a 2:45 | Demonstração | 4 e 5 |
| 2:45 a 3:30 | Impacto com números | 6 |
| 3:30 a 3:55 | Encerramento | 7 e 8 |

## Abertura (0:00 a 0:25)

> Somos o Termômetro da Sala. Eu sou Samuel Baecker, RM [preencher]. [Demais integrantes, com nome e RM.]
>
> Nas tardes quentes, a sala de aula passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal.

## Fala da pessoa entrevistada (0:25 a 0:55)

**[Espaço para a fala de 20 a 25 segundos: nome e função de [pessoa entrevistada], da escola [nome da escola], [bairro].]**

Peça autorização para usar a voz ou a imagem. Se a pessoa preferir, leia a frase dela com o crédito na tela. Sem alunos no vídeo. Trecho sugerido: em que horário a sala esquenta e o que acontece hoje quando isso ocorre.

> [Uma frase ligando a fala ao problema.] Hoje, essa informação não existe em lugar nenhum. Vou mostrar como ela passa a existir.

## Demonstração no modo demonstração (0:55 a 2:45)

Gravar entre 12h e 17h, com a API no ambiente `Demo`. Deixe o Wokwi publicando antes de começar e rode `POST /demo/semear?ambienteId=sala-03`.

| Tempo | Tela | O que mostrar | Fala |
|---|---|---|---|
| 0:55 a 1:15 | Wokwi | O ESP32 na porta da sala, em 33 °C, com o LED vermelho e a publicação no Serial | "Este sensor fica na porta da sala. Ele mede o calor, acende a cor do nível mesmo sem internet e envia a leitura assinada." |
| 1:15 a 1:35 | App, lista de salas | As salas com nível e índice, e a sala 3 em Cuidado extremo | "O professor vê as salas pelo app. As cores sempre vêm com o nome do nível." |
| 1:35 a 2:00 | App, sala 3 | Nível de agora, previsão para 2 h, ação sugerida e a etiqueta DEMONSTRAÇÃO. Rua prevista 34 + delta 4 = 38 | "O modelo, treinado com dados reais do INMET, prevê a rua daqui a 2 horas. Somamos o quanto esta sala costuma passar da rua e temos a previsão da sala. O erro médio foi 27 % menor que supor que nada vai mudar. Esta etiqueta avisa que as leituras são de demonstração." |
| 2:00 a 2:15 | Wokwi + terminal | Subir o slider em passos de até 4 °C, esperando 10 s entre eles, até a sala virar Perigo no terminal | "A sala esquentou. A coordenação vê isso em todas as salas ao mesmo tempo." |
| 2:15 a 2:30 | Terminal da coordenação | Registrar a ação "tirar a turma da sala" | "Quem decide é a coordenação. A ação fica registrada, e é com esse registro que medimos o resultado." |
| 2:30 a 2:45 | App, nova ocorrência | Descrição + foto da sala vazia pela câmera, e o painel web por 2 s | "O professor registra a ocorrência com foto da sala, sem pessoas. Nenhum dado de aluno entra no sistema." |

Com 55 % de umidade, a sala já vira Perigo no passo de 37 °C. Nesse caso, pare o slider ali. Arrastar 12 °C de uma vez gera uma leitura suspeita e atrapalha a cena.

## Impacto (2:45 a 3:30)

> A rede estadual de São Paulo tem cerca de 3,5 milhões de alunos **[A CONFERIR]**. Com 35 alunos por turma, são 100 mil turmas. Se três em cada quatro estudam sem climatização, que é a nossa premissa, são 2,6 milhões de alunos **[A CONFERIR]**.
>
> Nos dados do INMET para a capital, de 2022 a 2025, a rua chegou a 34 graus em 21 dias, e 2023 teve 10 deles. Isso é na rua. Dentro da sala, ninguém mede.
>
> Nosso indicador é simples: aulas em Perigo sem ação registrada. A meta é zero.

## Encerramento (3:30 a 3:55)

> O Termômetro da Sala não climatiza a sala. Ele faz uma coisa que hoje ninguém faz: mostra o calor antes de alguém passar mal e registra o que foi feito. Esse histórico vira a prova para pedir a obra que a escola precisa.
>
> Somos o Termômetro da Sala. Agradecemos a atenção.

## Texto dos slides

### Slide 1: Termômetro da Sala
- Samuel Baecker - RM [preencher]
- [Demais integrantes - RM]
- O calor da sala de aula, medido antes de alguém passar mal

### Slide 2: O problema
- Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça
- A escola só reage quando alguém passa mal

### Slide 3: Quem vive isso
- "[Frase da pessoa entrevistada]"
- [Nome], [função], [escola], [bairro]

### Slide 4: Como funciona
- Medir: sensor na porta da sala, com LED que funciona sem internet
- Prever: nível da sala para daqui a 2 h, com dados reais do INMET
- Agir: a coordenação decide e registra

### Slide 5: A previsão
- Rua prevista 34 + delta da sala 4 = 38: Cuidado extremo
- Erro médio de 1,05 °C, 27 % menor que "daqui a 2 h igual a agora"
- Nenhum dado de aluno

### Slide 6: O impacto
- 3,5 milhões de alunos ÷ 35 = 100 mil turmas (premissa; Seduc-SP e INEP)
- 75 % sem climatização (premissa) = 2,6 milhões de alunos
- 21 dias a 34 °C ou mais na capital, 2022 a 2025 (INMET A701)

### Slide 7: A arquitetura
- Motivação: nenhuma aula em Perigo sem ação (ODS 3.d, 4.a, 11.b, 13.1)
- Negócio: medir, prever, alertar, agir, registrar
- Aplicação e tecnologia: API, app, terminal, painel, ESP32, Windows Server

### Slide 8: O que muda
- Indicador: aulas em Perigo sem ação registrada. Meta: zero
- O histórico por sala mostra onde climatizar primeiro
- Termômetro da Sala
