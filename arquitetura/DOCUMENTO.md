# Termômetro da Sala: documento descritivo da arquitetura

Global Solution 2026.2 (CitizenTech). Disciplina: Testing, Compliance & Quality Assurance, Arquitetura de Soluções baseada no TOGAF.

Equipe: Samuel Baecker - RM [preencher]

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| Link do repositório | [preencher] |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## 1. Visão da arquitetura (Fase A do TOGAF)

Nas tardes quentes, ninguém sabe o quanto a sala de aula esquentou. A estação meteorológica mede a rua e o alerta público fala da cidade, mas nenhum dos dois diz que a sala 3 vai passar do limite às 14h. A escola só reage quando alguém passa mal.

O Termômetro da Sala mede cada sala, prevê o nível dela para daqui a 2 horas e faz a decisão chegar a quem pode agir: a coordenação. Cada ação fica registrada, e o histórico vira evidência para pedir climatização à Diretoria de Ensino.

**Objetivo da arquitetura:** nenhuma aula em Perigo sem ação.

**Escopo:** uma escola pública de São Paulo, com as salas sem climatização. A rede estadual aparece só como destino da evolução e como base da estimativa de impacto (seção 7).

**Fora do escopo:** controlar ventiladores ou ar-condicionado, guardar dado de aluno e substituir a decisão da coordenação. O sistema informa e registra, e quem decide é uma pessoa.

## 2. Público-alvo e partes interessadas

| Parte interessada | Papel | O que ganha |
|---|---|---|
| Turma | Beneficiária. Não usa o sistema e não é cadastrada | Aula com o calor sob controle |
| Professor(a) | Usa o app: acompanha a sala e registra ocorrências | Sabe o nível atual e o previsto, com a ação sugerida |
| Coordenação pedagógica | Opera o terminal: vê as salas, decide, registra a ação e revoga sensores | Um lugar só para decidir, antes de alguém passar mal |
| Famílias e comunidade escolar | Veem o painel web, só de leitura | Transparência sobre o nível das salas |
| Diretoria de Ensino / Seduc-SP | Recebe o relatório por sala | Dado para decidir onde climatizar primeiro |
| INMET | Fornece dados públicos | Não se aplica: é fonte de dados |

## 3. Requisitos

| Id | Requisito | Tipo | Onde aparece |
|---|---|---|---|
| R1 | Medir temperatura e umidade em cada sala | Funcional | ESP32 + DHT22 |
| R2 | Mostrar o nível na porta mesmo sem internet | Funcional | LED; o ESP32 calcula o nível no aparelho |
| R3 | Prever o nível da sala para daqui a 2 h | Funcional | Modelo + delta da sala, aplicados pela API |
| R4 | Mostrar nível, previsão e ação sugerida | Funcional | App, terminal e painel |
| R5 | Registrar ação e ocorrência com foto | Funcional | Terminal e app |
| R6 | Aceitar só leitura assinada, recusando forjada ou reenviada | Segurança | HMAC-SHA256 e ordem estrita do `medidoEm` |
| R7 | Não guardar nenhum dado de aluno | Privacidade | Ocorrência por sala, sem campo de aluno; foto sem pessoas |
| R8 | Nunca inventar dado | Qualidade | Sem leituras recentes, a tela mostra só a previsão da rua e o aviso |
| R9 | Recuperar sozinho de falha do serviço | Disponibilidade | Pool sempre ligado e recuperação do serviço no IIS |

Os níveis usam o índice de calor nas faixas da NOAA: abaixo de 27 °C é Normal, de 27 a 32 °C é Atenção, de 32 a 41 °C é Cuidado extremo e a partir de 41 °C é Perigo.

## 4. Diagrama de Arquitetura de Soluções

![Arquitetura em quatro camadas](diagrama.svg)

Os arquivos são `termometro-da-sala.xml` (modelo ArchiMate, formato Open Exchange 3.1) e `diagrama.svg` (a mesma visão como imagem). Para abrir o modelo no Archi: **File > Import > Open Exchange XML Model**. Depois, **File > Save** gera o arquivo nativo `.archimate`.

### 4.1 Motivação

Dois direcionadores explicam o problema: **ondas de calor** e **salas sem climatização**. A avaliação que os liga é que **ninguém mede o calor dentro da sala**. Daí sai o objetivo **"nenhuma aula em Perigo sem ação"**, sustentado por quatro requisitos: LED sem internet, previsão em 2 h, leitura assinada e nenhum dado de aluno.

| ODS e meta | Contribuição | Ligação no modelo |
|---|---|---|
| 3.d, alerta precoce de riscos à saúde | O nível previsto chega 2 h antes, com ação sugerida | Influenciado pelo objetivo |
| 13.1, resiliência a riscos do clima | A escola ganha um protocolo de calor e registra se ele foi cumprido | Influenciado pelo objetivo |
| 4.a, ambientes de aprendizagem seguros | Mede sala por sala e mostra quais precisam de obra primeiro | Realizado pelo histórico por sala |
| 11.b, planos de adaptação das cidades | O relatório dá à Diretoria de Ensino um dado que ela não tem hoje | Realizado pelo histórico por sala |

### 4.2 Negócio

O processo tem cinco passos em sequência:

| Processo | O que acontece | Quem participa |
|---|---|---|
| Medir | O sensor lê a sala a cada 10 s e acende o LED | ESP32 |
| Prever | Rua prevista + delta da sala = nível em 2 h | API e modelo |
| Alertar | O nível aparece no app, no terminal e no painel | Serve professor(a) e coordenação |
| Agir | A coordenação decide com base no protocolo de calor (água, pausa, troca de sala, adiar Educação Física, tirar a turma) | Coordenação, em favor da turma |
| Registrar | Ação e ocorrência ficam gravadas e alimentam o relatório por sala | Coordenação e professor(a); serve a Diretoria de Ensino |

A previsão é sempre um apoio. O registro é o que permite medir o indicador de impacto: aulas em Perigo sem ação e minutos entre o alerta e a ação.

### 4.3 Aplicação

| Componente | Responsabilidade | Tecnologia |
|---|---|---|
| API | Valida e grava leituras, calcula o índice e a previsão, guarda ocorrências e ações | ASP.NET Core 8, EF Core |
| Assinante MQTT | Parte da API. Recebe as leituras do broker e aplica a mesma checagem do `POST /leituras` | MQTTnet |
| Modelo de ML | Prevê o índice de calor da rua em 2 h | Regressão linear treinada em Python, aplicada pela API em C# |
| App do professor | Salas, detalhe com previsão e ocorrência com foto | Expo + TypeScript |
| Terminal da coordenação | Salas, registro de ação, cadastro e revogação de sensor | Windows Forms |
| Painel web | Mural de só leitura para a TV da escola | Página estática no `wwwroot` da API |

**Resultado do modelo.** Dados reais do INMET (A701), treino em 2022–2023, validação em 2024 e teste em 2025. No teste, o erro médio (MAE) foi de 1,052 °C contra 1,442 °C da regra "daqui a 2 h igual a agora", 27 % menor. O F1 macro dos níveis foi 0,745 contra 0,667. Detalhes em `ml/README.md`.

**A previsão da sala** é a rua prevista somada ao delta da sala, que é a média da diferença entre sala e rua nas últimas 3 h, com no mínimo 3 leituras. Exemplo: rua prevista em 34 + delta 4 = 38, Cuidado extremo. **Limitação:** o delta assume que a diferença se mantém nas próximas 2 h, e no fim da tarde tende a superestimar.

### 4.4 Tecnologia

| Elemento | Papel |
|---|---|
| ESP32 + DHT22 + LEDs (Wokwi) | Mede, acende o LED sem depender da rede e publica a leitura assinada |
| Broker MQTT público | Entrega as leituras à API. A conexão sai da VM para fora, sem abrir porta |
| Windows Server (VM) | Hospeda IIS, Active Directory, DNS e SQL Server |
| IIS | Publica a API e o painel, com pool sempre ligado |
| Active Directory e DNS | Domínio `termometro.local`, GPO e auditoria. O AD depende do DNS |
| SQL Server | Guarda salas, dispositivos, leituras, previsões, ocorrências e ações |
| INMET (A701) | Dados horários públicos: treinam o modelo e, no modo demonstração, reproduzem a rua de um dia quente |
| `modelo.json` | Artefato com os coeficientes, carregado pela API |

## 5. Estado da implementação

Situação deste ramo em 04/11/2026. **Atualizar antes da entrega.**

| Parte | Situação |
|---|---|
| Modelo de ML | Pronto: notebook, dataset e `modelo.json` |
| App do professor | Telas prontas, com modo simulado. Falta ligar na API real |
| API | Projeto inicial, com EF Core, SQL Server e MQTTnet já referenciados. Rotas por fazer |
| Terminal da coordenação | Projeto inicial |
| ESP32, Windows Server, painel web | Planejados |

O que é real: os dados do INMET e a pessoa entrevistada. O que é simulado: as leituras do sensor no Wokwi, as leituras semeadas no modo demonstração, as ocorrências e o dia reproduzido da rua. Na tela, a etiqueta DEMONSTRAÇÃO marca a previsão que usa leitura simulada.

## 6. Decisões de arquitetura

**Por que MQTT.** O ESP32 no Wokwi sai para a internet pública, mas não alcança uma VM local. Com um broker no meio, os dois lados só fazem conexão de saída, e o servidor não precisa de porta aberta. O tópico é público de propósito, porque a segurança vem da assinatura. O custo é depender de um broker externo. Por isso existe o `POST /leituras`, com a mesma checagem, e o LED da porta segue funcionando sem rede.

**Por que o modelo em JSON.** Rodar Python dentro do IIS traria um segundo runtime só para uma conta. A regressão linear ganhou da árvore rasa na validação e no teste, e seu resultado cabe em oito números: intercepto e sete coeficientes. A API em C# faz a conta, e dá para auditar o modelo linha a linha. O custo é trocar o arquivo sempre que o modelo for treinado de novo.

**Por que nenhum dado de aluno.** Os alunos são menores de idade, e dado que não existe não vaza. A unidade do sistema é a sala. O formulário pergunta "o que aconteceu", e não "com quem". A foto é da sala ou do equipamento, sem pessoas, e sai sem EXIF, para não levar localização. O painel público não mostra fotos. O custo é não acompanhar a saúde de cada aluno, o que nunca foi objetivo do sistema, e a identificação do professor fica só no aparelho dele.

**Por que HMAC por dispositivo.** Num tópico público, um token dentro da mensagem seria copiado por quem assinasse o tópico. Só a assinatura viaja, e a ordem estrita do `medidoEm` impede o reenvio. Como cada dispositivo tem seu segredo, vazar um não permite assinar por outro.

**Por que o nível é calculado duas vezes.** O ESP32 calcula para o LED funcionar sem internet. A API recalcula, e o valor dela é o oficial.

## 7. Público-alvo e estimativa de impacto

**Legenda:** **[MEDIDO]** é um número contado neste repositório, sobre dados públicos. **[PREMISSA]** é um valor assumido para a conta. **[A CONFERIR]** indica que o valor ou o link ainda não foi confirmado na fonte oficial.

### 7.1 Quantas pessoas o problema alcança

| Letra | Dado | Valor | Situação | Onde confirmar |
|---|---|---|---|---|
| A | Alunos da rede estadual de SP | 3,5 milhões | [PREMISSA] [A CONFERIR] valor aproximado do plano do projeto | Seduc-SP e Censo Escolar do INEP (referências 1 a 3) |
| B | Alunos por turma | 35 | [PREMISSA] [A CONFERIR] | INEP, indicador "média de alunos por turma" (referência 4) |
| C | Parcela sem climatização | 50 %, 75 % ou 90 % | [PREMISSA] [A CONFERIR] cenários sem base medida | Censo Escolar, microdados (referência 3) |

A conta usa **turmas**, e não salas físicas, porque a mesma sala recebe mais de uma turma nos diferentes turnos. Contar por sala física com 35 alunos deixaria de fora quem estuda em outro turno.

**Turmas da rede:** A ÷ B = 3.500.000 ÷ 35 = **100.000 turmas**.

| Cenário | Turmas sem climatização | × alunos por turma | Alunos em sala sem climatização |
|---|---|---|---|
| Parcela de 50 % | 50.000 | 35 | **1.750.000** |
| Parcela de 75 % | 75.000 | 35 | **2.625.000** |
| Parcela de 90 % | 90.000 | 35 | **3.150.000** |

**Como trocar a premissa pelo dado:** nos microdados do Censo Escolar mais recente, filtrar as escolas de SP da dependência estadual e somar as salas em uso e as salas climatizadas. C = 1 − (climatizadas ÷ em uso). Em versões recentes, esses campos devem aparecer como `QT_SALAS_UTILIZADAS` e `QT_SALAS_UTILIZA_CLIMATIZADAS` [A CONFERIR no dicionário de dados]. Se o item não existir, o valor de C continua sendo premissa e deve ser declarado assim.

**O piloto:** uma escola com [nº de salas da escola entrevistada] salas × 35 alunos × [nº de turnos] = [alunos por dia na escola].

Esta seção estima quantas pessoas estão expostas ao problema, e não quantas seriam protegidas. A redução real só pode ser medida num piloto, pelo indicador "aulas em Perigo sem ação".

### 7.2 Com que frequência faz calor em São Paulo

Contagem feita em `ml/dados/inmet_a701.csv`, com os dados horários da estação A701 (Mirante de Santana) de 2022 a 2025, 34.786 horas válidas. Todos os números da tabela são **[MEDIDO]**, exceto a última coluna.

| Ano | Horas com a rua a 32 °C ou mais | Horas a 34 °C ou mais | Dias a 34 °C ou mais | Desses, em dia útil de aula [PREMISSA] |
|---|---:|---:|---:|---:|
| 2022 | 29 | 0 | 0 | 0 |
| 2023 | 136 | 45 | 10 | 4 |
| 2024 | 124 | 15 | 5 | 5 |
| 2025 | 97 | 15 | 6 | 1 |
| **Total** | **386** | **75** | **21** | **10** |

Episódios de três dias seguidos ou mais a 34 °C: 11 a 15/11/2023, 24 a 26/09/2024 e 25 a 28/12/2025.

Pelo índice de calor, a rua ficou em Normal por 30.792 horas, em Atenção por 3.745 e em Cuidado extremo por 249, ou 0,7 % das horas. Perigo nunca apareceu na rua. É exatamente aí que a sala importa: o que falta saber é quanto a sala passa da rua, e essa medida hoje não existe. O delta de +4 °C usado nos exemplos é **[PREMISSA]**, e não uma medição.

**Limites desta contagem:**
- 34 °C é um corte escolhido para este trabalho, e não a definição oficial de onda de calor. A definição do INMET, que compara com a média histórica, deve ser citada à parte, se usada **[A CONFERIR]** (referência 5).
- Os valores são os de cada hora cheia, e não a máxima do dia. O pico real pode ser maior.
- 278 horas do período (0,8 %) não têm leitura válida.
- A estação representa a capital, e não o estado. Por isso a frequência não foi multiplicada pelas turmas da rede.
- "Dia útil de aula" é premissa: segunda a sexta, de fevereiro a meados de dezembro, sem feriados, sem conferir o calendário escolar de cada ano.

### 7.3 Referências

| Nº | Fonte | Link | Situação |
|---|---|---|---|
| 1 | Seduc-SP, dados da rede | https://www.educacao.sp.gov.br/ | [A CONFERIR] Acesso em: [data] |
| 2 | INEP, Censo Escolar | https://www.gov.br/inep/pt-br/areas-de-atuacao/pesquisas-estatisticas-e-indicadores/censo-escolar | [A CONFERIR] Acesso em: [data] |
| 3 | INEP, microdados do Censo Escolar | https://www.gov.br/inep/pt-br/acesso-a-informacao/dados-abertos/microdados/censo-escolar | [A CONFERIR] Acesso em: [data] |
| 4 | INEP, média de alunos por turma | https://www.gov.br/inep/pt-br/acesso-a-informacao/dados-abertos/indicadores-educacionais/media-de-alunos-por-turma | [A CONFERIR] Acesso em: [data] |
| 5 | INMET, avisos e definição de onda de calor | https://portal.inmet.gov.br/ | [A CONFERIR] Acesso em: [data] |
| 6 | INMET, dados históricos (A701, 2022–2025) | https://portal.inmet.gov.br/dadoshistoricos | Dados contidos no repositório. Contagem feita em 04/11/2026 |
| 7 | ONU Brasil, texto das metas dos ODS | https://brasil.un.org/pt-br/sdgs | [A CONFERIR] Acesso em: [data] |
| 8 | NOAA, índice de calor | https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml | [A CONFERIR] Acesso em: [data] |

## 8. Riscos

| Risco | Impacto | Tratamento |
|---|---|---|
| Quem tem o link do Wokwi vê o segredo de demonstração e forja leituras daquele dispositivo | Médio | Segredo por dispositivo e revogável. A decisão final é da coordenação, e o LED mede sozinho |
| Broker público fora do ar ou com o tópico inundado | Médio | `POST /leituras` como reserva. A sala aparece como "sem leitura recente" e o LED segue funcionando |
| Previsão errada no fim da tarde, porque o delta superestima | Baixo | Limitação declarada. O erro é para o lado da cautela, e a leitura ao vivo prevalece |
| Sensor na porta não representar o meio da sala | Médio | Calibrar com termo-higrômetro no piloto |
| O modelo não prevê acima do que viu: a rua nunca passou de Cuidado extremo nos dados | Médio | O Perigo da sala vem da medição e do delta. Treinar de novo a cada ano com dados novos |
| Foto de ocorrência com rosto de aluno | Alto | Aviso na tela, sem EXIF, foto fora do painel e acesso por id, nunca por caminho |
| Assinante MQTT parar com o pool ocioso | Alto | Pool sempre ligado e recuperação automática, testados em Operating Systems |
| Coordenação sem tempo de olhar o terminal | Médio | Indicador de minutos entre alerta e ação, para mostrar o gargalo. Notificação como evolução |

## 9. Evolução

| Horizonte | O que muda |
|---|---|
| Piloto em uma escola | Termo-higrômetro por uma semana, medindo o delta real. Sensor físico. Segredo gravado na memória do ESP32 |
| Mais de uma escola | Broker próprio com usuário, senha e TLS. Busca do INMET ao vivo. Notificação no app. Login do terminal com o AD |
| Rede de ensino | Relatório consolidado para a Diretoria de Ensino decidir onde climatizar primeiro. Novo treino anual do modelo. Modelo por sala, depois de haver histórico real |

Como o código usa a entidade neutra `Ambiente`, a mesma solução pode atender outros lugares quentes trocando dados, e não código.
