# Termômetro da Sala: roteiro do pitch

Vídeo de até 4 minutos, enviado em Testing. Tempo total do roteiro: 3min30s, com 30 s de folga sobre o limite. O briefing pede a solução e as entregas que mais somaram: o bloco "Entregas" cumpre essa parte.

Cronometre o ensaio. Se passar de 3min40s, corte primeiro na demonstração, nunca no impacto, na abertura nem na frase da pessoa entrevistada. A frase entra inteira, sem completar o bloco com outra fala.

Antes de gravar: os números de escola do impacto estão na seção 7 do `DOCUMENTO.md`. Se um número mudar lá, ele muda aqui e no slide 6. As informações do CRAS vêm do relato da coordenadora, não de medição no local. O aviso no celular é o que ela pediu e o próximo passo.

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

> Somos o Termômetro da Sala. A escola só reage quando alguém passa mal. No CRAS Santo Amaro passam no mínimo 150 pessoas por dia, sem climatização por orçamento, com calor de janeiro a março. Segundo a coordenadora, nenhuma escola ou CCA que ela saiba tem ar-condicionado.

## Fala da pessoa entrevistada (0:20 a 0:45)

> "O conforto térmico não deve ser compreendido como um privilégio, mas como uma condição necessária para garantir dignidade, acolhimento e qualidade no atendimento à população que utiliza os serviços públicos."

Crédito autorizado no slide 3: Monalisa Daniela Pereira Amaral, coordenadora do CRAS Santo Amaro (São Paulo).

Se ela não aparecer, leia o crédito e a frase, com o crédito na tela. Sem alunos no vídeo.

## Demonstração no modo demonstração (0:45 a 2:15)

Gravar entre 12h e 17h, com a API no ambiente `Demo`. Deixe o Wokwi publicando antes de começar e rode `POST /demo/semear?ambienteId=sala-03`. A foto da ocorrência é gravada antes e entra cortada.

| Tempo | Tela | O que mostrar | Fala |
|---|---|---|---|
| 0:45 a 1:00 | Wokwi | O ESP32 na porta da sala, em 33 °C, com o LED vermelho e a publicação no Serial | "Este sensor fica na porta da sala. Mede o calor, acende a cor do nível mesmo sem internet e envia a leitura assinada." |
| 1:00 a 1:10 | App, lista de salas | As salas com nível e índice, e a sala 3 em Cuidado extremo | "O professor vê as salas pelo app, sempre com a cor e o nome do nível." |
| 1:10 a 1:35 | App, sala 3 | Nível de agora, previsão para 2 h, ação sugerida e a etiqueta DEMONSTRAÇÃO. A conta rua prevista + delta da sala, com os números que a tela mostrar: o número da rua é a saída do modelo, calculada pela API naquele instante | "A rua de agora vem de um dia quente real do INMET. O modelo treinado com dados do INMET prevê a rua daqui a 2 horas, e somamos quanto esta sala costuma passar da rua. O modelo errou em média 0,909 grau no teste de 2025, contra 1,442 de supor que nada muda. A etiqueta avisa que as leituras são de demonstração." |
| 1:35 a 1:50 | Wokwi + terminal | Subir o slider em passos de até 4 °C, esperando 10 s entre eles, até a sala virar Perigo no terminal | "A sala esquentou, e a coordenação vê isso em todas as salas ao mesmo tempo." |
| 1:50 a 2:05 | Terminal da coordenação | Registrar a ação "tirar a turma da sala" | "Quem decide é a coordenação. A ação fica registrada, e é com esse registro que medimos o resultado." |
| 2:05 a 2:15 | App, nova ocorrência | Descrição + foto da sala vazia, e o painel web por 2 s | "O professor registra a ocorrência com foto da sala, sem pessoas. Nenhum dado de aluno entra no sistema." |

Com 55 % de umidade, a sala já vira Perigo no passo de 37 °C. Nesse caso, pare o slider ali. Arrastar 12 °C de uma vez gera uma leitura suspeita e atrapalha a cena.

A fala da sala 3 não cita a conta: ela depende da hora da gravação. Os números da tela são os que valem, e o slide 5 traz o exemplo 34 + 4 = 38. `ml/ROTEIRO-VIDEO.md` não tem tabela por hora: leia a rua prevista na tela.

## Entregas (2:15 a 2:30)

Cortes rápidos de 3 s cada, gravados antes, com o nome da disciplina na tela.

| Tela | Disciplina |
|---|---|
| Archi com a "Visão em quatro camadas" aberta e as visões Motivação, Negócio, Aplicação, Tecnologia e Implementação e migração na árvore do modelo (`arquitetura/termometro-da-sala.archimate`) | Arquitetura (TOGAF) |
| Notebook do modelo com a tabela de MAE de validação e teste | Machine Learning |
| Saída do `dotnet test` da solução da API com os 81 testes passando | SOA e Testing |
| Windows Server com o IIS e o pool da API | Operating Systems |

> Por trás disso: o modelo ArchiMate com uma visão por camada, o notebook com dados reais do INMET, a API com 81 testes passando e o servidor Windows que publica tudo.

Contagem conferida em 07/11/2026: 81 testes da API, conforme `api/evidencias/dotnet-test.txt`. Os 9 testes do desktop foram executados separadamente e não entram nessa fala.

## Impacto (2:30 a 3:10)

> A rede estadual de São Paulo: cerca de 3,5 milhões de alunos. Censo de 2024: quase 96 % das salas sem climatização; cerca de 3,36 milhões, se a proporção valer para os alunos. Na capital, de 2022 a 2025, 21 dias a 34 graus ou mais. No CRAS Santo Amaro passam no mínimo 150 pessoas por dia, sem climatização por orçamento, com calor de janeiro a março. Segundo ela, nenhuma escola ou CCA que ela saiba tem ar-condicionado. Meta em quatro semanas: zero aula em Perigo sem ação registrada, em até 15 minutos.

As ressalvas ficam no slide 6: a base de 3 milhões da outra nota da Seduc, a premissa de 35 alunos por turma e o fato de os 21 dias não serem a contagem oficial de ondas de calor do INMET.

## Encerramento (3:10 a 3:30)

> O próximo passo é o aviso no celular de quem cuida do local, como ela pediu. As precauções: ventilador, janelas, água gelada e leques. O histórico é a prova para pedir a manutenção. O alerta de frio, no berçário e no dormitório. Obrigado.

No slide 8, leia só a fala acima. O indicador do piloto e onde climatizar primeiro ficam na tela.

## Texto dos slides

### Slide 1: Termômetro da Sala
- Samuel Baecker - RM [preencher]
- Demais integrantes e RMs: [preencher]
- O calor da sala de aula, medido antes de alguém passar mal

### Slide 2: O problema
- A escola só reage quando alguém passa mal
- CRAS Santo Amaro: no mínimo 150 pessoas por dia
- Sem climatização por orçamento; calor de janeiro a março
- Segundo a coordenadora, nenhuma escola ou CCA que ela saiba tem ar-condicionado

### Slide 3: Quem vive isso
- "O conforto térmico não deve ser compreendido como um privilégio, mas como uma condição necessária para garantir dignidade, acolhimento e qualidade no atendimento à população que utiliza os serviços públicos."
- Monalisa Daniela Pereira Amaral, coordenadora do CRAS Santo Amaro (São Paulo)

### Slide 4: Como funciona
- Medir: sensor na porta da sala, com LED que funciona sem internet
- Prever: nível da sala para daqui a 2 h, com dados reais do INMET
- Agir: a coordenação decide e registra

### Slide 5: A previsão
- Exemplo: rua prevista 34 + delta da sala 4 = 38, Cuidado extremo
- Erro médio de 0,909 °C no teste (árvore), contra 1,442 °C de "daqui a 2 h igual a agora"
- Nenhum dado de aluno

### Slide 6: O impacto
- Cerca de 3,5 milhões de alunos na rede estadual (Seduc-SP). Notícia de 20/12/2024 fala em 3 milhões
- 67.468 de 70.346 salas sem climatização, 95,91 % (Censo 2024, tabulação CIEPP) ≈ 3,36 milhões de alunos
- 35 alunos por turma: premissa. Onda de calor (INMET/OMM): +5 °C por 5 dias. 21 dias a 34 °C na A701, 2022–2025, não são essa contagem
- CRAS Santo Amaro: no mínimo 150 pessoas por dia; sem climatização por orçamento; calor de janeiro a março
- Segundo ela, nenhuma escola ou CCA que ela saiba tem ar-condicionado

### Slide 7: A arquitetura
- Motivação: nenhuma aula em Perigo sem ação (ODS 3.d, 4.a, 11.b, 13.1)
- Negócio: medir, prever, alertar, agir, registrar
- Aplicação e tecnologia: API, app, terminal, painel, ESP32, Windows Server

### Slide 8: O que muda
- Próximo passo: aviso no celular de quem cuida do local, como ela pediu
- Precauções: ventilador, janelas, água gelada e leques
- O histórico é a prova para pedir a manutenção
- Alerta de frio: berçário e dormitório
- Indicador: aulas em Perigo sem ação registrada. Meta do piloto (4 semanas): zero, com ação em até 15 min do alerta
- O histórico por sala mostra onde climatizar primeiro
- Termômetro da Sala
