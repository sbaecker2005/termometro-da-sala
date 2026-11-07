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
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## 1. Visão da arquitetura (Fase A do TOGAF)

Nas tardes quentes, ninguém sabe o quanto a sala de aula esquentou. A estação meteorológica mede a rua e o alerta público fala da cidade, mas nenhum dos dois diz que a sala 3 vai passar do limite às 14h. A escola só reage quando alguém passa mal.

O Termômetro da Sala mede cada sala, prevê o nível dela para daqui a 2 horas e faz a decisão chegar a quem pode agir: a coordenação. Cada ação fica registrada, e o histórico vira evidência para pedir climatização à Diretoria de Ensino.

**Objetivo da arquitetura:** nenhuma aula em Perigo sem ação.

**Escopo:** uma escola pública de São Paulo, com as salas sem climatização. A rede estadual aparece só como destino da evolução e como base da estimativa de impacto (seção 7).

**Fora do escopo:** controlar ventiladores ou ar-condicionado, guardar dado de aluno e substituir a decisão da coordenação. O sistema informa e registra, e quem decide é uma pessoa.

**Princípios de arquitetura.** Cada princípio está no modelo como elemento Principle, realizado por um requisito, pela restrição ou por um processo.

| Princípio | Enunciado | Justificativa | Implicações |
|---|---|---|---|
| A decisão é humana | O sistema informa e registra; quem decide a ação é a coordenação | A previsão erra, e a escola responde pela turma | Nenhuma ação automática; o processo Agir é sempre de uma pessoa, e o registro mostra a ação e o horário |
| A sala é a unidade, não o aluno | Nenhum dado de aluno entra no sistema | Os alunos são menores de idade, e dado que não existe não vaza | Ocorrência por sala, foto da sala sem pessoas e sem EXIF, nenhum cadastro de aluno (R7) |
| Nunca inventar dado | Sem leitura recente, o sistema diz isso em vez de supor um valor | Um nível inventado tira a confiança no alerta verdadeiro | Previsão sem leitura válida responde `sem-dados-para-prever`; a etiqueta DEMONSTRAÇÃO marca dado simulado (R8) |
| A porta funciona sem rede | O alerta mais próximo da turma não depende de internet | A rede da escola cai, e o calor não espera | O ESP32 calcula o nível e acende o LED sozinho; a API recalcula e o valor dela é o oficial (R2) |
| Confiança pela assinatura, não pelo canal | Vale só a leitura assinada pelo dispositivo | O broker é público e qualquer um assina o tópico | HMAC-SHA256 com segredo por dispositivo, ordem estrita do `medidoEm` e revogação pelo terminal (R6) |

## 2. Público-alvo e partes interessadas

| Parte interessada | Papel | O que ganha |
|---|---|---|
| Turma | Beneficiária. Não usa o sistema e não é cadastrada | Aula com o calor sob controle |
| Professor(a) | Usa o app: acompanha a sala e registra ocorrências | Sabe o nível atual e o previsto, com a ação sugerida |
| Coordenação pedagógica | Opera o terminal: vê as salas, decide, registra a ação e revoga sensores | Um lugar só para decidir, antes de alguém passar mal |
| Famílias e comunidade escolar | Veem o painel web, só de leitura | Transparência sobre o nível das salas |
| Diretoria de Ensino / Seduc-SP | Recebe o relatório por sala | Dado para decidir onde climatizar primeiro |
| INMET | Fornece dados públicos | Não se aplica: é fonte de dados |

**Matriz de partes interessadas.** As cinco primeiras estão no modelo como Stakeholder, ligadas à preocupação (Assessment ou Goal) que a arquitetura precisa responder.

| Parte interessada | Preocupação no modelo | Visão que responde |
|---|---|---|
| Turma sem climatização | Salas sem climatização; nenhuma aula em Perigo sem ação | Motivação e Negócio (serviço "Resposta ao calor pelo protocolo") |
| Professor(a) | Ninguém mede o calor dentro da sala | Negócio (canal "App no celular do professor") e Aplicação |
| Coordenação pedagógica | A escola só reage quando alguém passa mal | Negócio (papel "Operador do protocolo") e Aplicação |
| Famílias e comunidade escolar | Comunidade vê o nível das salas | Negócio (canal "Mural na TV da escola") e Tecnologia |
| Diretoria de Ensino / Seduc-SP | Não há dado por sala para priorizar obras; histórico por sala | Negócio (serviço "Registro e relatório por sala") e Implementação e migração |

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

O modelo está em `termometro-da-sala.archimate`, o arquivo nativo do Archi (**File > Open**). O mesmo modelo está em `termometro-da-sala.xml`, no formato Open Exchange 3.1, para abrir em outra ferramenta ArchiMate (no Archi: **File > Import > Open Exchange XML Model**).

O modelo tem 106 elementos, 178 relações e seis visões, todas sobre os mesmos elementos e relações. Todo elemento tem uma documentação de uma frase, que o Archi mostra na aba **Properties**.

| Visão no Archi | Camada da rubrica | Fase do TOGAF | Seção | Imagem |
|---|---|---|---|---|
| Motivação | Visão da Arquitetura (Motivação) | A | 4.1 | `visao-motivacao.png` |
| Negócio | Visão de Negócios (Business) | B | 4.2 | `visao-negocio.png` |
| Aplicação | Visão de Sistemas (Aplicação e dados) | C | 4.3 | `visao-aplicacao.png` |
| Tecnologia | Visão de Infraestrutura (Tecnologia) | D | 4.4 | `visao-tecnologia.png` |
| Implementação e migração | Evolução da solução | E e F | 9 | `visao-implementacao.png` |
| Visão em quatro camadas | Resumo das quatro, com as ligações entre camadas | A a D | 4 | `diagrama-archi.png` |

As imagens foram geradas pelo próprio Archi a partir do modelo (relatório HTML da linha de comando do Archi 5.10). Para gerar de novo pela interface: abrir a visão e usar **File > Export > View As Image**, em PNG, com o nome da tabela.

A visão em quatro camadas é um resumo: mostra os elementos principais de cada camada e as ligações entre elas. Cada visão de camada traz o detalhe, e nela algumas ligações de atalho da visão geral passam pelo serviço correspondente (por exemplo, o IIS realiza "Hospedagem web", que serve a API).

**Conferência do modelo.** `python arquitetura/conferir_modelo.py` confere os dois arquivos: XML bem formado, ids únicos, toda relação com origem e destino existentes, toda visão apontando para elementos e relações existentes, todo elemento com documentação, os dois arquivos com o mesmo conteúdo e, com o Archi instalado, cada relação permitida pela tabela de relações do ArchiMate que vem com a ferramenta. O Archi também valida o `.xml` contra o XSD do Open Exchange ao importar.

![Visão em quatro camadas, exportada do Archi](diagrama-archi.png)

**Situação atual, situação alvo e lacuna por camada**

| Camada | Situação atual (escola hoje) | Situação alvo (esta arquitetura) | Lacuna e como é fechada |
|---|---|---|---|
| Motivação | Calor tratado como assunto da cidade; nenhuma meta para a sala | Objetivo "nenhuma aula em Perigo sem ação", com indicador medido pelo próprio sistema | Medir o indicador: minutos entre alerta e ação e aulas em Perigo sem ação (seção 7.1) |
| Negócio | A escola reage quando alguém passa mal; nada fica registrado | Medir, Prever, Alertar, Agir e Registrar, com protocolo de calor e papel de operador | Protocolo por nível e registro da ação no terminal |
| Aplicação e dados | Nenhum sistema; no máximo um termômetro de parede | API com serviços de leitura, consulta, ação, ocorrência e cadastro; app, terminal e painel | Construída nesta versão; a notificação no app fica para a evolução |
| Tecnologia | Sem sensor e sem servidor | ESP32 por sala, broker MQTT, Windows Server com IIS, SQL Server e AD | Sensor físico e segredo na memória do ESP32 no piloto; broker próprio com TLS na expansão |

### 4.1 Motivação (Visão da Arquitetura, Fase A)

![Visão Motivação, exportada do Archi](visao-motivacao.png)

Dois direcionadores explicam o problema: **ondas de calor** e **salas sem climatização**. A avaliação que os liga é que **ninguém mede o calor dentro da sala**. Daí sai o objetivo **"nenhuma aula em Perigo sem ação"**, sustentado por oito dos nove requisitos (R1 a R7 e R9) e pela restrição "Não acionar o ar nem guardar aluno", todos realizando o objetivo no modelo. O R8, "Sem leitura recente, não inventar o nível", realiza o princípio "Nunca inventar dado".

| ODS e meta | Contribuição | Ligação no modelo |
|---|---|---|
| 3.d, alerta precoce de riscos à saúde | O nível previsto chega 2 h antes, com ação sugerida | Influenciado pelo objetivo |
| 13.1, resiliência a riscos do clima | A escola ganha um protocolo de calor e registra se ele foi cumprido | Influenciado pelo objetivo |
| 4.a, ambientes de aprendizagem seguros | Mede sala por sala e mostra quais precisam de obra primeiro | Realizado pelo histórico por sala |
| 11.b, planos de adaptação das cidades | O relatório dá à Diretoria de Ensino um dado que ela não tem hoje | Realizado pelo histórico por sala |

A visão também traz as cinco partes interessadas ligadas às suas preocupações (matriz da seção 2), os nove requisitos da seção 3 (R1 a R9), a restrição "Não acionar o ar nem guardar aluno" e os cinco princípios da Fase A, cada um realizado pelo que o põe em prática: quatro por um requisito e "A decisão é humana" pela restrição (no modelo, também pelo processo Agir). Dois requisitos são realizados fora do negócio: "Recuperar sozinho de falha do serviço" pelo IIS, na camada de Tecnologia, e "Sem leitura recente, não inventar o nível" pela API.

### 4.2 Negócio (Visão de Negócios, Fase B)

![Visão Negócio, exportada do Archi](visao-negocio.png)

O processo tem cinco passos em sequência:

| Processo | O que acontece | Quem participa |
|---|---|---|
| Medir | O sensor lê a sala a cada 10 s e acende o LED | ESP32 |
| Prever | Rua prevista + delta da sala = nível em 2 h | API e modelo |
| Alertar | O nível aparece no app, no terminal e no painel | Serve professor(a) e coordenação |
| Agir | A coordenação decide com base no protocolo de calor (água, pausa, troca de sala, adiar Educação Física, tirar a turma) | Coordenação, em favor da turma |
| Registrar | Ação e ocorrência ficam gravadas e alimentam o relatório por sala | Coordenação e professor(a); serve a Diretoria de Ensino |

A previsão é sempre um apoio. O registro é o que permite medir o indicador de impacto: aulas em Perigo sem ação e minutos entre o alerta e a ação.

Na visão, o evento "Sala entra em Perigo" dispara o Alertar. Cada processo que entrega algo a alguém realiza um serviço de negócio:

| Serviço de negócio | Realizado por | Serve | Canal (Business Interface) |
|---|---|---|---|
| Alerta do nível da sala | Alertar | Professor(a), coordenação e famílias | App no celular do professor, terminal da coordenação, mural na TV da escola e LED na porta da sala |
| Resposta ao calor pelo protocolo | Agir | Turma | Não se aplica: a coordenação presta o serviço pessoalmente, no papel "Operador do protocolo" |
| Registro e relatório por sala | Registrar | Diretoria de Ensino | Terminal (ação) e app (ocorrência) |

A coordenação assume o papel "Operador do protocolo" para Agir, consultando o "Protocolo de calor". Agir e Registrar gravam a "Ação registrada"; o Registrar grava também a "Ocorrência da sala" e o "Relatório por sala". As famílias aparecem como ator, servidas pelo alerta no mural.

### 4.3 Aplicação (Visão de Sistemas, Fase C)

![Visão Aplicação, exportada do Archi](visao-aplicacao.png)

A Fase C junta aplicação e dados. A visão mostra os processos de negócio no alto, os serviços de aplicação que os servem, a API (com o assinante MQTT e as rotas HTTP dentro dela), os clientes à direita e os objetos de dados embaixo.

| Componente | Responsabilidade | Tecnologia |
|---|---|---|
| API | Valida e grava leituras, calcula o índice e a previsão, guarda ocorrências e ações | ASP.NET Core 8, EF Core |
| Assinante MQTT | Parte da API. Recebe as leituras do broker e aplica a mesma checagem do `POST /leituras` | MQTTnet |
| Modelo de ML | Prevê o índice de calor da rua em 2 h | Árvore de decisão treinada em Python (MAE 0,909 °C no teste 2025), aplicada pela API em C# a partir do modelo.json |
| App do professor | Salas, detalhe com previsão e ocorrência com foto | Expo + TypeScript |
| Terminal da coordenação | Salas, registro de ação, cadastro de sala e revogação de sensor | Windows Forms |
| Painel web | Mural de só leitura para a TV da escola | Página estática no `wwwroot` da API |

A API realiza cinco serviços de aplicação e expõe as "Rotas HTTP da API", usadas pelo app e pelo terminal:

| Serviço de aplicação | Rotas | Serve |
|---|---|---|
| Recebimento de leitura assinada | `POST /leituras` e assinatura do tópico MQTT | Medir |
| Consulta de nível e previsão | `GET /ambientes` e `GET /ambientes/{id}/previsao` | Prever e Alertar |
| Registro de ação | `GET` e `POST /acoes` (escrita com a chave da coordenação) | Agir e Registrar |
| Registro de ocorrência | `POST /ocorrencias`; a foto sai por `GET /ocorrencias/{id}/foto` | Registrar |
| Cadastro de sala e revogação de sensor | `POST /ambientes` e `POST /dispositivos/{id}/revogar` (com a chave da coordenação) | Terminal da coordenação |

**Dados (parte da Fase C).** No modelo, os objetos de dados são a "Leitura assinada", a "Previsão da sala" (calculada pela API a cada consulta; a tabela `Previsoes` fica reservada e nada grava nela hoje), "Ambiente (sala)", "Dispositivo (sensor)", "Ocorrência", "Ação" e "Nível e ação sugerida". Os três últimos realizam os objetos de negócio "Ocorrência da sala", "Ação registrada" e "Protocolo de calor". No banco, as tabelas são `Ambientes` (salas), `Dispositivos`, `Leituras`, `LeiturasRua`, `Previsoes`, `Ocorrencias`, `Acoes` e `NiveisAcao` (`api/TermometroDaSala.Api/Dados/SalaDb.cs`). Nenhuma tabela tem campo de aluno.

**Resultado do modelo.** Dados reais do INMET (A701), treino em 2022–2023, validação em 2024 e teste em 2025. A árvore (profundidade 12, folha 24) foi a escolhida pelo menor MAE na validação. No teste, o MAE foi de 0,909 °C contra 1,442 °C da regra "daqui a 2 h igual a agora". O F1 macro dos níveis foi 0,730 contra 0,667 do baseline. Em 2025, o recall de cuidado passou de 14,6% na árvore anterior para 39,6% na revisada, contra 37,5% do baseline; a precisão da revisada foi de 52,8%, com 29 falsos negativos e 17 falsos positivos. Em 2024, o recall da revisada ainda ficou abaixo do baseline (34,0% contra 39,4%). Detalhes em `ml/README.md`.

**A previsão da sala** é a rua prevista somada ao delta da sala, que é a média da diferença entre sala e rua nas últimas 3 h, com no mínimo 3 leituras. Exemplo: rua prevista em 34 + delta 4 = 38, Cuidado extremo. **Limitação:** o delta assume que a diferença se mantém nas próximas 2 h, e no fim da tarde tende a superestimar.

### 4.4 Tecnologia (Visão de Infraestrutura, Fase D)

![Visão Tecnologia, exportada do Archi](visao-tecnologia.png)

A visão mostra, no alto, os componentes de aplicação; no meio, os serviços de tecnologia e os aparelhos onde os clientes rodam; embaixo, o Windows Server com o que roda nele, o ESP32 com o firmware e a internet pública.

| Elemento | Papel |
|---|---|
| ESP32 + DHT22 + LEDs (Wokwi) | Mede, acende o LED sem depender da rede e publica a leitura assinada |
| Broker MQTT público | Entrega as leituras à API. A conexão sai da VM para fora, sem abrir porta |
| Windows Server (VM) | Hospeda IIS, Active Directory, DNS e SQL Server |
| IIS | Publica a API e o painel, com pool sempre ligado |
| Active Directory e DNS | Domínio `termometro.local`, GPO e auditoria. O AD depende do DNS |
| SQL Server | Guarda salas, dispositivos, leituras, previsões, ocorrências e ações |
| `inmet_a701.csv` (INMET, A701) | Artefato de dados: série horária pública de 2022 a 2025 que treina e testa o modelo. A API não consulta o INMET ao vivo (isso é evolução, seção 9); no ambiente Demo, a rua vem de `demo/rua-dia-quente.csv`, um dia quente de exemplo |
| `modelo.json` | Artefato com a árvore de decisão (nós, atributos e limites de corte), parte da API publicada, carregado e percorrido pela API |
| API publicada e Banco SalaDb | Artefatos implantados no Windows Server: o site no IIS (com o painel e o `modelo.json`) realiza a API e o painel; o banco no SQL Server realiza os objetos de dados |
| Hospedagem web, Banco de dados relacional e Mensageria MQTT | Serviços de tecnologia realizados pelo IIS, pelo SQL Server e pelo broker, que servem a API, o painel, o assinante e o ESP32 |
| Celular do professor, PC da coordenação e TV da escola | Aparelhos onde rodam o app, o terminal e o painel |
| Internet pública | Rede associada ao ESP32, ao broker, à VM e aos aparelhos |

## 5. Estado da implementação

Situação desta versão em 05/11/2026.

| Parte | Situação |
|---|---|
| Modelo de ML | Notebook executado. Árvore (profundidade 12, folha 24) escolhida pelo MAE de validação. No teste de 2025, MAE 0,909 °C (`ml/modelo.json`) |
| App do professor | Quatro telas em Expo. O modo simulado vem ligado; com `EXPO_PUBLIC_API_MOCK=0` o app chama a API |
| API | Rotas de salas, leituras, previsão, ocorrências, ações, revogação e demonstração. Painel em `wwwroot`. 55 testes automatizados da API aprovados em 07/11/2026 no `dotnet test api/TermometroDaSala.sln --no-restore`: 39 `[Fact]` e 16 casos `[InlineData]` em 3 `[Theory]` (`api/evidencias/dotnet-test.txt`) |
| Terminal da coordenação | Windows Forms: salas, ação, cadastro, revogação e CSV. Se a API cai, usa a reserva local. Separadamente da API, 6 testes aprovados em 07/11/2026 no `dotnet test desktop/TermometroDaSala.Desktop.sln --no-restore` (`api/evidencias/dotnet-test.txt`) |
| ESP32 | `sketch.ino`, `diagram.json` e `libraries.txt` para o Wokwi. O `esp32/README.md` descreve a simulação e o vetor de HMAC |
| Windows Server | Roteiro em `infra/README.md` e scripts `01-rede.ps1` a `09-teste-de-falha.ps1` |
| Relatório de riscos | `seguranca/RELATORIO-RISCOS.md`, com ameaças, controles e evidências da solução |

O que é real: os dados do INMET e a pessoa entrevistada. O que é simulado: as leituras do sensor no Wokwi, as leituras semeadas no modo demonstração, as ocorrências e o dia reproduzido da rua. Na tela, a etiqueta DEMONSTRAÇÃO marca a previsão que usa leitura simulada.

## 6. Decisões de arquitetura

**Por que MQTT.** O ESP32 no Wokwi sai para a internet pública, mas não alcança uma VM local. Com um broker no meio, os dois lados só fazem conexão de saída, e o servidor não precisa de porta aberta. O tópico é público de propósito, porque a segurança vem da assinatura. O custo é depender de um broker externo. Por isso existe o `POST /leituras`, com a mesma checagem, e o LED da porta segue funcionando sem rede.

**Por que o modelo em JSON.** Rodar Python dentro do IIS traria um segundo runtime só para uma conta. O notebook exporta a árvore escolhida em `modelo.json` (`tipo: arvore`), e a API em C# percorre a árvore. Dá para auditar o arquivo sem o notebook. O custo é trocar o arquivo sempre que o modelo for treinado de novo.

**Por que nenhum dado de aluno.** Os alunos são menores de idade, e dado que não existe não vaza. A unidade do sistema é a sala. O formulário pergunta "o que aconteceu", e não "com quem". A foto é da sala ou do equipamento, sem pessoas, e sai sem EXIF, para não levar localização. O painel público não mostra fotos. O custo é não acompanhar a saúde de cada aluno, o que nunca foi objetivo do sistema, e a identificação do professor fica só no aparelho dele.

**Por que HMAC por dispositivo.** Num tópico público, um token dentro da mensagem seria copiado por quem assinasse o tópico. Só a assinatura viaja, e a ordem estrita do `medidoEm` impede o reenvio. Como cada dispositivo tem seu segredo, vazar um não permite assinar por outro.

**Por que o nível é calculado duas vezes.** O ESP32 calcula para o LED funcionar sem internet. A API recalcula, e o valor dela é o oficial.

## 7. Público-alvo e estimativa de impacto

**Legenda:** **[MEDIDO]** é um número contado neste repositório, sobre dados públicos. **[PREMISSA]** é um valor assumido para a conta. **[META]** é um alvo que a equipe definiu para o piloto e que o próprio sistema mede.

### 7.1 Quantas pessoas o problema alcança

| Letra | Dado | Valor | Situação | Fonte |
|---|---|---|---|---|
| A | Alunos da rede estadual de SP | cerca de 3,5 milhões | Valor aproximado publicado pela Seduc-SP. Não é a matrícula inteira do Censo | Referência 1. Acesso em 05/11/2026 |
| A' | O mesmo contingente, noutra nota da Seduc | 3 milhões de estudantes | Frase lida na notícia de 20/12/2024. Arredondamento diferente do da página institucional | Referência 9. Acesso em 05/11/2026 |
| B | Alunos por turma | 35 | **[PREMISSA]** Valor de referência adotado para a conta. O INEP publica o indicador de média de alunos por turma, que pode substituir a premissa | Referência 4. Acesso em 05/11/2026 |
| C | Salas da rede estadual de SP sem climatização | 67.468 de 70.346 (95,91 %) | Tabulação dos microdados do Censo 2024. Não é um total publicado pelo Inep | Referências 10 e 11. Acesso em 05/11/2026 |

O item de climatização existe: o caderno de conceitos do Censo descreve sala climatizada como a que tem ar-condicionado, aquecedor ou climatizador em funcionamento (referência 11). A Seduc, em notícia de 13/02/2025, informa 595 escolas com todas as salas climatizadas (referência 12). Isso é escola, não sala, e não entra no lugar de C.

A conta de pessoas usa as matrículas e a parcela de salas. **[PREMISSA]** a parcela de salas vale para os alunos: o microdado não foi ponderado matrícula a matrícula. Com a base de 3.500.000:

3.500.000 × 67.468 / 70.346 = 3.356.807,8, isto é **3.356.808** alunos, cerca de **3,36 milhões**.

Com a base de 3 milhões da notícia de 20/12/2024, a mesma conta dá 2.877.263,8, isto é 2.877.264 alunos, cerca de 2,88 milhões. O pitch usa os 3,36 milhões e cita as duas bases.

**Turmas**, só com a premissa B: 3.500.000 ÷ 35 = **100.000 turmas**. A mesma parcela dá cerca de 95.900 turmas sem climatização. Contar 67.468 salas × 35 deixaria de fora quem estuda noutro turno.

**O piloto:** uma escola com [nº de salas da escola entrevistada] salas × 35 alunos × [nº de turnos] = [alunos por dia na escola].

Esta seção estima quantas pessoas estão expostas ao problema, e não quantas seriam protegidas. A redução real só pode ser medida num piloto, pelo indicador "aulas em Perigo sem ação".

**Meta de benefício do piloto [META].** Quatro semanas numa escola, com o sensor nas salas sem climatização. Hoje a linha de base é desconhecida, porque ninguém mede a sala, e a primeira semana serve para medi-la.

| Indicador | Como o sistema mede | Meta ao fim de 4 semanas |
|---|---|---|
| Aulas em Perigo sem ação registrada | Períodos de aula com a sala em Perigo sem ação gravada no terminal | Zero |
| Minutos entre o alerta e a ação | Diferença entre a primeira leitura em Perigo ou Cuidado extremo e o registro da ação | Mediana de até 15 minutos |
| Salas com leitura recente no horário de aula | Salas que receberam leitura nos últimos 10 minutos | Ao menos 95 % do tempo |
| Delta real de cada sala | Média da diferença entre sala e rua, medida com o termo-higrômetro | Medido em todas as salas do piloto, substituindo a premissa de +4 °C |

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
- 34 °C é um corte deste trabalho, e não a definição oficial de onda de calor. O INMET adota o critério da OMM: máximas diárias pelo menos 5 °C acima da média mensal, por no mínimo cinco dias consecutivos, numa área ampla (referência 5, acesso em 05/11/2026). Não foi localizada uma série do INMET com a contagem anual de ondas só na capital em 2022–2025. Os 21 dias da tabela não são essa contagem. Aplicar a definição à série A701 exigiria a normal climatológica de 30 anos, que não foi usada aqui.
- Os valores são os de cada hora cheia, e não a máxima do dia. O pico real pode ser maior.
- 278 horas do período (0,8 %) não têm leitura válida.
- A estação representa a capital, e não o estado. Por isso a frequência não foi multiplicada pelas turmas da rede.
- "Dia útil de aula" é premissa: segunda a sexta, de fevereiro a meados de dezembro, sem feriados, sem conferir o calendário escolar de cada ano.

### 7.3 Referências

| Nº | Fonte | Link | Situação |
|---|---|---|---|
| 1 | Seduc-SP, a secretaria: aproximadamente 3,5 milhões de alunos e 5,3 mil escolas | https://www.educacao.sp.gov.br/institucional/a-secretaria/ | Acesso em 05/11/2026 |
| 2 | INEP, Censo Escolar | https://www.gov.br/inep/pt-br/areas-de-atuacao/pesquisas-estatisticas-e-indicadores/censo-escolar | Acesso em 05/11/2026. Portal do levantamento |
| 3 | INEP, microdados do Censo Escolar | https://www.gov.br/inep/pt-br/acesso-a-informacao/dados-abertos/microdados/censo-escolar | Acesso em 05/11/2026. Base da tabulação da referência 10 |
| 4 | INEP, média de alunos por turma. Página atualizada em 03/07/2026 | https://www.gov.br/inep/pt-br/acesso-a-informacao/dados-abertos/indicadores-educacionais/media-de-alunos-por-turma | Acesso em 05/11/2026. B segue como premissa |
| 5 | INMET, definição de onda de calor (critério da OMM), notícia de 05/03/2025 | https://portal.inmet.gov.br/noticias/onda-de-calor-em-areas-do-centro-sul-do-pais-persiste-ate-sabado-8 | Acesso em 05/11/2026 |
| 6 | INMET, dados históricos (A701, 2022–2025) | https://portal.inmet.gov.br/dadoshistoricos | Dados contidos no repositório. Contagem feita em 04/11/2026 |
| 7 | ONU Brasil, Objetivos de Desenvolvimento Sustentável: metas 3.d, 4.a, 11.b e 13.1 | https://brasil.un.org/pt-br/sdgs | Texto das metas citadas na seção 4.1. Acesso em 6 nov. 2026 |
| 8 | NOAA, Weather Prediction Center: equação do índice de calor (Rothfusz, com os ajustes de umidade) | https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml | Fórmula implementada no ESP32, na API e em `ferramentas/indice_calor.py`; os casos do `CONTRATO.md` conferem as três. Acesso em 6 nov. 2026 |
| 9 | Seduc-SP, notícia de 20/12/2024: mais de 5 mil escolas e 3 milhões de estudantes | https://www.educacao.sp.gov.br/educacao-de-sao-paulo-anuncia-maior-investimento-da-historia-para-2025/ | Acesso em 05/11/2026. Frase lida no HTML da página |
| 10 | CIEPP, Almada, J. *Saunas de aula*, 2. ed., dez/2025. Censo 2024: SP estadual, 70.346 salas e 2.878 climatizadas (4,09 %) | https://www.ciepp.org/_files/ugd/25b93e_3d80dd7bd40241f199473a55747e208c.pdf | Acesso em 05/11/2026 |
| 11 | INEP, caderno de conceitos do Censo Escolar 2026: salas de aula climatizadas | https://download.inep.gov.br/publicacoes/institucionais/estatisticas_e_indicadores/cadernos_de_conceitos_2026.pdf | Acesso em 05/11/2026 |
| 12 | Seduc-SP, notícia de 13/02/2025: 595 escolas estaduais climatizadas | https://www.educacao.sp.gov.br/em-dois-anos-governo-de-sp-aumenta-em-mais-de-60-vezes-o-numero-de-escolas-climatizadas/ | Acesso em 05/11/2026. Unidade é escola, não sala |

## 8. Riscos

| Risco | Impacto | Tratamento |
|---|---|---|
| Quem tem o link do Wokwi vê o segredo de demonstração e forja leituras daquele dispositivo | Médio | Segredo por dispositivo e revogável. A decisão final é da coordenação, e o LED mede sozinho |
| Broker público fora do ar ou com o tópico inundado | Médio | `POST /leituras` como reserva. A sala aparece como "sem leitura recente" e o LED segue funcionando |
| Previsão errada no fim da tarde, porque o delta superestima | Baixo | Limitação declarada. O erro é para o lado da cautela, e a leitura ao vivo prevalece |
| Sensor na porta não representar o meio da sala | Médio | Calibrar com termo-higrômetro no piloto |
| O modelo não prevê acima do que viu: a rua nunca passou de Cuidado extremo nos dados | Médio | O Perigo da sala vem da medição e do delta. Treinar de novo a cada ano com dados novos |
| Foto de ocorrência com rosto de aluno | Alto | Aviso na tela, sem EXIF, foto fora do painel e acesso por id com a chave da coordenação, nunca por caminho |
| Assinante MQTT parar com o pool ocioso | Alto | Pool sempre ligado e recuperação automática, configurados pelo roteiro de Operating Systems. O teste de falha está em `infra/TESTE-DE-FALHA.md`, e o resultado na VM vai para `infra/EVIDENCIAS.md` |
| Coordenação sem tempo de olhar o terminal | Médio | Indicador de minutos entre alerta e ação, para mostrar o gargalo. Notificação como evolução |

## 9. Evolução (Fases E e F do TOGAF)

A Fase E (oportunidades e soluções) agrupa as lacunas em pacotes de trabalho, e a Fase F (planejamento da migração) ordena os patamares. No modelo, isso está na visão "Implementação e migração": cada horizonte é um Plateau, ligado ao seguinte por Triggering; cada pacote de trabalho (Work Package) realiza um patamar; e as lacunas (Gap) ficam entre os patamares que separam. O piloto realiza o objetivo "nenhuma aula em Perigo sem ação", e a rede de ensino realiza o "histórico por sala para priorizar obras".

![Visão Implementação e migração, exportada do Archi](visao-implementacao.png)

| Patamar | Lacuna até ele | Pacote de trabalho: o que muda |
|---|---|---|
| Escola hoje: sem medição na sala | Não se aplica | Ponto de partida |
| Protótipo desta versão (Wokwi e VM) | Medir a sala e registrar a ação | Construção do protótipo: o que este repositório entrega (seção 5) |
| Piloto em uma escola | Dado real de campo | Termo-higrômetro por uma semana, medindo o delta real. Sensor físico. Segredo gravado na memória do ESP32 |
| Mais de uma escola | Infraestrutura própria e segura | Broker próprio com usuário, senha e TLS. Busca do INMET ao vivo. Notificação no app. Login do terminal com o AD |
| Rede de ensino | Visão consolidada da rede | Relatório consolidado para a Diretoria de Ensino decidir onde climatizar primeiro. Novo treino anual do modelo. Modelo por sala, depois de haver histórico real |

Como o código usa a entidade neutra `Ambiente`, a mesma solução pode atender outros lugares quentes trocando dados, e não código.
