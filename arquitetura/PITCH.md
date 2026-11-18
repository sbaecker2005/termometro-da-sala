# Termômetro da Sala: roteiro do pitch

Vídeo de até 4 minutos, enviado em Testing. Tempo total do roteiro: 3min30s, com 30 s de folga sobre o limite. São cerca de 410 palavras faladas (cerca de 2min45s a 150 palavras por minuto); o restante é a pausa das telas gravadas. O briefing pede a solução e as entregas que mais somaram: o bloco "Entregas" cumpre essa parte.

Cronometre o ensaio. Se passar de 3min30s, corte primeiro na demonstração, nunca no impacto, na abertura nem na frase da pessoa entrevistada. A frase entra inteira, sem completar o bloco com outra fala.

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

> O mapa térmico mostra onde cada sensor e cada lugar ficam na planta, com temperatura, nível e precauções também em texto. A alternância para daqui a uma hora é uma aproximação, e o aviso na tela diz isso.

Gravar entre 12h e 17h, com a API no ambiente `Demo`. Antes de cada ensaio ou tomada, pare a API e o Wokwi e recrie **somente o banco local descartável abaixo**, reservado a este roteiro. Requer LocalDB e a ferramenta EF já disponíveis (ver `api/README.md`). Em PowerShell, dentro de `api/`, execute cada comando apenas se o anterior terminar sem erro:

```powershell
$env:ConnectionStrings__Sala='Server=(localdb)\MSSQLLocalDB;Database=TermometroRoteiroPitch;Integrated Security=True;TrustServerCertificate=True'
$env:ASPNETCORE_ENVIRONMENT='Demo'
$env:Mqtt__Desligado='false'
dotnet ef database drop --force --project TermometroDaSala.Api
dotnet ef database update --project TermometroDaSala.Api
dotnet run --project TermometroDaSala.Api --no-launch-profile --urls http://localhost:5080
```

O `drop` apaga todo o histórico desse banco de demonstração; não substitua a conexão pela de um banco com dados a preservar. O perfil `http` não é usado porque fixa Development.

O circuito atual de `esp32/README.md:36` usa quatro dispositivos na Sala 7; não publica na Sala 3 sem esta preparação específica. Após cada reset, as migrações recriam a Sala 3 e `esp32-sala-03` (usado aqui como porta), mas não configuram a planta. Com o Wokwi ainda parado, execute o SQL abaixo em um editor SQL já disponível, conectado a `(localdb)\MSSQLLocalDB`. Ele cadastra os outros três pontos apenas no banco descartável do pitch, reutilizando internamente o valor público de demonstração do dispositivo existente, sem exibi-lo, e configura uma planta fictícia de 6 × 4 m com seis cadeiras (a Sala 7 do exemplo, no `esp32/README.md`, tem outra planta, com vinte lugares):

```sql
USE [TermometroRoteiroPitch];
INSERT INTO Dispositivos (Id, AmbienteId, Segredo, Ativo)
SELECT ponto.Id, 'sala-03', porta.Segredo, 1
FROM Dispositivos AS porta
CROSS JOIN (VALUES ('esp32-sala-03-janela'), ('esp32-sala-03-fundo'), ('esp32-sala-03-centro')) AS ponto(Id)
WHERE porta.Id = 'esp32-sala-03' AND porta.AmbienteId = 'sala-03';
UPDATE Ambientes SET LarguraM = 6, ProfundidadeM = 4 WHERE Id = 'sala-03';
UPDATE d SET Rotulo = p.Rotulo, PosX = p.X, PosY = p.Y
FROM Dispositivos AS d
JOIN (VALUES ('esp32-sala-03-janela', 'janela', 0, 2),
             ('esp32-sala-03-fundo', 'fundo', 6, 4),
             ('esp32-sala-03', 'porta', 6, 0),
             ('esp32-sala-03-centro', 'centro', 3, 2)) AS p(Id, Rotulo, X, Y)
ON d.Id = p.Id
WHERE d.AmbienteId = 'sala-03';
INSERT INTO Lugares (AmbienteId, Nome, X, Y, Vulneravel)
VALUES ('sala-03', 'Cadeira 1', 0, 2, 1), ('sala-03', 'Cadeira 2', 3, 2, 0),
       ('sala-03', 'Cadeira 3', 6, 2, 0), ('sala-03', 'Cadeira 4', 0, 4, 0),
       ('sala-03', 'Cadeira 5', 3, 4, 0), ('sala-03', 'Cadeira 6', 6, 4, 1);
SELECT Id, Ativo, Rotulo, PosX, PosY FROM Dispositivos WHERE AmbienteId = 'sala-03' ORDER BY Id;
SELECT LarguraM, ProfundidadeM FROM Ambientes WHERE Id = 'sala-03';
SELECT Nome, X, Y, Vulneravel FROM Lugares WHERE AmbienteId = 'sala-03' ORDER BY Nome;
```

Esperado: três dispositivos inseridos, quatro IDs ativos com rótulos e coordenadas acima, dimensões 6 × 4 m e seis cadeiras, com 1 e 6 vulneráveis. Confira também `Invoke-RestMethod 'http://localhost:5080/ambientes/sala-03/mapa' | ConvertTo-Json -Depth 6`: `larguraM=6`, `profundidadeM=4` e dez `pontos` (quatro sensores e seis lugares) nas posições cadastradas; sem leituras, `semLeitura` deve ser verdadeiro. Se não conferir, não ligue o circuito. Não repetir os INSERTs sem reset. `POST /ambientes` não cadastra sensores e `PUT /ambientes/{id}/mapa` só configura sensores existentes; não substituem esse cadastro. `/demo/semear` só grava leituras e não configura a planta.

No projeto Wokwi reservado ao pitch, mantenha `sketch.ino`, `diagram.json` e `libraries.txt` atuais. Configure localmente `secrets.h`: `AMBIENTE_ID` como `sala-03`, `DISPOSITIVO` como `esp32-pitch-sala-03` (identificador da conexão MQTT) e os quatro itens de `DISPOSITIVOS`, na ordem janela, fundo, porta, centro, como `esp32-sala-03-janela`, `esp32-sala-03-fundo`, `esp32-sala-03`, `esp32-sala-03-centro`. Para `SEGREDO`, use somente o valor público do vetor de demonstração da Sala 3 em `CONTRATO.md`, que corresponde ao cadastro restaurado pelas migrações; o exemplo atualizado da Sala 7 usa outro valor. Não exiba esse arquivo na gravação.

Antes da primeira publicação, ajuste **os quatro DHT22 para 33 °C / 55 %**; não deixe os valores iniciais do diagrama. Com a API pronta, inicie o Wokwi, aguarde NTP e as quatro publicações. Noutra janela, consulte `Invoke-RestMethod 'http://localhost:5080/ambientes/sala-03/leituras'` e confirme os quatro IDs com `origem` igual a `dispositivo`, 33 °C / 55 % e `suspeita` falsa: o Serial sozinho não prova aceitação. Só então rode `Invoke-RestMethod -Method Post "http://localhost:5080/demo/semear?ambienteId=sala-03"` (201; sem leitura de dispositivo nessa sala, 409). Não execute `/demo/mapa` neste preparo: ele cadastra a Sala 7. A foto da ocorrência é gravada antes e entra cortada.

| Tempo | Tela | O que mostrar | Fala |
|---|---|---|---|
| 0:45 a 1:00 | Wokwi | O ESP32 na porta da sala, em 33 °C, com o LED vermelho e a publicação no Serial | "Este sensor fica na porta da sala. Mede o calor, acende a cor do nível mesmo sem internet e envia a leitura assinada." |
| 1:00 a 1:10 | App, lista de salas | As salas com nível e índice, e a sala 3 em Cuidado extremo | "O professor vê as salas pelo app, sempre com a cor e o nome do nível." |
| 1:10 a 1:35 | App, sala 3 | Nível de agora, previsão para 2 h, ação sugerida e a etiqueta DEMONSTRAÇÃO. A conta rua prevista + delta da sala, com os números que a tela mostrar: o número da rua é a saída do modelo, calculada pela API naquele instante | "A rua de agora vem de um dia quente real do INMET. O modelo treinado com dados do INMET prevê a rua daqui a 2 horas, e somamos quanto esta sala costuma passar da rua. O modelo errou em média 0,909 grau no teste de 2025, contra 1,442 de supor que nada muda. A etiqueta avisa que as leituras são de demonstração." |
| 1:35 a 1:50 | Wokwi + desktop da coordenação | Subir os quatro DHT22 de 33 para 37 °C, mantendo 55 % de umidade, e esperar as quatro publicações (cerca de 10 s); usar Atualizar no desktop para mostrar Perigo | "A sala esquentou, e a coordenação vê isso em todas as salas ao mesmo tempo." |
| 1:50 a 2:05 | Desktop da coordenação | Registrar a ação "tirar a turma da sala" | "Quem decide é a coordenação. A ação fica registrada, e é com esse registro que medimos o resultado." |
| 2:05 a 2:15 | App, nova ocorrência | Descrição + foto da sala vazia, e o painel web por 2 s, já atualizado antes do corte (mural só de leitura, atualização a cada 30 s) | "O professor registra a ocorrência com foto da sala, sem pessoas. Nenhum dado de aluno entra no sistema." |

Com 55 % de umidade, a sala já vira Perigo no passo de 37 °C. Nesse caso, pare os quatro sliders ali. Mantenha os quatro pontos iguais neste ensaio: a lista de salas usa a última leitura da sala, que pode vir de qualquer um deles. Arrastar 12 °C de uma vez gera uma leitura suspeita e atrapalha a cena.

A fala da sala 3 não cita a conta: ela depende da hora da gravação. Os números da tela são os que valem, e o slide 5 traz o exemplo 34 + 4 = 38. `ml/ROTEIRO-VIDEO.md` não tem tabela por hora: leia a rua prevista na tela.

## Entregas (2:15 a 2:30)

Cortes rápidos de 3 s cada, gravados antes, com o nome da disciplina na tela.

| Tela | Disciplina |
|---|---|
| Archi com a "Visão em quatro camadas" aberta e as visões Motivação, Negócio, Aplicação, Tecnologia e Implementação e migração na árvore do modelo (`arquitetura/termometro-da-sala.archimate`) | Arquitetura (TOGAF) |
| Notebook do modelo com a tabela de MAE de validação e teste | Machine Learning |
| Saída do `dotnet test` da solução da API com os 134 testes passando | SOA e Testing |
| Windows Server com o IIS e o pool da API | Operating Systems |

> Por trás disso: o modelo ArchiMate com uma visão por camada, o notebook com dados reais do INMET, a API com 134 testes passando e o servidor Windows que publica tudo.

Resultados medidos em 18/11/2026, zero falhas: API 134/134, desktop 19/19, app 24/24 e ML 46 testes. O desktop não entra na fala da API.

## Impacto (2:30 a 3:10)

> A rede estadual de São Paulo tem cerca de 3,5 milhões de alunos. Pelo Censo de 2024, quase 96 % das salas não têm climatização: cerca de 3,36 milhões de alunos, se a proporção valer para eles. Na capital, de 2022 a 2025, foram 21 dias a 34 graus ou mais. Meta em quatro semanas: zero aula em Perigo sem ação registrada, com mediana de até 15 minutos entre alerta e ação.

As ressalvas ficam no slide 6: a base de 3 milhões da outra nota da Seduc, a premissa de 35 alunos por turma e o fato de os 21 dias não serem a contagem oficial de ondas de calor do INMET.

## Encerramento (3:10 a 3:30)

> O próximo passo é o aviso no celular de quem cuida do local, como ela pediu. As precauções: ventilador, janelas, água gelada e leques. O histórico é a prova para pedir a manutenção. O alerta de frio, no berçário e no dormitório. Obrigado.

No slide 8, leia só a fala acima. O indicador do piloto e onde climatizar primeiro ficam na tela.

## Texto dos slides

### Slide 1: Termômetro da Sala
- Samuel David Baecker - RM 559269
- Lucas de Almeida Villar - RM 560005
- Guilherme Augusto Caseiro - RM 559765
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
- Indicador: aulas em Perigo sem ação registrada. Meta do piloto (4 semanas): zero; mediana de até 15 min entre alerta e ação
- O histórico por sala mostra onde climatizar primeiro
- Termômetro da Sala
