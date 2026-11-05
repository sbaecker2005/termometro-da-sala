# Seu tema da GS: "Termômetro da Sala", calor extremo dentro da sala de aula

## A resposta em uma linha

Fique com **Calor extremo**, mas mude o recorte. Em vez de "sombra e água para idosos e trabalhadores de rua", que é o exemplo literal do briefing, foque **no calor dentro da sala de aula de uma escola pública de São Paulo perto de você**.

**O problema em uma frase:** nas tardes quentes, a sala de aula passa do limite seguro sem que ninguém meça. A coordenação só fica sabendo quando um aluno passa mal.

**Quem se beneficia:** o professor ou a professora e a turma daquela sala. A pessoa real que vocês entrevistam é um(a) professor(a) ou coordenador(a) dessa escola.

### Antes de tudo, uma pergunta (responda até 05/11)

Você perguntou de novo qual é o tema, mas não disse o que não gostou no plano anterior. A resposta muda conforme o motivo:

1. **O plano do calor não convenceu, mas o calor serve.** Fique com o **Termômetro da Sala**, que é todo o resto deste documento. *(Recomendado: é o que você já tinha escolhido, e o recorte novo resolve os pontos fracos do plano anterior.)*
2. **Não quero mais trabalhar com calor.** Fique com a **Régua da Rua**, sobre alagamento num ponto da rua (veja "Alternativa fora do calor", no fim). Ela usa a mesma arquitetura, então este plano continua valendo quase inteiro.

A decisão precisa sair até 05/11 porque o primeiro commit é em 06/11. Até lá, trocar de tema custa só texto.

---

## Por que esse e não o plano anterior

O plano anterior ("Sombra & Água") explicava o *como* antes de responder *qual tema e por quê*, e tinha seis pontos fracos. O novo recorte resolve todos e mantém a escolha que você já tinha feito (o calor).

| Ponto fraco do plano anterior | Como o "Termômetro da Sala" resolve |
|---|---|
| Copiava o exemplo do briefing palavra por palavra. Muitas equipes vão entregar a mesma coisa, e o pitch concorre a melhor GS. | Quase ninguém vai pegar esse recorte, e a banca entende a ideia em 10 segundos. |
| Tinha dois cidadãos (idosos e trabalhadores de rua), o que dava duas jornadas e dois apps. | Um lugar (a sala), um responsável que decide (a coordenação) e um grupo beneficiado (professor e turma). |
| O ESP32 num poste de rua repetia o que a estação do INMET já mede, e o sensor virava enfeite. | Ninguém mede a temperatura **dentro** da sala. O ESP32 vira o instrumento principal e também alimenta a previsão de cada sala, como pede a disciplina de Physical Computing ("não criar um projeto paralelo"). |
| "Ninguém é avisado" era um argumento fraco, porque a Defesa Civil já manda alerta de evento extremo por SMS (40199). | Nenhum alerta público avisa que "a sala 3 vai passar do limite às 14h". Hoje essa informação não existe. |
| Pedia um cadastro de idosos, ou seja, dado pessoal de gente vulnerável, o que pesa em Cybersecurity e na LGPD. | **Nenhum dado de aluno.** Tudo é registrado por sala. Os alunos são menores de idade, e não guardar nada sobre eles é uma decisão de projeto (Security by Design). |
| O bairro e a pessoa real ficaram como "[a preencher]". | Há escola pública em todo bairro, e quase todo mundo conhece um professor. A conversa cabe nesta semana. |

Mais três motivos:
- **Encaixa nas 7 disciplinas.** O Windows Forms vira o terminal da coordenação, que é exatamente o "controle administrativo / posto local" descrito pelo professor de C#. A câmera do app gera o upload de foto, que é o cenário de LFI citado pelo professor de Cybersecurity.
- **Fica longe da deficiência visual,** como você pediu.
- **Continua valendo daqui a seis meses.** O verão vai até março. Não há dado pessoal, e o segredo de demonstração do Wokwi é revogado depois da avaliação (veja o contrato). Assim, o repositório pode ficar público no seu portfólio.

---

## Ficha da CitizenTech (os nove campos da p. 20, no topo de todos os README)

A p. 20 diz que a ficha responde às seis perguntas do briefing. A coluna da direita mostra qual pergunta cada campo responde.

| Campo (p. 20) | Conteúdo | Pergunta |
|---|---|---|
| Nome da CitizenTech | Termômetro da Sala | |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada]. É uma escola estadual ou municipal do bairro de vocês ou do caminho até a FIAP, preenchida depois da conversa | |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica | 1 |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal | 2 |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada (pausa para água, troca de sala, mudança na Educação Física). O professor acompanha pelo app e registra ocorrências com foto | 3 |
| O que cada parte faz | **ESP32 (Wokwi):** mede temperatura e umidade, mostra o nível no LED mesmo sem internet e envia a leitura assinada por MQTT. **API (ASP.NET Core + SQL Server):** valida e grava as leituras, calcula o índice de calor e a previsão e guarda ocorrências e ações. **Modelo (Python):** prevê o índice de calor da rua daqui a 2 h, treinado com dados reais do INMET. **App (React Native + Expo):** o professor vê a sala e registra ocorrências com a câmera. **Desktop (Windows Forms):** a coordenação acompanha as salas, registra ações e revoga sensores. **Windows Server:** hospeda a API e o painel web no IIS, com domínio, monitoramento e recuperação de falha. **Painel web:** mural de só leitura para a TV da escola | 4 |
| Link do repositório | `https://github.com/<organização>/termometro-da-sala` (criar no dia 06/11 e colar aqui) | |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação. Para ampliar, o histórico de cada sala vira evidência para a escola pedir climatização à Diretoria de Ensino, e a mesma solução serve para outras escolas | 6 |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1). A contribuição de cada um está na tabela abaixo | |
| Ecossistema *(pergunta 5, fora dos nove campos)* | A escola (coordenação e professores), alunos e famílias (painel público, só leitura), a Diretoria de Ensino/Seduc (relatório para decidir onde climatizar primeiro) e o INMET (dados públicos) | 5 |

**ODS e como a solução contribui para cada meta** (o briefing, na p. 14, torna essa explicação obrigatória; antes de copiar para o README, confiram o texto oficial das metas em brasil.un.org):

| ODS e meta | Contribuição concreta |
|---|---|
| **3, meta 3.d**: reforçar a capacidade de alerta precoce e de gestão de riscos à saúde | A sala recebe um alerta precoce: o nível previsto para daqui a 2 h, com uma ação sugerida, antes de alguém passar mal. O indicador é o número de aulas em "Perigo" sem ação registrada |
| **4, meta 4.a**: instalações físicas de educação que ofereçam ambientes de aprendizagem seguros e eficazes | Mede, sala por sala, se o ambiente de aula está em condição segura, e gera o histórico que mostra quais salas precisam de obra primeiro |
| **11, meta 11.b**: cidades com planos integrados de adaptação à mudança do clima e de resiliência a desastres | O relatório por sala entrega à Diretoria de Ensino um dado que hoje ela não tem, para planejar a adaptação da rede às ondas de calor |
| **13, meta 13.1**: reforçar a resiliência e a capacidade de adaptação a riscos relacionados ao clima | A escola passa a ter um protocolo de calor (a tabela de ações por nível) e a registrar se ele foi cumprido |

---

## O contrato que liga as partes (fechar no primeiro dia)

Em projeto de 7 disciplinas, o que costuma quebrar é a integração. Registrem num `CONTRATO.md`, na raiz do repositório, as cinco coisas que todas as partes usam. Quem mexer nelas abre um pull request e avisa o grupo.

**Nomes neutros desde o primeiro commit.** No código, a entidade se chama `Ambiente` (com `tipo = "sala"`), o domínio do AD é `termometro.local` e as ações sugeridas ficam numa tabela do banco, não escritas no código. Assim, trocar de lugar (veja o Plano B) muda dados, e não código. Na tela continua aparecendo "Sala".

### 1. O relógio

Todas as partes usam a mesma regra de tempo:
- **No banco e nas mensagens, tudo é UTC.** O `medidoEm` do ESP32 é em segundos Unix, e as colunas de data são `DateTimeOffset`.
- **A hora local de São Paulo é UTC−3**, sem horário de verão desde 2019. Ela só é usada em três lugares: no atributo "hora" (e "mês") do modelo, na projeção do dia quente do modo demonstração e no que aparece na tela.
- **O INMET entrega em UTC.** Os CSV históricos trazem a coluna "Hora UTC", e a API horária também usa UTC. O notebook converte antes de qualquer coisa: `tz_localize("UTC").dt.tz_convert("America/Sao_Paulo")`. A API converte com `TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")`, o ID do Windows, que funciona no Windows Server sem depender de outra biblioteca.
- **Vetor de teste do relógio:** o registro do INMET de um dia de janeiro às "1700 UTC" é a hora local 14. O notebook tem um `assert` com esse caso, e a API tem um teste unitário com o mesmo caso. Sem isso, o modelo aprende o ciclo do dia deslocado em 3 h e a previsão da sala sai errada.

### 2. A leitura do sensor

O ESP32 publica, a API grava e o resto consome:
```json
{ "ambienteId": "sala-03", "dispositivo": "esp32-sala-03",
  "temperaturaC": 33.4, "umidadePct": 58, "medidoEm": 1794331200,
  "assinatura": "<HMAC-SHA256 em hex minúsculo>" }
```
O ESP32 publica uma leitura a cada 10 s. O `medidoEm` vem do NTP (`time(nullptr)`). O ESP32 chama `sntp_set_sync_interval(60000)` antes do `configTime`, para ressincronizar a cada 60 s: no Wokwi, o relógio do ESP32 anda no tempo simulado, que fica mais lento que o real com Wi-Fi e MQTT ligados, e o padrão (ressincronizar de hora em hora) deixaria o relógio atrasar durante a gravação. A API converte com `DateTimeOffset.FromUnixTimeSeconds`. Data em texto não entra na assinatura, porque cada lado formata de um jeito.

**Texto canônico assinado** (os dois lados precisam gerar exatamente estes bytes):
```
ambienteId|dispositivo|temperaturaC|umidadePct|medidoEm
sala-03|esp32-sala-03|33.4|58|1794331200
```
| Campo | ESP32 (C++) | API (C#) |
|---|---|---|
| temperaturaC | `snprintf(t, sizeof t, "%.1f", temp)`. O **mesmo** texto `t` vai para o JSON | `leitura.TemperaturaC.ToString("0.0", CultureInfo.InvariantCulture)` |
| umidadePct | arredondar para inteiro e usar `%d` | `leitura.UmidadePct.ToString(CultureInfo.InvariantCulture)` (campo `int`) |
| medidoEm | `%lld` de `time(nullptr)` | `leitura.MedidoEm.ToString(CultureInfo.InvariantCulture)` (campo `long`) |
| separador e codificação | `\|`, UTF-8, sem espaços | igual |
| assinatura | HMAC-SHA256 pelo mbedTLS do core do ESP32, em hex minúsculo | `HMACSHA256.HashData`, hex minúsculo, comparação com `CryptographicOperations.FixedTimeEquals` |

Usar sempre `InvariantCulture` é obrigatório: num Windows Server em pt-BR, o padrão escreveria "33,4" e recusaria todas as leituras. **Vetor de teste:** no primeiro dia, a equipe escolhe um segredo de teste, calcula a assinatura da linha de exemplo acima e escreve as duas no `CONTRATO.md`. O ESP32 imprime essa assinatura no Serial ao ligar, e um teste unitário da API confere o mesmo valor. É a primeira coisa do degrau 0: enquanto os dois lados não baterem, o resto da integração espera.

O segredo **nunca viaja na mensagem**. A API refaz a assinatura com o segredo daquele dispositivo e decide. Cada recusa vai para o log com um motivo próprio, para não misturar ataque com falha de rede:

| Situação | O que a API faz | Motivo no log | Para que serve |
|---|---|---|---|
| A assinatura não confere | recusa | `assinatura-invalida` | Evidência de Cyber (leitura forjada ou alterada) |
| `medidoEm` igual ou anterior à última leitura aceita daquele dispositivo | recusa | `reenvio` | Evidência de Cyber (mensagem copiada e reenviada) |
| Dispositivo revogado (`ativo = false`) | recusa | `dispositivo-revogado` | Evidência de Cyber |
| `medidoEm` mais de 10 min atrás ou mais de 1 min à frente do relógio do servidor | recusa | `atrasada` | Log de operação, não de ataque: leitura velha nunca vira "agora" na previsão |
| Fora da faixa física (-10 a 60 °C, 0 a 100 %) | recusa | `fora-da-faixa` | Defeito de sensor ou forja grosseira |
| Salto de mais de 5 °C sobre a leitura anterior, ou sala mais de 8 °C abaixo da rua num dia acima de 30 °C | grava como "suspeita", fora da previsão, em destaque no desktop | `suspeita` | Plausibilidade (equipes de 4 ou 5; nas de 2 ou 3, fica em "Se sobrar tempo") |

O que impede o reenvio é a ordem estrita do `medidoEm` por dispositivo. A janela de 10 min só impede que leitura velha entre como atual. Por isso ela pode ser larga o bastante para tolerar o relógio do Wokwi e uma reconexão do MQTT.

**Onde fica o segredo, sem fingir mais proteção do que existe.** Physical Computing exige entregar o link do projeto no Wokwi, e quem tem o link vê todos os arquivos, inclusive o `secrets.h`. Então:
- o segredo usado no Wokwi é um **segredo de demonstração**, e cada dispositivo tem o seu. Vazar o segredo de um dispositivo não permite assinar leitura de outro;
- a tabela `Dispositivo` tem `ativo` e `segredo`. A coordenação revoga ou troca o segredo pelo desktop. Depois da avaliação, o segredo de demonstração é revogado;
- no GitHub, o `secrets.h` está no `.gitignore` e o repositório traz só `secrets.exemplo.h`. Na API, o segredo fica numa variável de ambiente do pool do IIS, nunca no `appsettings.json` versionado;
- num aparelho instalado de verdade, o segredo seria gravado na memória do ESP32 (NVS) na hora da instalação, fora do código-fonte. Isso é descrito como evolução, não como algo pronto.

### 3. Os níveis de calor

Usam o índice de calor nas faixas da NOAA e ficam numa tabela só, a mesma para ESP32, API, app e desktop:

| Índice de calor | Nível | LED na porta | Ação sugerida (tabela do banco, tipo "sala") |
|---|---|---|---|
| abaixo de 27 °C | Normal | verde | nenhuma |
| 27 a 32 °C | Atenção | amarelo | liberar água, abrir janelas e ligar ventiladores |
| 32 a 41 °C | Cuidado extremo | vermelho | pausa, troca de sala ou adiar a Educação Física |
| 41 °C ou mais | Perigo | vermelho piscando | tirar a turma da sala |

O ESP32 calcula o nível no próprio aparelho para o LED funcionar **mesmo sem internet**, como o briefing pede ("continuar relevante fora dos cenários ideais de conectividade"). A API recalcula, e o valor dela é o oficial.

### 4. A previsão da sala

Cada parte tem uma origem definida:

| Peça | De onde vem |
|---|---|
| Índice de calor da rua agora | No núcleo: o dia quente reproduzido do CSV do INMET (veja o item 5). Se sobrar tempo: a API busca a cada hora o boletim da estação A701 (Mirante de Santana) na API pública do INMET, com o Open-Meteo como reserva |
| Índice da rua daqui a 2 h | O modelo de ML (veja IA e ML), aplicado pela API com os atributos do mesmo boletim e a hora **local** (item 1) |
| Delta da sala | Média de (índice da sala − índice da rua) nas leituras das **últimas 3 h**, com no mínimo 3 leituras, comparando sala e rua no mesmo instante UTC. Ex.: a sala 3 marcou 36, 37 e 37 com a rua em 32, 33 e 33, o que dá delta de +4 |
| Previsão da sala | Índice da rua previsto + delta da sala, convertido em nível pela tabela acima. Rua prevista em 34 + delta 4 = 38, ou seja, "Cuidado extremo às 16h" |
| Sem leituras suficientes | O app e o desktop mostram só a previsão da rua, com o aviso "sala sem leituras recentes". Nunca inventam o delta |

O delta é uma regra estatística simples e documentada, e por isso pode ser auditada linha a linha. **Limitação declarada no README:** o delta assume que a diferença entre a sala e a rua das últimas 3 h se mantém nas próximas 2 h (num fim de tarde ele tende a superestimar).

### 5. O modo demonstração

Sem ele, a previsão da sala só aparece depois de 3 h de Wokwi ligado. Com ele, rua e sala ficam no mesmo relógio e a gravação começa na hora:

| Peça | Como funciona |
|---|---|
| Ativação | A API sobe com `ASPNETCORE_ENVIRONMENT=Demo`. Em `Production`, nada disto existe |
| Rua | O dia quente do CSV do INMET, já convertido para a hora local (item 1), é **projetado no relógio de hoje**: as 14h locais daquele dia (17 UTC no CSV) viram hoje às 14h locais. Gravem entre 12h e 17h, quando a curva de calor da rua está no alto |
| Sala ao vivo primeiro | O `diagram.json` do Wokwi já começa o DHT22 em 33 °C e 55 % (`"attrs": { "temperature": "33", "humidity": "55" }`), e não nos 24 °C e 40 % do padrão da peça. Assim, a primeira leitura é coerente com um dia quente e não cai na regra de plausibilidade |
| Histórico da sala | Com o Wokwi já publicando, `POST /demo/semear?ambienteId=sala-03` grava as 3 h anteriores à primeira leitura ao vivo, com `origem = "simulada"`. A curva sai de `demo/leituras-simuladas.csv`, gerado por `demo/gerador.py` (ganho da sala conforme a face do sol e o andar, 1 h de atraso térmico e ruído, e **não** a regra do delta), e é deslocada para terminar no valor da última leitura ao vivo. Sem leitura ao vivo, a rota responde 409 "ligue o sensor primeiro". Essa rota não passa pelo HMAC, porque não é mensagem de dispositivo, e só é registrada no ambiente Demo |
| Mexer no slider | Subir em passos de até 4 °C, esperando uma publicação (10 s) entre um passo e outro: 33 → 37 → 41 leva cerca de 30 s e leva a sala de "Cuidado extremo" a "Perigo" na tela |
| Cena extra para Cyber | Arrastar o slider 12 °C de uma vez: a leitura aparece como "suspeita" no desktop e não muda a previsão. A regra que atrapalharia a demonstração vira evidência |
| Transparência | Enquanto houver leitura simulada na janela, o app e o desktop mostram a etiqueta "DEMONSTRAÇÃO". O README diz o que é simulado |
| Evidência em Cyber | Print da `/demo/semear` respondendo 404 com a API em `Production` |

O gerador é um só e serve tanto à demonstração quanto à parte 2 do notebook. Assim, ninguém mantém duas simulações diferentes.

---

## Construir em degraus: cada degrau funciona sozinho

O briefing pede "Comecem pequeno" (p. 19) e "Simples primeiro [...] Façam o básico bem feito e só depois incrementem" (p. 21). Por isso o projeto sobe em degraus. Cada degrau termina com tudo funcionando, uma tag de versão e um pull request aprovado. Se o tempo acabar no meio de um degrau, entrega-se o anterior, que funciona.

| Degrau e tag | Prazo | O que funciona ao final |
|---|---|---|
| **0, fio ponta a ponta** (`v0.1.0`) | 06 a 09/11 | Vetor de teste do HMAC e do relógio conferido dos dois lados. ESP32 publica a leitura assinada, a API recusa a leitura forjada e a reenviada e grava a válida (com a primeira migração antes de qualquer rota). `GET /ambientes` responde. O app lista as salas, o desktop lista as salas e o painel web mostra o nível. Notebook com o INMET carregado em hora local. Tudo ainda na máquina de cada um |
| **1, a função completa** (`v0.2.0`) | 09 a 12/11 | `modelo.json` com métrica e baseline, delta e previsão na API, no app e no desktop. Ocorrência com foto pela câmera. Registro de ação e revogação de sensor no desktop. Modo demonstração |
| **2, infraestrutura e documentos** (`v0.3.0`) | 12 a 16/11 | Windows Server com a API e o painel no IIS, os dois testes de falha, o monitoramento e a auditoria. Relatório de riscos com as evidências. ArchiMate com a estimativa de impacto. Ensaio completo do modo demonstração no servidor |
| **3, entrega** (`v1.0.0`) | 16 a 18/11 | Vídeos, pitch, README com a Ficha, `.txt` e `authors`. Só então os itens de "Se sobrar tempo" |

## O que cada disciplina entrega

**Regra da equipe:** a coluna "Núcleo" de **todas** as disciplinas fica pronta antes de qualquer item da coluna "Se sobrar tempo".

| Disciplina | Núcleo (tem que funcionar) | Se sobrar tempo |
|---|---|---|
| **SOA e Web Services** | ASP.NET Core com as entidades `Ambiente`, `Dispositivo`, `Leitura` (com `origem`: dispositivo, simulada ou manual), `LeituraRua`, `Previsao`, `Ocorrencia`, `Acao` e `NivelAcao`. EF Core com SQL Server e **migrações desde o primeiro commit** (valem 2,5 dos 10 pontos). Rotas `GET /ambientes`, `GET /ambientes/{id}/leituras` (também em CSV, para o notebook), `GET /ambientes/{id}/previsao`, `POST /leituras` (com a mesma checagem do MQTT), `POST /ocorrencias`, `POST /acoes` e `POST /demo/semear` (só no ambiente Demo). Testes unitários dos vetores do HMAC e do relógio. Coleção do Insomnia com prints (os "testes manuais" valem 1,5) | Paginação, filtro por período, relatório por sala |
| **Cybersecurity** | Ativos: banco, API, servidor, dispositivo, segredos e contas da coordenação. Ameaças concretas: **leitura forjada ou reenviada** num tópico MQTT público (alguém forja calor para liberar a turma, ou esconde o calor), SQLi na busca de ocorrências, **LFI no download da foto** e OSINT (foto da sala com rosto de aluno). Evidências: prints do log com `assinatura-invalida`, `reenvio` e `dispositivo-revogado`, da consulta parametrizada, da foto salva com nome GUID e servida por id (nunca por caminho), da `/demo/semear` respondendo 404 em produção, do `secrets.h` fora do repositório e do nmap do servidor mostrando só as portas necessárias. **Riscos residuais declarados:** (1) quem tem o link do Wokwi vê o segredo de demonstração e pode forjar leituras **daquele** dispositivo até ele ser revogado. O impacto é limitado: a decisão final é da coordenação e o LED de cada porta mede sozinho. (2) Num broker público, qualquer um pode encher o tópico de lixo (negação de serviço) | Evidência da leitura "suspeita" (com a plausibilidade), broker com usuário, senha e TLS (ex.: plano gratuito de um MQTT gerenciado), limite de envios por dispositivo, segredo gravado na NVS |
| **C# Windows Forms** | Terminal da coordenação: lista de salas com nível atual e previsto, cadastro de sala (andar, face do sol, se tem ventilador) com validação, registro da ação tomada, revogação do dispositivo e POO com classes do domínio, tudo conversando com a API. **Versionamento semântico com pull request** (veja "Entregas de cada disciplina") | Leituras "suspeitas" em destaque, exportar o relatório do dia em CSV, login com conta do AD |
| **IA e ML** | **O modelo.** Dados **reais** do INMET (o briefing aceita bases públicas como dado do projeto): estação automática São Paulo, Mirante de Santana (A701), dados horários dos anos completos mais recentes, **convertidos de UTC para a hora local** antes de tudo. Alvo: **índice de calor da rua daqui a 2 h** (regressão). Atributos, todos presentes no boletim horário e portanto disponíveis em produção: temperatura, umidade, hora local, mês e variação da última hora (radiação fica de fora). Modelo: regressão linear ou árvore rasa, com divisão por tempo (treina nos anos antigos, testa no mais recente). Métricas: MAE do índice e F1 macro dos níveis, os dois comparados com a regra "daqui a 2 h igual a agora", que o modelo precisa vencer. Exportar em `modelo.json`. Dataset e origem no repositório. A previsão aparece no app e no desktop | **Parte 2, a sala** (núcleo nas equipes de 4 ou 5): o notebook baixa as leituras da sala pela API (CSV) e avalia a previsão completa da sala (modelo + delta) contra o índice que a sala realmente teve 2 h depois, comparando com o baseline "sala daqui a 2 h = sala agora". Enquanto as leituras forem simuladas, o README diz que o número prova a integração e mostra onde o delta erra (o fim de tarde), não a acurácia no mundo real. Leituras **reais** da sala entrevistada com um termo-higrômetro barato, anotadas pelo professor durante uma semana e lançadas como `origem = "manual"`. Árvore maior em ONNX |
| **Mobile e IoT** | Expo + TypeScript estrito, sem `any`, `tsc --noEmit` limpo. Identificação do professor lembrada ao reabrir o app. Lista de salas (estados de carregando, erro e vazio). Detalhe da sala com a previsão (ou o aviso "sala sem leituras recentes", e a etiqueta "DEMONSTRAÇÃO" quando for o caso). Formulário validado de ocorrência com **foto pela câmera** (recurso nativo, com a permissão tratada). Pasta `services/`, validação pura, README e DECISOES.md | Notificação quando a sala do professor muda para "Cuidado extremo". Tela de leitura manual do termo-higrômetro |
| **Operating Systems** | VM com domínio (`termometro.local`), DNS, hostname e IP fixo (prints de `ipconfig`, `nslookup` e `Test-NetConnection`). API e **painel web** no IIS (site, pool, runtime .NET, pasta de fotos com NTFS onde só o pool escreve). Pool sempre ligado: Application Initialization instalado, `startMode=AlwaysRunning`, `preloadEnabled=true` e idle timeout 0. GPO (inclui acesso remoto por RDP só para um grupo do AD), auditoria de logon e de acesso à pasta de fotos, alertas de CPU, disco e memória no Monitor de Desempenho. **Teste de falha, em duas partes, porque a nota pede a "recuperação automática do serviço e do pool do IIS":** (1) matar o `w3wp.exe` (`Stop-Process`) e mostrar o WAS subindo outro processo; (2) configurar a recuperação do W3SVC (`sc failure w3svc reset= 86400 actions= restart/5000`), derrubar o processo do serviço e mostrar que ele volta sozinho. Prova: Visualizador de Eventos e logs do IIS. Atenção: o padrão do Rapid-Fail Protection desliga o pool depois de 5 falhas em 5 min, então não matem o processo mais de 4 vezes seguidas na gravação | Desktop rodando numa máquina cliente do domínio |
| **Physical Computing** | ESP32 + DHT22 + LEDs na porta da sala, com horário por NTP (ressincronizado a cada 60 s), publicando a leitura assinada do contrato e imprimindo o vetor de teste no Serial ao ligar | Display OLED ("Índice 34 °C: pausa para água"), botão "professor pediu ajuda" que gera uma ocorrência |
| **Testing (TOGAF) + pitch** | ArchiMate em 4 camadas. Motivação: ondas de calor e salas sem climatização, com a meta "nenhuma aula em perigo sem ação". Negócio: coordenação, professor e Diretoria de Ensino. Aplicação: API, app, desktop, painel e modelo. Tecnologia: Windows Server, SQL Server, ESP32, broker e INMET. **Abertura (10 % da nota):** nome do projeto, equipe com nome e RM de cada integrante e o problema em uma frase. **Público e estimativa de impacto (10 % da nota):** quantos alunos estudam na rede (Seduc-SP; Censo Escolar do INEP), quantas salas não têm climatização (se o Censo não trouxer o item, declarar a premissa usada), e a conta "salas sem climatização × alunos por sala", com fonte e premissa escritas. Pitch de 4 min que abre com a fala da pessoa entrevistada e mostra a previsão no modo demonstração | Registros de ondas de calor do INMET para mostrar a frequência do problema |

O **painel web** é uma página estática dentro do próprio projeto da API (`wwwroot`), só de leitura, para o mural ou a TV da escola. Custa pouco e garante a "aplicação WEB hospedada executando completamente" que Operating Systems avalia, porque uma API sozinha corre o risco de não contar como aplicação web.

### Entregas de cada disciplina (núcleo, não "se sobrar tempo")

São os itens que se perdem no último dia. Cada um tem um dono, e todos estão prontos até 18/11.

| Disciplina | O que enviar | Itens fáceis de esquecer |
|---|---|---|
| Todas | — | Um repositório, uma pasta por parte (`api/`, `app/`, `desktop/`, `esp32/`, `ml/`, `infra/`, `arquitetura/`) e a Ficha no topo de cada README |
| SOA | Link do GitHub | README com instruções de execução, descrição e **nomes dos integrantes**. Prints do Insomnia |
| Cybersecurity | Relatório em PDF, envio único | Ativos, ameaças, vulnerabilidades, riscos e controles, com os prints |
| C# | ZIP do projeto + **arquivo .txt com RM, nome completo e link do repositório** (um integrante envia) | **Versionamento semântico com pull request:** a `main` é protegida, toda mudança entra por PR revisado por outro integrante, cada degrau gera uma tag (`v0.1.0`, `v0.2.0`, `v0.3.0`, `v1.0.0`) e há um `CHANGELOG.md`. O README cita o problema, a pessoa real, como rodar, as tecnologias e o nome da equipe |
| IA e ML | Notebook `.ipynb` + link do vídeo de até 2 min | Dataset e sua origem, modelo treinado, métrica e README com as decisões |
| Mobile | ZIP da pasta do app **sem `node_modules`** + vídeo de até 2 min | **Array `authors` no `package.json` com nome e RM de todos** (ex.: `"authors": ["Nome Completo - RM000000"]`), README com a Ficha, DECISOES.md |
| Operating Systems | Link do GitHub, até 19/11 às 23h55 | Roteiro de configuração, prints e saídas de comando, README com a Ficha, vídeo de até 7 min |
| Physical Computing | Link do projeto no Wokwi + link do vídeo de até 4 min | **Arquivo .txt com o nome dos integrantes** |
| Testing (TOGAF) | Diagrama ArchiMate + documento descritivo | Slide de abertura com nome e RM e a estimativa de impacto com fonte |

### Quem faz o quê, conforme o tamanho da equipe

O tamanho não foi informado. Escolham a linha que corresponde à equipe. Cada pessoa é dona das partes listadas, mas todo mundo precisa saber explicar o projeto inteiro, como o briefing pede.

| Equipe | Divisão | Núcleo |
|---|---|---|
| 2 pessoas | **A:** API + banco, Cybersecurity, Windows Forms e Windows Server (tudo em .NET e na mesma VM). **B:** ESP32/Wokwi, app, notebook de ML, ArchiMate e as entregas de cada disciplina. Pitch e entrevista juntos | **Reduzido** (abaixo) |
| 3 pessoas | **A:** API + banco + Cybersecurity. **B:** Windows Server + Windows Forms + ArchiMate. **C:** ESP32/Wokwi + app + ML | **Reduzido** (abaixo) |
| 4 a 5 pessoas | Uma pessoa para cada um destes blocos: API + Cyber; Windows Server + Forms; app; ESP32 + ML. Com 5, a quinta fica com ArchiMate, pitch, vídeos, README e as entregas de cada disciplina, e testa a integração todo dia | Completo, mais a plausibilidade e a parte 2 do notebook |

**Núcleo reduzido para 2 ou 3 pessoas.** Desde o primeiro dia, ficam fora: a regra de plausibilidade (só vale a faixa física, e o risco é declarado em Cyber), a parte 2 do notebook, a busca do INMET ao vivo e o Open-Meteo (a rua vem só do dia reproduzido), a sessão persistente do MQTT (QoS 0, e leitura perdida aparece como "sem leitura recente") e o OLED. Cada item entra no README como limitação. O que sobra é o degrau 0 e o degrau 1, e cada peça corresponde a uma nota pontuada.

**Ordem de cortes para equipes de 4 ou 5.** Se em 12/11 o degrau 1 não estiver pronto, cortem nesta ordem, e só o necessário. Cada corte mantém a solução funcionando e entra no README como limitação:
1. A busca do INMET ao vivo e o Open-Meteo saem, e a rua vem só do dia reproduzido.
2. A parte 2 do notebook sai.
3. A regra de plausibilidade fica só com a faixa física.
4. O MQTT passa de QoS 1 com sessão persistente para QoS 0.
5. Os botões e o OLED saem, e ficam só os LEDs.

O teste de falha **não** entra nos cortes: a nota de Operating Systems pede a recuperação do serviço e também a do pool.

**Nunca cortar, e por quê:**
- **A entrevista**, porque é a base da proposta e a abertura do pitch.
- **As migrações**, porque valem 2,5 pontos em SOA.
- **A assinatura com antirreenvio**, porque é o único controle contra a ameaça principal (forjar calor num tópico público) e sustenta as evidências de Cyber (2,0) e a análise de riscos (3,0). São umas 30 linhas de cada lado, verificadas pelo vetor de teste.
- **A câmera no app**, porque vale 20 % em Mobile.
- **O modelo com métrica e baseline**, porque modelo, treino e avaliação valem 5 dos 10 pontos de IA e ML.
- **O painel web no IIS**, porque o "funcionamento completo" vale 2 pontos em Operating Systems.
- **O modo demonstração**, porque sem ele os vídeos não mostram a previsão.
- **As entregas de cada disciplina.**

---

## Armadilhas que este desenho já evita

1. **O ESP32 do Wokwi não alcança a sua VM.** O simulador sai para a internet pública, mas não chega ao `localhost` nem a uma VM local sem gateway privado. Por isso o ESP32 publica a leitura por **MQTT num broker público**, num tópico da equipe (ex.: `fiap-gs/<nome-da-equipe>/leituras`), e a API assina o tópico com o MQTTnet. A conexão sai da VM para fora, sem abrir porta. O tópico é público de propósito: a segurança vem da assinatura HMAC, não de esconder o nome do tópico. Para a demonstração não depender do broker, a coleção do Insomnia tem um `POST /leituras` equivalente, com a mesma checagem.
2. **O assinante MQTT morre com o pool ocioso.** Com a configuração padrão, a API no IIS só sobe na primeira requisição e é desligada depois de 20 min sem uso, levando junto o serviço que escuta o MQTT. O pool "sempre ligado" (em Operating Systems) resolve isso. Com QoS 1 e sessão persistente, se o broker oferecer, as leituras de uma queda de até 10 min chegam depois e são aceitas, porque cabem na janela e vêm em ordem. Numa queda mais longa, elas chegam com o motivo `atrasada`, o LED da porta continua funcionando e a API mostra a sala como "sem leitura recente", sem inventar dado.
3. **Um token que viaja na mensagem não protege nada.** Num tópico público, quem assina o tópico copia o token. Por isso só a assinatura viaja, e a ordem estrita do `medidoEm` impede o reenvio. O segredo do Wokwi é público para quem tem o link, por isso ele é de demonstração, um por dispositivo e revogável.
4. **Assinatura que nunca confere.** Número e data formatados de um jeito em cada lado (como "33.40" contra "33,4") fazem a API recusar toda leitura legítima. O texto canônico, a `InvariantCulture`, o horário em segundos Unix e o vetor de teste do primeiro dia evitam isso.
5. **Relógio do Wokwi atrasando.** O tempo simulado corre mais devagar que o real. A ressincronização do NTP a cada 60 s e a janela de 10 min evitam que as leituras ao vivo sejam recusadas no meio da gravação. No ensaio, deixem o Wokwi ligado 15 min e confiram que o log não mostra nenhum `atrasada`.
6. **Hora do INMET em UTC.** Projetar ou treinar com "Hora UTC" como se fosse hora local desloca o ciclo do dia em 3 h e estraga o delta e a previsão. O item "Relógio" do contrato e o seu vetor de teste evitam isso.
7. **Demonstração que esbarra nas próprias regras.** Com o DHT22 no padrão de 24 °C, a primeira leitura ao vivo seria "suspeita". O `diagram.json` começa em 33 °C, o `/demo/semear` ancora o histórico na leitura ao vivo e o slider sobe em passos de até 4 °C.
8. **Modelo treinado com dado inventado só aprende a própria fórmula.** Por isso o modelo é treinado com dado real do INMET e só com atributos que existem em produção.
9. **Servidor Python dentro do IIS.** Não precisa: o modelo sai em JSON e quem aplica é o C#.
10. **Migrações esquecidas.** Valem 2,5 pontos em SOA. Criem a primeira migração antes de qualquer rota.
11. **Dado de menor de idade.** Nada de nome de aluno, nem nas ocorrências: o formulário pede "o que aconteceu", e não "com quem". A foto é da sala ou do equipamento, sem pessoas, e o painel público não mostra fotos.
12. **Entrega incompleta no último dia.** O .txt de C#, o de Physical, o `authors` e as tags com PR estão na tabela "Entregas de cada disciplina" e no degrau 3, e não na memória de alguém.

---

## O que fazer agora

| Quando | O quê |
|---|---|
| 05/11 | **Decidir entre o Termômetro da Sala e a Régua da Rua** (a pergunta do topo) |
| até 06/11 | Conversar com um(a) professor(a) ou coordenador(a) de escola pública. Perguntas: "A sala esquenta? Em que horário? O que vocês fazem hoje? Quem decide? Alguém já passou mal?". Se der, pedir para anotar a temperatura da sala com um termo-higrômetro durante uma semana. Fotografar a sala vazia. Criar o repositório com a `main` protegida e as pastas por parte. Fechar a Ficha (com o link do repositório) e o `CONTRATO.md` com os vetores de teste do HMAC e do relógio |
| 06/11 à noite | **Decisão:** se não houver entrevista marcada até aqui, acionar o Plano B |
| 06 a 09/11 | Degrau 0 (`v0.1.0`) |
| 09 a 12/11 | Degrau 1 (`v0.2.0`). Em 12/11, conferir o núcleo e aplicar o núcleo reduzido ou a ordem de cortes, se precisar |
| 12 a 16/11 | Degrau 2 (`v0.3.0`), terminando com o **ensaio completo do modo demonstração** no servidor: Wokwi ligado, semear, slider em passos e a previsão no app e no desktop, das 12h às 17h |
| 16 a 18/11 | Degrau 3 (`v1.0.0`): vídeos, pitch, README e a tabela "Entregas de cada disciplina" conferida item por item |
| 19/11 | Cada disciplina enviada por um integrante, até 23h55 (20/11 é feriado, não existe prazo extra). Depois da nota, revogar o segredo de demonstração |

### Plano B: a cozinha de um pequeno restaurante ou padaria do bairro

Escolha esse plano se até 06/11 à noite ninguém de escola topar conversar. É um caso só, para não abrir uma nova escolha: adultos, um dono que decide e calor de forno e fogão todos os dias. Como o código já usa nomes neutros, a troca é quase toda de dados e de texto:

| Item | Escola | Cozinha (Plano B) |
|---|---|---|
| Entidade, rotas, AD, contrato, relógio, assinatura, modo demonstração, ML, delta, IIS | iguais | **iguais** (só muda `tipo = "cozinha"` no cadastro) |
| Quem decide / beneficiado | coordenação / professor e turma | dono ou gerente / cozinheiros e auxiliares |
| Ações por nível (tabela `NivelAcao`) | água, troca de sala, adiar Educação Física, tirar a turma | água e pausa, revezar quem fica no fogão, desligar um forno, pausa longa fora da cozinha |
| Dado sensível | nenhum dado de aluno (menores de idade) | nenhum nome de funcionário nas ocorrências (dado de saúde do trabalhador) |
| Ecossistema | Diretoria de Ensino/Seduc | dono, funcionários, sindicato ou associação do setor. A NR-15 (Anexo 3) é a norma de calor no trabalho: citem-na como contexto, mas ela usa outro índice (IBUTG), então a fórmula da NOAA não muda |
| ODS e metas | 3.d, 4.a, 11.b, 13.1 | 3.d (alerta precoce de risco à saúde do trabalhador), 11.b e 13.1. O ODS 8 (meta 8.8, ambiente de trabalho seguro) combina com o tema, mas não está na lista da p. 14 do briefing: citem-no só como complementar |
| Indicador de impacto | aulas em "Perigo" sem ação | turnos em "Perigo" sem pausa registrada |
| Estimativa para o TOGAF | alunos e salas da rede (Seduc-SP, INEP) | estabelecimentos e empregados de alimentação em SP (RAIS/CAGED ou associação do setor; conferir a fonte) |
| Demonstração | gravar entre 12h e 17h | igual, ou no horário de pico do forno |
| Pitch, painel web, README, ArchiMate | escola | reescrever os nomes e a fala de abertura |

### Alternativa fora do calor: "Régua da Rua", alagamento num ponto da rua

Só se você responder a opção 2 da pergunta do topo. A arquitetura, os degraus e as entregas são os mesmos, e muda o que o sensor mede:

| Campo | Conteúdo |
|---|---|
| Problema em uma frase | Quando chove forte, um ponto conhecido da rua (perto da escola, de um comércio ou de um ponto de ônibus) alaga em minutos, e quem está ali só descobre quando a água já subiu na calçada |
| Cidadão e pessoa real | Dono de um comércio ou morador da rua que alaga. A conversa é fácil: todo bairro tem a sua rua que alaga, e a época de chuva em São Paulo começa justamente agora |
| ESP32 | Sensor ultrassônico HC-SR04 (existe no Wokwi) virado para a sarjeta, medindo a altura da água em cm, com LED e buzzer no próprio ponto |
| ML | INMET A701 horário, real e convertido para a hora local: classificar "chuva forte na próxima hora" com pressão, variação da pressão, umidade, temperatura, chuva da última hora, hora e mês, e comparar o F1 com o baseline "vai chover se está chovendo agora". A classe é rara, então o relatório mostra a precisão e o recall, sem esconder |
| O papel do "delta" | Quantos cm a água sobe naquele ponto por mm de chuva, medido pelo próprio sensor. É o mesmo padrão de regra auditável aplicada sobre o modelo |
| O que muda no plano | Entidade `Ponto` em vez de `Ambiente`, campo `nivelAguaCm` em vez de temperatura e umidade no texto assinado (o vetor de teste é recalculado), a tabela de níveis em cm, a regra de plausibilidade em cm e a posição inicial do sensor no `diagram.json`. Como a decisão sai antes do primeiro commit, nada precisa ser reescrito |
| Ecossistema | Moradores e comerciantes, a subprefeitura (o pedido de limpeza de bueiro pelo SP156), o CGE e a Defesa Civil (199) |
| ODS e metas | 11.5 (reduzir as pessoas afetadas por desastres, inclusive os ligados à água), 13.1 (resiliência a riscos do clima) e 3.d (alerta precoce) |

---

## Como cheguei nessa escolha

Olhei o tema de três lados e resolvi os pontos em que eles discordavam:
- **Quem corrige (professores da FIAP):** cada disciplina precisa de um papel natural, não de um enfeite, cada entrega listada no briefing precisa estar lá, e a previsão tem que aparecer na solução com entradas reais. Esse lado queria uma ML treinada com dados ricos da própria sala.
- **Quem convive com o problema (pesquisa com o cidadão):** um cidadão, uma pessoa real com quem dá para conversar nesta semana e um problema que ainda não tem solução. Esse lado queria fotos reais de alunos na sala quente.
- **Quem mantém o código:** escopo que caiba em duas semanas com equipe de qualquer tamanho, degraus que funcionam sozinhos, um contrato único entre as partes (com relógio e vetores de teste), nomes que não precisem ser trocados e nenhuma armadilha de integração.

Como as divergências foram resolvidas:
- **ML.** O modelo é treinado com dado público real (o INMET), na hora local, e só com atributos disponíveis em produção. A sala entra pelo delta medido pelo ESP32. Ainda não existe histórico real da sala para treinar. As leituras do termo-higrômetro são o caminho para isso, e um modelo treinado com a própria fórmula não se sustenta na avaliação.
- **Fotos.** Só da sala vazia, porque foto de menor é risco de OSINT e de LGPD.
- **Abrangência.** Uma sala bem resolvida vale mais que a cidade inteira feita pela metade. Por isso as equipes pequenas têm um núcleo reduzido, e não um núcleo apertado.

---

## Premissas

- Interpretei o "qual vai ser meu tema" repetido como sinal de que o plano anterior não convenceu, mas você não disse o motivo. Por isso a pergunta do topo decide entre o mesmo tema com outro recorte (recomendado) e uma alternativa fora do calor, já pronta.
- O tamanho da equipe não foi informado. A divisão por tamanho, o núcleo reduzido e a ordem de cortes cobrem equipes de 2 a 5 pessoas.
- Os números da rede para a estimativa de impacto do TOGAF precisam ser conferidos nas fontes oficiais. A rede estadual paulista tem algo em torno de 3,5 milhões de alunos (Seduc-SP). Também servem o Censo Escolar do INEP e os registros de ondas de calor do INMET. Citem a fonte e a premissa no documento.
- O texto das metas dos ODS está resumido. Confiram a redação oficial antes de colar no README.
- O formato de hora do INMET ("Hora UTC" nos CSV) e a disponibilidade da API horária não foram verificados aqui. O vetor de teste do relógio e o teste da API no degrau 0 conferem isso, e o dia reproduzido do CSV é a reserva.
- A lentidão do relógio simulado do Wokwi varia com a carga. A ressincronização a cada 60 s e a janela de 10 min têm folga, e o ensaio de 15 min confirma.
- A escola e a pessoa entrevistada são reais. As leituras do sensor no Wokwi, as leituras semeadas no modo demonstração, as ocorrências e o dia reproduzido do INMET são simulados, e isso deve estar escrito no README, como o briefing pede. As leituras do termo-higrômetro, se acontecerem, são reais e marcadas como `origem = "manual"`.
