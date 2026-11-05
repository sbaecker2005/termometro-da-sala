# Termômetro da Sala: roteiro do pitch

Vídeo de até 4 minutos, enviado em Testing. Tempo total do roteiro: 3min30s, com 30 s de folga sobre o limite. O briefing pede a solução e as entregas que mais somaram: o bloco "Entregas" cumpre essa parte.

Cada bloco tem menos fala do que tempo: a 150 palavras por minuto, as falas da equipe somam cerca de 2min15s, mais os 20 s da pessoa entrevistada, e o resto é tela. Cronometre o ensaio. Se passar de 3min40s, corte primeiro na demonstração, nunca no impacto nem na abertura.

Antes de gravar: os números do impacto estão na seção 7 do `DOCUMENTO.md`. Se um número mudar lá, ele muda aqui e no slide 6.

| Tempo | Bloco | Slide |
|---|---|---:|
| 0:00 a 0:20 | Abertura | 1 e 2 |
| 0:20 a 0:45 | Fala da pessoa entrevistada | 3 |
| 0:45 a 2:15 | Demonstração | 4 e 5 |
| 2:15 a 2:30 | Entregas | telas gravadas |
| 2:30 a 3:10 | Impacto com números | 6 |
| 3:10 a 3:30 | Encerramento | 7 e 8 |

## Abertura (0:00 a 0:20)

O slide 1 fica na tela com o nome e o RM de cada integrante. Ninguém lê os RMs em voz alta.

> Somos o Termômetro da Sala, e a equipe está na tela. Nas tardes quentes, a sala de aula passa do limite seguro sem ninguém medir, e a escola só reage quando alguém passa mal.

## Fala da pessoa entrevistada (0:20 a 0:45)

**[Espaço para a fala de até 20 segundos: nome e função de [pessoa entrevistada], da escola [nome da escola], [bairro].]**

Peça autorização para usar a voz ou a imagem. Se a pessoa preferir, leia a frase dela com o crédito na tela. Sem alunos no vídeo. Trecho sugerido: em que horário a sala esquenta e o que acontece hoje quando isso ocorre.

> Hoje, essa informação não existe em lugar nenhum.

## Demonstração no modo demonstração (0:45 a 2:15)

Gravar entre 12h e 17h, com a API no ambiente `Demo`. Deixe o Wokwi publicando antes de começar e rode `POST /demo/semear?ambienteId=sala-03`. A foto da ocorrência é gravada antes e entra cortada.

| Tempo | Tela | O que mostrar | Fala |
|---|---|---|---|
| 0:45 a 1:00 | Wokwi | O ESP32 na porta da sala, em 33 °C, com o LED vermelho e a publicação no Serial | "Este sensor fica na porta da sala. Mede o calor, acende a cor do nível mesmo sem internet e envia a leitura assinada." |
| 1:00 a 1:10 | App, lista de salas | As salas com nível e índice, e a sala 3 em Cuidado extremo | "O professor vê as salas pelo app, sempre com a cor e o nome do nível." |
| 1:10 a 1:35 | App, sala 3 | Nível de agora, previsão para 2 h, ação sugerida e a etiqueta DEMONSTRAÇÃO. A conta rua prevista + delta da sala, com os números que a tela mostrar | "A rua daqui a 2 horas vem de um dia quente real do INMET, e somamos quanto esta sala costuma passar da rua. O modelo treinado com esses dados errou em média 0,96 grau no teste, contra 1,44 de supor que nada muda. A etiqueta avisa que as leituras são de demonstração." |
| 1:35 a 1:50 | Wokwi + terminal | Subir o slider em passos de até 4 °C, esperando 10 s entre eles, até a sala virar Perigo no terminal | "A sala esquentou, e a coordenação vê isso em todas as salas ao mesmo tempo." |
| 1:50 a 2:05 | Terminal da coordenação | Registrar a ação "tirar a turma da sala" | "Quem decide é a coordenação. A ação fica registrada, e é com esse registro que medimos o resultado." |
| 2:05 a 2:15 | App, nova ocorrência | Descrição + foto da sala vazia, e o painel web por 2 s | "O professor registra a ocorrência com foto da sala, sem pessoas. Nenhum dado de aluno entra no sistema." |

Com 55 % de umidade, a sala já vira Perigo no passo de 37 °C. Nesse caso, pare o slider ali. Arrastar 12 °C de uma vez gera uma leitura suspeita e atrapalha a cena.

A fala da sala 3 não cita a conta: ela depende da hora da gravação. Os números da tela são os que valem, e o slide 5 traz o exemplo 34 + 4 = 38.

## Entregas (2:15 a 2:30)

Cortes rápidos de 3 s cada, gravados antes, com o nome da disciplina na tela.

| Tela | Disciplina |
|---|---|
| Archi com a "Visão em quatro camadas" aberta e as visões Motivação, Negócio, Aplicação, Tecnologia e Implementação e migração na árvore do modelo (`arquitetura/termometro-da-sala.archimate`) | Arquitetura (TOGAF) |
| Notebook do modelo com a tabela de MAE de validação e teste | Machine Learning |
| Saída do `dotnet test` com os 40 testes passando | SOA e Testing |
| Windows Server com o IIS e o pool da API | Operating Systems |

> Por trás disso: o modelo ArchiMate com uma visão por camada, o notebook com dados reais do INMET, a API com 40 testes passando e o servidor Windows que publica tudo.

## Impacto (2:30 a 3:10)

> A rede estadual de São Paulo tem cerca de 3,5 milhões de alunos. No Censo de 2024, quase 96 % das salas estaduais não tinham climatização. Se a proporção valer para os alunos, são cerca de 3,36 milhões.
>
> Na capital, de 2022 a 2025, a rua chegou a 34 graus ou mais em 21 dias, e dentro da sala ninguém mede.
>
> Nossa meta no piloto de quatro semanas: zero aula em Perigo sem ação registrada, com a ação em até 15 minutos depois do alerta.

As ressalvas ficam no slide 6: a base de 3 milhões da outra nota da Seduc, a premissa de 35 alunos por turma e o fato de os 21 dias não serem a contagem oficial de ondas de calor do INMET.

## Encerramento (3:10 a 3:30)

> O Termômetro da Sala não climatiza a sala. Ele mostra o calor antes de alguém passar mal e registra o que foi feito, e esse histórico vira a prova para pedir a obra. Obrigado.

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
- Exemplo: rua prevista 34 + delta da sala 4 = 38, Cuidado extremo
- Erro médio de 0,958 °C no teste (árvore), contra 1,442 °C de "daqui a 2 h igual a agora"
- Nenhum dado de aluno

### Slide 6: O impacto
- Cerca de 3,5 milhões de alunos na rede estadual (Seduc-SP). Notícia de 20/12/2024 fala em 3 milhões
- 67.468 de 70.346 salas sem climatização, 95,91 % (Censo 2024, tabulação CIEPP) ≈ 3,36 milhões de alunos
- 35 alunos por turma: premissa. Onda de calor (INMET/OMM): +5 °C por 5 dias. 21 dias a 34 °C na A701, 2022–2025, não são essa contagem

### Slide 7: A arquitetura
- Motivação: nenhuma aula em Perigo sem ação (ODS 3.d, 4.a, 11.b, 13.1)
- Negócio: medir, prever, alertar, agir, registrar
- Aplicação e tecnologia: API, app, terminal, painel, ESP32, Windows Server

### Slide 8: O que muda
- Indicador: aulas em Perigo sem ação registrada. Meta do piloto (4 semanas): zero, com ação em até 15 min do alerta
- O histórico por sala mostra onde climatizar primeiro
- Termômetro da Sala
