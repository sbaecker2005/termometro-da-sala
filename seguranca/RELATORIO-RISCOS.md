# Termômetro da Sala

## Relatório de Gestão de Riscos

**Equipe:** Termômetro da Sala  
**Integrantes:** Samuel Baecker - RM [preencher]; [preencher demais integrantes - RM]  
**Disciplina:** Cybersecurity  
**Instituição:** FIAP — Engenharia de Software, Global Solution 2026.2  
**Data da análise:** 05/11/2026

## Resumo executivo

O Termômetro da Sala mede o calor de salas de aula sem climatização (ESP32), envia as leituras assinadas por MQTT a uma API ASP.NET Core com SQL Server e apoia a coordenação na decisão de agir. O professor registra ocorrências com foto pelo app. Os ativos mais sensíveis são as **fotos de ocorrência** (podem expor menores, LGPD art. 14), os **segredos dos dispositivos** e o **servidor Windows**.

A análise seguiu a ISO/IEC 27005:2022 e a NIST SP 800-30 Rev. 1, com ameaças classificadas por STRIDE e vulnerabilidades ligadas ao OWASP Top 10 2021 e ao CWE. Foram identificados **14 riscos**. Sem controles, 6 eram críticos e 8 altos. Com os controles que o código já executa e que a suíte de testes comprova (40 de 40 testes aprovados em 05/11/2026), ficam **1 crítico, 4 altos, 8 moderados e 1 baixo**. C1 e C4 estão parciais (seção 6.1), e a matriz só desconta a parte que o código executa.

| Prioridade | Risco | Classe depois | O que segue aberto |
|---|---|---|---|
| 1 | R9 — acesso indevido por RDP ao servidor | Crítico (20) | GPO, auditoria e firewall do RDP têm roteiro (`infra/06-gpo.ps1` e passo 6 do `infra/README.md`), mas ainda não foram aplicados e comprovados no laboratório Windows Server (E11) |
| 2 | R14 — sem HSTS e sem `nosniff` | Alto (12) | O roteiro de infraestrutura cria binding e firewall 443, mas `Program.cs` não usa `UseHsts` nem envia `nosniff` |
| 3 | R8 — negação de serviço no MQTT | Alto (12) | A cota de 30/min vale só para HTTP; o assinante MQTT descarta mensagem acima de 1024 bytes, mas não limita volume |
| 4 | R6 — foto que identifica aluno (OSINT/LGPD) | Alto (10) | O upload remove EXIF, mas um rosto na imagem só é barrado pela revisão humana |
| 5 | R13 — MQTT na porta 1883 sem TLS | Alto (10) | Leituras trafegam em claro no broker público; aceito só no laboratório com dados simulados |
| 6 | R12 — foto lida sem autorização | Moderado (8) | `GET /ocorrencias/{id}/foto` exige só o GUID da ocorrência; lacuna declarada |

Nos arquivos da versão atual do repositório público e no histórico do git não há credencial de produção nem dado de aluno (exercício de OSINT na seção 4.1 e `evidencias/osint.txt`, item 8). Três cenários de ataque (SQLi, LFI e OSINT) foram executados passo a passo contra a API local com banco de teste: os 23 passos conferem, sendo 16 ataques barrados, 2 lacunas declaradas confirmadas (S2.10, foto lida sem chave, R12; S3.1, forja com o vetor de teste público, R7) e 5 passos de referência ou da coordenação (seção 4.2). Os segredos `segredo-de-teste-sala-01` a `-04` são **valores de teste, públicos de propósito e revogáveis**, e serão revogados após a avaliação de 19/11/2026.

## 1. Escopo e ficha resumida

| Campo | Descrição |
|---|---|
| Projeto | Termômetro da Sala |
| Local de referência | São Paulo; recorte de uma sala de escola pública sem climatização |
| Beneficiários e operadores | Professor e turma; coordenação pedagógica decide e registra as ações |
| Problema | Calor dentro da sala sem medição e sem informação antecipada para a coordenação |
| Funcionamento previsto | ESP32 mede temperatura e umidade, sinaliza localmente e publica por MQTT; API valida, armazena e fornece nível atual e previsão de duas horas; app registra ocorrências com foto; desktop registra ações e revoga sensores |
| Tecnologia prevista | ESP32/DHT22 no Wokwi, MQTT público, ASP.NET Core, EF Core/SQL Server, Expo/React Native, Windows Forms e IIS/Windows Server no domínio termometro.local |
| Indicadores | Aulas em nível Perigo sem ação registrada e tempo entre alerta e ação |
| ODS | 3, 4, 11 e 13: saúde, ambiente escolar seguro, adaptação urbana e resiliência climática |
| Dados | Medições por ambiente, ocorrências simuladas e meteorologia pública do INMET; nenhum dado de aluno deve ser coletado |

O escopo inclui banco, API REST, assinante MQTT, servidor Windows, dispositivo, configuração de segredos, contas administrativas e imagens. Abrange SQL Injection (SQLi), inclusão/leitura indevida de arquivo local (LFI), exposição por fontes abertas (OSINT), acesso remoto e disponibilidade. Não é certificação de segurança nem validação clínica dos limites de calor.

**Estado observado em 05/11/2026:** a API tem salas, leituras assinadas, ocorrências, ações, revogação e previsão. O HMAC usa segredo distinto por dispositivo. A coordenação escreve com `X-Chave-Coordenacao`. As rotas HTTP de escrita têm cota de 30 pedidos por minuto por IP; a ingestão MQTT não passa por ela. O upload regrava o JPEG sem APP1. O app do professor continua com simulação por padrão (`app/app.config.ts:5`). GPO, firewall, nmap e IIS têm roteiro na pasta `infra/`, mas a aplicação e a coleta no laboratório Windows Server ainda não foram feitas. As saídas locais estão em [evidencias/](evidencias/).

## 2. Método

A análise segue o processo de gestão de riscos da **ISO/IEC 27005:2022** e as etapas de avaliação da **NIST SP 800-30 Rev. 1**:

| Etapa (ISO/IEC 27005) | O que foi feito (NIST SP 800-30, etapa 2, tarefas 2-1 a 2-6) | Seção |
|---|---|---|
| Estabelecimento do contexto | Escopo, partes interessadas e critérios de aceitação do risco | 1 e 5 |
| Identificação | Inventário dos ativos; agentes (fontes) de ameaça; fluxo de dados e fronteiras de confiança; eventos de ameaça; vulnerabilidades e condições predisponentes | 3 e 4 |
| Análise | Cenários de ataque passo a passo; probabilidade (1 a 5) e impacto (1 a 5) de cada evento, antes e depois dos controles | 4.2 e 5 |
| Avaliação | Pontuação P × I comparada com o critério de aceitação | 5 |
| Tratamento | Controles com critério de aceitação, tipo, controle da ISO/IEC 27001:2022 Anexo A, custo e responsável | 6 |
| Monitoramento e comunicação | Riscos residuais, gatilhos de reavaliação e evidências | 7 e 8 |

Classificações usadas:

- **STRIDE** para a natureza da ameaça: S falsificação de identidade, T adulteração, R repúdio, I divulgação de informação, D negação de serviço, E elevação de privilégio.
- **OWASP Top 10 2021** (e OWASP API Security Top 10 2023 quando o Top 10 não tem categoria) e **CWE** para a vulnerabilidade.
- **ISO/IEC 27001:2022, Anexo A** para os controles.

A escala é semiquantitativa, como a da NIST SP 800-30 (cinco níveis). Os valores são julgamento da equipe a partir do código e da arquitetura, sem frequência de incidentes medida. O impacto pode continuar elevado mesmo quando a probabilidade cai. Opções de tratamento da ISO/IEC 27005: modificar (mitigar com controle), reter (aceitar), evitar ou compartilhar.

## 3. Inventário e classificação dos ativos

Escala de criticidade de confidencialidade (C), integridade (I) e disponibilidade (D): **baixa**, **média** ou **alta**, conforme o dano da exposição, alteração ou indisponibilidade. Criticidade não é probabilidade de ataque.

| ID | Ativo e conteúdo | C | I | D | Responsável funcional e justificativa | Cidadão afetado |
|---|---|---|---|---|---|---|
| A1 | Banco SQL Server: ambientes, dispositivos, leituras, previsões, ocorrências e ações | Alta | Alta | Alta | Backend/coordenação; alteração muda alertas e histórico; segredos e fotos exigem restrição | Aluno (alerta errado ou histórico falso); coordenação (decisão) |
| A2 | API REST e assinante MQTT | Média | Alta | Alta | Backend; é a fronteira de validação entre internet, dispositivo e banco | Aluno e professor (alerta ausente ou errado) |
| A3 | Windows Server, IIS, AD, disco de fotos e logs | Alta | Alta | Alta | Infraestrutura; comprometimento dá acesso a serviços, arquivos e contas | Todos: aluno, professor e coordenação |
| A4 | ESP32, sensor e LED local | Baixa | Alta | Alta | Dispositivo/coordenação; leitura ou sinal incorreto prejudica a decisão; LED deve continuar sem rede | Aluno e professor na sala (sinal local) |
| A5 | Segredos HMAC por dispositivo, conexão do banco e credenciais administrativas | Alta | Alta | Alta | Backend/infraestrutura; permitem forja ou tomada de controle; indisponibilidade impede operação | Aluno (leitura forjada esconde calor real) |
| A6 | Contas da coordenação e grupos administrativos/RDP | Alta | Alta | Alta | Coordenação/infraestrutura; habilitam decisões, revogação e administração | Coordenação; aluno indiretamente |
| A7 | Fotos de ocorrências e metadados | Alta | Alta | Média | Aplicativo/backend; podem identificar pessoas e local, embora isso seja proibido pelo projeto | Aluno menor de idade (identificação); professor |
| A8 | App, desktop e painel público | Média | Alta | Média | Interfaces; painel mostra somente informação por sala, sem fotos; dados do professor ficam no aparelho | Professor (nome e escola no aparelho); coordenação |

O app armazena nome e escola do professor localmente (`app/services/professor.ts:6` e `:19`); isso continua sendo dado pessoal de adulto, sem equivaler a autenticação. O objetivo de não coletar dados de aluno não significa ausência de qualquer dado pessoal.

### 3.1 Agentes de ameaça

Quem pode atacar o Termômetro da Sala, por que, com que meios e qual cidadão sofre o dano. A capacidade segue a escala da NIST SP 800-30 (baixa, média, alta).

| ID | Agente | Motivação | Capacidade | Ativos-alvo | Dano ao cidadão | Riscos |
|---|---|---|---|---|---|---|
| AG1 | Aluno curioso ou que quer liberar a turma | Sair mais cedo, brincadeira, testar o sistema | Baixa a média: celular, cliente MQTT gratuito, leitura do repositório público | A4, A5, broker | Alerta falso interrompe a aula, ou calor real fica escondido e colegas passam mal | R1, R2, R7, R13 |
| AG2 | Pessoa de dentro da escola (funcionário ou terceirizado com acesso à rede ou ao servidor) | Esconder falha, evitar trabalho, curiosidade sobre fotos | Média: rede interna, conta do AD, acesso físico ao servidor | A1, A3, A6, A7 | Fotos de menores expostas; ações e revogações falsificadas | R6, R9, R11, R12 |
| AG3 | Atacante oportunista na internet (robôs de varredura) | Ganho genérico: sequestro de dados, servidor para botnet | Média a alta, automatizada: varredura de portas, cargas prontas de SQLi e LFI | A1, A2, A3 | Sistema fora do ar no dia quente; banco vazado ou apagado | R3, R4, R5, R9, R14 |
| AG4 | Coletor OSINT (golpista ou pessoa que monta perfil de crianças) | Identificar e localizar alunos, escola e rotina | Baixa técnica, muita paciência: busca no GitHub, no Wokwi, em fotos e metadados | A7, A8, repositório público | Menor identificado e localizado (LGPD art. 14) | R6, R7, R10 |
| AG5 | Robô ou anônimo no tópico MQTT público | Spam, teste de brokers abertos, sem alvo específico | Publica e assina qualquer tópico sem autenticação | Broker, A2 | Leituras falsas ou ausentes na tela da coordenação | R1, R8, R13 |

A banca e a equipe usam os vetores de teste públicos de propósito (seção 4.1). Elas não são agentes de ameaça, mas o mesmo valor serve ao AG1 e ao AG4 até a revogação.

### 3.2 Fluxo de dados e fronteiras de confiança

![Fluxo de dados e fronteiras de confiança](fluxo-dados.svg)

Cada linha tracejada é uma fronteira de confiança: tudo que a atravessa é tratado como não confiável até a API validar.

| Fluxo | De → para | Dado | Fronteira atravessada | Controle na travessia | Riscos |
|---|---|---|---|---|---|
| 1 | ESP32 → broker público | Leitura com sala, dispositivo, temperatura, umidade, horário e HMAC; o segredo não trafega | Sala de aula → internet | C1 (assinatura por dispositivo) | R1, R2, R7, R13 |
| 2 | Broker → assinante da API (conexão de saída) | A mesma mensagem, até 1024 bytes | Internet → laboratório | C1 e C4 (1024 bytes); nenhuma porta aberta no servidor para MQTT (C8) | R1, R2, R8, R13 |
| 3 | App do professor → API | `POST /ocorrencias` (texto e foto) e `GET /ambientes` | Cliente → servidor | C3, C4, C9 e cota de escrita | R4, R5, R6, R12 |
| 4 | Desktop da coordenação → API | `POST /acoes`, `POST /ambientes` e `POST /dispositivos/{id}/revogar` com `X-Chave-Coordenacao` | Cliente → servidor | C10, cota de escrita, HTTPS do C8 (roteiro) | R12, R14 |
| 5 | Painel web → API | `GET /ambientes`, leituras e previsão, sem fotos | Navegador → servidor | C2 | R3, R14 |
| 6 | API → SQL Server | Comandos do EF Core com parâmetros | Nenhuma: mesmo servidor; controle pela conta do pool e ACL | C2; porta 1433 só interna (C8) | R3 |
| 7 | API → pasta de fotos | JPEG sem APP1, nome GUID gerado no servidor | Nenhuma: mesmo servidor; controle pela conta do pool e ACL | C3, C9; NTFS e SACL (C7) | R4, R5, R6 |
| 8 | Estação administrativa → Windows Server | RDP 3389 com conta do AD | Rede administrativa → servidor | C7; firewall do C8 | R9 |

`POST /leituras` por HTTP é o caminho alternativo dos fluxos 1 e 2 e passa pelo mesmo `ServicoLeituras.Receber` (C1).

## 4. Ameaças, vulnerabilidades e consequências

| Risco | Ativos | Agente e cenário concreto | Vulnerabilidade a impedir | Consequência | STRIDE | OWASP 2021 | CWE |
|---|---|---|---|---|---|---|---|
| R1 — leitura forjada | A1, A2, A4 | Pessoa no tópico público publica calor falso para liberar a turma ou ocultar calor real | Ingestão sem assinatura por dispositivo ou sem associação dispositivo/ambiente | Alerta e previsão incorretos; decisão operacional prejudicada | S, T | A08 | CWE-345 |
| R2 — reenvio | A1, A2 | Ouvinte copia uma mensagem válida e a publica novamente, inclusive após reinício da API | Aceitar timestamp igual/anterior; estado antirreenvio só em memória; corrida entre MQTT e HTTP | Histórico duplicado ou leitura antiga tratada como atual | S, T | A07 | CWE-294 |
| R3 — SQLi na consulta de leituras | A1, A2 | Cliente manda apóstrofo e `OR 1=1` no id de `GET /ambientes/{id}/leituras` | Concatenar o id em SQL | Leitura ou alteração indevida de registros | T, I | A03 | CWE-89 |
| R4 — LFI no download | A3, A7 | Cliente tenta `../`, caminho absoluto ou nome de arquivo do Windows no identificador da foto | Usar caminho fornecido pelo cliente em leitura de disco | Exposição de configuração, credenciais ou arquivos do servidor | I | A01 | CWE-22 |
| R5 — upload malicioso | A2, A3, A7 | Cliente envia script como JPEG, nome com travessia ou arquivo grande | Confiar em extensão/MIME, permitir execução no diretório de imagens ou não limitar tamanho | Execução, sobrescrita de arquivos ou disco cheio | T, E, D | A04 | CWE-434 |
| R6 — OSINT e LGPD | A7, A8 | Terceiro reconhece aluno, uniforme, placa, documento ou localização em foto pública | Fotografar pessoas, enviar EXIF/localização ou publicar fotos no mural/repositório | Identificação e exposição de menor de idade | I | A01 | CWE-359, CWE-212 |
| R7 — segredo do Wokwi | A4, A5 | Visitante do projeto copia o segredo e assina novas mensagens daquele dispositivo | Link do simulador permite ver todos os arquivos, inclusive secrets.h | HMAC válido produzido por terceiro até revogação | I, S | A07 | CWE-798 |
| R8 — negação de serviço | A2, A4 | Terceiro ocupa o tópico com lixo; broker público cai ou limita conexões | Dependência de serviço público sem garantia de capacidade; mensagens MQTT sem limite de volume (o tamanho já é limitado a 1024 bytes em `AssinanteMqtt.cs:12`) | Falta de medições recentes e atraso na resposta da coordenação | D | API4:2023 | CWE-770 |
| R9 — acesso indevido/RDP | A1, A3, A6 | Conta comum ou atacante tenta logon remoto no servidor | RDP exposto, permissões amplas, senha fraca e auditoria desativada | Tomada do servidor e dos dados | S, E | A07 | CWE-307, CWE-269 |
| R10 — credenciais versionadas | A1, A3, A5 | Leitor de repositório, histórico, print ou log recupera segredo | Segredo em configuração rastreada; acreditar que ignorar arquivo remove histórico | Forja de leituras, acesso ao banco ou servidor | I | A07 | CWE-798 |
| R11 — semeadura indevida | A1, A2 | Cliente chama POST /demo/semear no ambiente de produção | Rota de carga simulada sem HMAC, presente em todos os ambientes, que grava também fora de Demo | Histórico artificial influencia previsão real | T | A05 | CWE-489 |
| R12 — operações administrativas sem controle | A2, A6, A7 | Cliente direto tenta registrar ação, revogar sensor ou ler foto sem autorização | Confundir identificação local do professor e middleware com autenticação/autorização | Decisões falsas, revogação indevida e acesso a imagens | E, S, I | A01 | CWE-862, CWE-639 |
| R13 — MQTT sem TLS | A2, A4 | Qualquer pessoa assina o tópico `fiap-gs/termometro-da-sala/leituras` em `broker.hivemq.com:1883` e lê ou altera mensagens em trânsito | Porta 1883 sem TLS e sem autenticação no broker (`api/TermometroDaSala.Api/appsettings.json:6-7`, `esp32/sketch.ino:162`) | Leituras e identificadores de dispositivo expostos; base para R1, R2 e R8 | I, T | A02 | CWE-319 |
| R14 — sem HSTS e cabeçalhos de segurança | A2, A6, A8 | Na rede da escola, alguém força o navegador a usar HTTP (rebaixamento) e captura a `X-Chave-Coordenacao`, ou explora a interpretação de conteúdo pelo navegador | `Program.cs` não chama `UseHsts` nem envia `X-Content-Type-Options: nosniff`. O roteiro de infraestrutura cria binding HTTPS 443 e regra de firewall 443 (`infra/README.md`, seção "HTTPS no laboratório"), mas mantém HTTP 80 aberto (`infra/04-iis.ps1:49`) e a API só redireciona (`Program.cs:62`); sem HSTS, o primeiro pedido em HTTP pode ser interceptado | Chave da coordenação capturada; ações e revogações falsas | I, T | A05 | CWE-319, CWE-693 |

`UseAuthorization()` em `api/TermometroDaSala.Api/Program.cs:67` sozinho não protege as rotas. A escrita da coordenação passa pelo filtro `ExigeChaveCoordenacaoAttribute`. Leitura de foto e `POST /ocorrencias` seguem sem essa chave.

### 4.1 Exercício de OSINT sobre o próprio projeto

Em 05/11/2026 a equipe procurou, na cópia local do repositório público, o que um terceiro descobre sem acesso privilegiado. Comandos e saídas estão em [evidencias/osint.txt](evidencias/osint.txt).

| Achado | Onde | Decisão |
|---|---|---|
| Segredos `segredo-de-teste-sala-01` a `-04` (56 ocorrências em 21 arquivos, contando os próprios roteiros que os citam) | `api/TermometroDaSala.Api/Dados/SalaDb.cs:65-68`, migrações, testes, `esp32/secrets.exemplo.h`, `CONTRATO.md` | **Valores de teste**, publicados de propósito como vetor do contrato HMAC para a banca reproduzir a assinatura. Valem só para os quatro dispositivos semeados do laboratório e nunca são chave de produção. A coordenação revoga os quatro com `POST /dispositivos/{id}/revogar` após a avaliação; um uso real cadastra dispositivos novos com segredo gerado fora do repositório |
| Broker e tópico MQTT públicos | `appsettings.json:6-8`, `esp32/sketch.ino:162` | Aceito só no laboratório (R13); leitura assinada (C1) e sem dado pessoal |
| Chave da coordenação e senha de banco | Só o valor de teste `chave-teste` em `api/TermometroDaSala.Api.Tests/ApiTestes.cs:544` e `:561`, usado pela suíte automatizada; `api/insomnia.json` traz o campo vazio e `api/README.md:215` usa variável de ambiente | **Valor de teste**, público como os vetores, nunca configurado no IIS (a chave do laboratório fica só no pool, `infra/04-iis.ps1:72-86`). Histórico conferido no item 8 |
| Nomes de pessoas | Só o nome do integrante, exigido nas entregas; nenhum e-mail, telefone, aluno ou entrevistado | Manter apenas nome e RM dos integrantes |
| Escola e bairro | Apenas marcadores a preencher | Ao preencher, nunca citar turma, sala real ou horário de aula |
| Fotos | Nenhuma foto de sala ou ocorrência; `api/TermometroDaSala.Api/fotos/` está no `.gitignore` | Prints só de sala vazia ou equipamento |

A busca no histórico do git foi executada em 05/11/2026 (`evidencias/osint.txt`, item 8): só aparecem os vetores de teste do item 1 e o valor de teste `chave-teste` da suíte automatizada (`ApiTestes.cs:544` e `:561`); `esp32/secrets.h` nunca foi commitado e nenhuma chave da coordenação de uso real, senha, `.env` ou certificado entrou em commit algum. A conferência do link do Wokwi (item 9) fica para a equipe, depois de publicado.

### 4.2 Cenários de ataque passo a passo

Os três cenários pedidos no enunciado foram modelados e executados em 05/11/2026 só contra a própria API, com o script [cenarios-ataque.ps1](cenarios-ataque.ps1). A API rodou com o mesmo pipeline dos testes automatizados: ambiente de teste, banco SQLite em memória com a semente das quatro salas e MQTT desligado. Saída completa e trechos do log em [evidencias/cenarios-ataque.txt](evidencias/cenarios-ataque.txt): **23 passos, os 23 com o resultado esperado**: 16 ataques barrados (S1.3 a S1.7, S2.1 a S2.8, S3.3, S3.4 e S3.6), 2 lacunas declaradas e confirmadas (S2.10, R12; S3.1, R7) e 5 passos de referência ou da coordenação (S1.1, S1.2, S2.9, S3.2 e S3.5). O script sai com o número de passos que divergem. No laboratório, a equipe roda o mesmo script contra a API publicada no IIS, com `-Api https://api.termometro.local` (anexo, linhas S1 a S3).

**Cenário 1: SQL Injection na consulta de leituras (AG3; R3)**

O atacante procura um parâmetro que vá para o banco. `GET /ambientes` é público e mostra os ids das salas; o id entra na rota `GET /ambientes/{id}/leituras`.

| Passo | Ação do atacante | Onde é barrado | Esperado | Obtido |
|---|---|---|---|---|
| S1.1 | Grava uma leitura legítima na sala-01, para ter o que roubar, e outra na sala-04, para a linha de base | Não é ataque: leituras assinadas | 201 e 201 | 201 e 201 |
| S1.2 | Linha de base: lê as leituras da sala-04 | Não é ataque | 200, só a sala-04, ao menos 1 leitura | 200, 1 leitura |
| S1.3 | Id `sala-04' OR 1=1 --` para listar todas as salas | C2: `AnyAsync(a => a.Id == id)` em `LeiturasController.cs:18`; o id vai inteiro como parâmetro `@__id_0` | 404, nada de outra sala | 404; log com `@__id_0` de 18 caracteres |
| S1.4 | Id `x' UNION SELECT Id, Segredo FROM Dispositivos --` para roubar os segredos | C2, mesma linha | 404, nenhum segredo | 404; `@__id_0` de 48 caracteres |
| S1.5 | Id `sala-04'; DROP TABLE Leituras; --` para apagar o histórico | C2, mesma linha | 404 | 404; `@__id_0` de 33 caracteres |
| S1.6 | Injeção no parâmetro `formato=csv' OR '1'='1` | O parâmetro só escolhe JSON ou CSV (`LeiturasController.cs:20`) e nunca chega ao SQL | 200, as mesmas leituras da sala-04 | 200, 1 leitura, a mesma do S1.2 |
| S1.7 | Confere se o `DROP` funcionou | — | 200 com a leitura do S1.1 | 200, 1 leitura: tabela intacta |

Onde o ataque para: no primeiro comando SQL. O texto do atacante nunca vira SQL, só valor de parâmetro (CWE-89 tratada). O teste `ApiTestes.ConsultaDeLeiturasParametrizada` repete a prova com o provedor do SQL Server (`evidencias/consulta-parametrizada.txt`).

**Cenário 2: LFI e upload malicioso nas fotos (AG3 e AG2; R4 e R5)**

O atacante quer ler arquivos do servidor (`appsettings.json`, `web.config`, `win.ini`) pela rota de foto ou gravar um arquivo executável no site.

| Passo | Ação do atacante | Onde é barrado | Esperado | Obtido |
|---|---|---|---|---|
| S2.1 | `GET /appsettings.json` | Só `wwwroot` é servido (`Program.cs:64-65`) | 404 | 404 |
| S2.2 | `GET /../appsettings.json` sem normalizar a URL | No ensaio local, barrado pelo HTTP.sys do encaminhador de coleta; sem ele, o Kestrel normaliza para `/appsettings.json` (404, como no S2.1). A execução contra o IIS no laboratório é o que confirma | 4xx, sem o arquivo | 403 |
| S2.3 | `GET /ocorrencias/..%2F..%2Fappsettings.json/foto` | No ensaio local, barrado pelo HTTP.sys do encaminhador de coleta; sem ele, a rota `{id:guid}` recusa com 404 (`OcorrenciasController.cs:42`). A execução contra o IIS no laboratório é o que confirma | 4xx, sem o arquivo | 403 |
| S2.4 | `GET /ocorrencias/C%3A%5CWindows%5Cwin.ini/foto` | Rota `{id:guid}` (`OcorrenciasController.cs:42`) | 404 | 404, 0 bytes |
| S2.5 | GUID inventado | Id resolvido no banco (`:45-46`); o caminho do disco nunca vem do cliente (`:47`) | 404 | 404 |
| S2.6 | Upload de JPEG válido com nome `../../wwwroot/index.html` | Nome com `..`, `/` ou `\` recusado (`:22-23`) | 400 `nome-invalido` | 400 `nome-invalido` |
| S2.7 | Upload com nome `..\..\web.config` | Mesma regra | 400 `nome-invalido` | 400 `nome-invalido` |
| S2.8 | Página ASPX enviada como `image/jpeg` | Bytes `FF D8 FF` conferidos (`:24` e `:58-64`) | 400 `nao-jpeg` | 400 `nao-jpeg` |
| S2.9 | Upload legítimo | Nome GUID gerado no servidor (`:31`), fora do site no IIS (`infra/04-iis.ps1:85`) | 201 com GUID | 201 com GUID |
| S2.10 | Lê a foto do S2.9 sem chave | **Não barrado:** lacuna R12 declarada | 200 hoje (alvo: 401) | 200 |

Onde o ataque para: na rota (só GUID), no banco (o id vira nome de arquivo gerado pelo servidor) e na validação do upload. O S2.10 mostra o que segue aberto: quem conhece o GUID de uma ocorrência baixa a foto (seção 6, C10).

**Cenário 3: OSINT sobre o projeto (AG4 e AG1; R6, R7 e R10)**

O atacante só usa o que está publicado: o repositório no GitHub, o link do Wokwi e as fotos.

| Passo | Ação do atacante | Onde é barrado | Esperado | Obtido |
|---|---|---|---|---|
| S3.0a | Lê o repositório e acha `segredo-de-teste-sala-0N` (`SalaDb.cs:65-68`) | **Não barrado:** vetores de teste públicos de propósito | Encontrado | Encontrado (`evidencias/osint.txt`, item 1) |
| S3.0b | Procura no histórico do git um segredo real, `secrets.h`, `.env` ou a chave da coordenação | C6: segredos fora do repositório desde o primeiro commit | Nada além dos valores de teste | Só os vetores e o valor de teste `chave-teste` dos testes automatizados (`osint.txt`, item 8) |
| S3.1 | Forja uma leitura da sala-04 com o vetor copiado | **Não barrado até a revogação:** risco R7 aceito no laboratório | 201 | 201 |
| S3.2 e S3.3 | Envia foto com EXIF de localização (`GPS-TESTE`) e baixa de volta para ver se a localização sobrou | C9: `SemApp1` regrava o JPEG sem APP1 (`OcorrenciasController.cs:28` e `:69`) | 200 sem `GPS-TESTE` | 200, `GPS-TESTE` ausente |
| S3.4 | Tenta revogar o sensor da sala para esconder o calor | C10: `ExigeChaveCoordenacaoAttribute.cs:20`, antes de qualquer acesso ao banco | 401 `chave-invalida` | 401 `chave-invalida` |
| S3.5 | A coordenação responde ao vazamento e revoga `esp32-sala-04` | C6: `DispositivosController.cs:20` | 200, `ativo: false` | 200, `ativo: false` |
| S3.6 | Repete a forja | C1: `dispositivo-revogado` (`Contrato.cs:102`) | 400 `dispositivo-revogado` | 400 `dispositivo-revogado` |
| S3.7 | Abre o link do Wokwi e lê o `secrets.h` | **Não barrado:** só o vetor da sala-03 fica visível | Só o vetor da sala-03 | A executar pela equipe depois de publicar (`osint.txt`, item 9) |
| S3.8 | Procura rosto, uniforme ou nome de aluno numa foto | **Não barrado por código:** revisão humana (C9) | Nenhuma pessoa nas fotos entregues | A executar pela equipe a cada entrega (E14) |

Onde o ataque para: o repositório não entrega nada além dos vetores de teste, e cada vetor é desligado pela revogação. O vazamento por imagem só é barrado em parte: a localização sai no upload, o rosto depende de revisão humana (R6 continua alto).

## 5. Matriz de risco: probabilidade × impacto

Probabilidade (P): 1 rara, 2 improvável, 3 possível, 4 provável, 5 quase certa. Impacto (I): 1 desprezível, 2 pequeno/reversível, 3 interrupção localizada, 4 perda relevante ou decisão operacional incorreta, 5 comprometimento amplo ou exposição de menores. Pontuação **P × I**: 1–4 baixo; 5–9 moderado; 10–16 alto; 17–25 crítico.

“Antes” estima o cenário sem o conjunto de controles. “Depois” usa só os controles que o código já executa e que a suíte de testes comprova (HMAC por dispositivo, consulta LINQ, nome GUID, validação, 404 fora do Demo, chave da coordenação, cota das rotas HTTP de escrita e remoção de APP1). A parte planejada de C1 (atualização atômica) e de C4 (decodificação dos pixels), C7, a parte de rede de C8, C11 e C12 não entram nessa conta enquanto não houver código ou evidência no laboratório; por isso R9 continua crítico, R8, R13 e R14 continuam altos e R2 não desce a baixo.

| ID | Antes P | Antes I | Antes P×I / classe | Controles | Depois P | Depois I | Depois P×I / classe |
|---|---:|---:|---|---|---:|---:|---|
| R1 | 4 | 4 | 16 alto | C1 | 2 | 4 | 8 moderado |
| R2 | 4 | 4 | 16 alto | C1 (parcial: reenvio sequencial recusado; duplicata concorrente sem trava) | 2 | 4 | 8 moderado |
| R3 | 3 | 5 | 15 alto | C2 | 1 | 5 | 5 moderado |
| R4 | 3 | 5 | 15 alto | C3 | 1 | 5 | 5 moderado |
| R5 | 4 | 5 | 20 crítico | C3, C4 (parcial: estrutura JPEG e 5 MB, sem decodificar pixels) e cota de escrita | 2 | 4 | 8 moderado |
| R6 | 4 | 5 | 20 crítico | C9 | 2 | 5 | 10 alto |
| R7 | 5 | 4 | 20 crítico | C1, C6 | 3 | 3 | 9 moderado |
| R8 | 4 | 4 | 16 alto | C1 recusa mensagem sem assinatura válida sem gravar; C4 descarta mensagem MQTT acima de 1024 bytes; a cota de 30/min não cobre MQTT, não há limite de volume e o broker público permanece | 4 | 3 | 12 alto |
| R9 | 4 | 5 | 20 crítico | C7, a comprovar no laboratório | 4 | 5 | 20 crítico |
| R10 | 3 | 5 | 15 alto | C6 | 2 | 4 | 8 moderado |
| R11 | 3 | 4 | 12 alto | C5 | 1 | 4 | 4 baixo |
| R12 | 4 | 5 | 20 crítico | C10 | 2 | 4 | 8 moderado |
| R13 | 5 | 4 | 20 crítico | C1 (assinatura); C12 pendente | 5 | 2 | 10 alto |
| R14 | 3 | 4 | 12 alto | C11 pendente | 3 | 4 | 12 alto |

Critério de aceitação: não se aceita exposição real de menores nem produção aberta com risco alto ou crítico. O recorte demonstrativo reduz o impacto de R7, R8 e R13 por usar dados simulados, decisão humana e sinalização local; fora desse recorte a matriz precisa ser refeita.

## 6. Controles e comprovação

| ID | Controle e critério de aceitação | Onde está implementado / estado observado | Evidência a anexar |
|---|---|---|---|
| C1 | HMAC-SHA256 por dispositivo; bytes UTF-8 `ambienteId\|dispositivo\|temperaturaC\|umidadePct\|medidoEm`, temperatura com uma casa e cultura invariável, comparação em tempo constante. Validar vínculo com ambiente, dispositivo ativo, timestamp estritamente crescente e persistido; rejeitar mais de 10 min atrás ou 1 min à frente. HTTP e MQTT devem usar a mesma regra; atualização atômica impede duplicata concorrente (planejada) | `Assinatura.Confere` e `FixedTimeEquals` em `api/TermometroDaSala.Api/Dominio/Contrato.cs:71` e `:79`. Motivos em `ValidadorLeitura.Motivo` (`Contrato.cs:98`). HTTP em `LeiturasController.cs:41`; MQTT em `AssinanteMqtt.cs:70`; os dois chamam `ServicoLeituras.Receber` (`ServicoLeituras.cs:26`), que registra a recusa em `:43`. Segredo distinto em `SalaDb.cs:65-68` e na migração `Dados/Migrations/20261105050515_SegredosPorDispositivo.cs:13`. Testes: `VetoresTestes.HmacDoContrato`, `ApiTestes.RecusasELeituraAceita`, `ApiTestes.SegredoDeOutraSalaNaoAssina`. **Parcial:** não há token de concorrência nem atualização condicional. `Receber` lê o dispositivo (`ServicoLeituras.cs:28`), valida e só depois grava a leitura e o `UltimoMedidoEm` (`:67-69`); duas cópias simultâneas da mesma leitura (MQTT e HTTP, por exemplo) podem passar pela checagem antes de qualquer gravação. Planejado: usar `UltimoMedidoEm` como token de concorrência ou um `UPDATE` condicional, com teste de duas gravações paralelas | E02–E04, `evidencias/recusas-log.txt` |
| C2 | Consultas LINQ/EF; nunca concatenar o id em SQL. O texto do cliente vai como parâmetro, nunca dentro do SQL | `GET /ambientes/{id}/leituras` em `LeiturasController.cs:18-19`: `Where(l => l.AmbienteId == id)`. Não há SQL montado no controlador. `ToQueryString()` (SQLite do teste e provedor SQL Server, sem abrir conexão) mostra o `WHERE` comparando com um parâmetro `@...`; o texto `OR 1=1` não aparece no comando. O GET com esse id responde 404 e não lista as outras salas. Teste: `ApiTestes.ConsultaDeLeiturasParametrizada` | E05, `evidencias/consulta-parametrizada.txt` |
| C3 | Gerar nome GUID no servidor; guardar imagem fora de diretório executável e servir por id de ocorrência, resolvido no banco; nunca aceitar caminho do cliente. GUID evita colisão, não substitui autorização | Nome em `OcorrenciasController.cs:31`. A rota de leitura só aceita GUID (`{id:guid}`, `:42`) e usa `Path.GetFileName` em `:47`. Nome com `..`, `/` ou `\` recusado em `:22-23` (`nome-invalido`). O `foto.jpg` de `app/app/salas/[id]/ocorrencia.tsx:39` é só o nome enviado pelo cliente. Pasta de fotos fora do site no IIS: `infra/04-iis.ps1:85`. Teste: `ApiTestes.UploadRecusaOQueNaoEJpegENomeComDoisPontos` (`../x.jpg`) | E06–E07, `evidencias/upload-travessia.txt` |
| C4 | Validar na API ambiente existente, campos/tipos, temperatura -10 a 60 °C e umidade 0 a 100%; descrição 10–500 caracteres após trim; foto obrigatória, tamanho limitado, tipo permitido e estrutura de JPEG conferida (decodificação completa dos pixels planejada). Limite de bytes também na ingestão MQTT | Cliente: `app/domain/ocorrencia.ts:9`, `ocorrencia.tsx:55`, `ocorrencia.test.ts:5`. Servidor: descrição 10–500 em `OcorrenciasController.cs:20`; foto vazia ou acima de 5 MB recusada com 400 `arquivo` em `:21`; teto da requisição de 6 MB em `:15`. Faixa da leitura em `ValidadorLeitura` (`Contrato.cs:106`). Testes: `ApiTestes.RecusasELeituraAceita` (`fora-da-faixa`), `VetoresTestes.LimitesDaJanelaEDaFaixa`, `ApiTestes.CadastroRecusaEntradaInvalida`, `ApiTestes.UploadRecusaOQueNaoEJpegENomeComDoisPontos`. Tipo: `Content-Type` e três primeiros bytes `FF D8 FF` (`OcorrenciasController.cs:58-64`); estrutura: `SemApp1` (`:69`) devolve vazio, e a ação responde 400 `nao-jpeg` (`:29`), se faltar quadro SOF com altura e largura maiores que zero ou varredura SOS. Teste: `ApiTestes.UploadRecusaJpegSemQuadroOuSemVarredura`. MQTT: mensagem acima de 1024 bytes (`AssinanteMqtt.cs:12`) é descartada antes de desserializar (`:60`), com log `acima-do-limite`. Teste: `ApiTestes.MqttDescartaMensagemAcimaDoLimite`. **Parcial:** os pixels não são decodificados, então um arquivo com cabeçalhos JPEG válidos e dados de imagem corrompidos passa; `POST /leituras` só tem o limite padrão do servidor web. Planejado: decodificar a imagem no servidor | E07–E08 |
| C5 | `/demo/semear` só grava em Demo; em qualquer outro ambiente responde 404 sem tocar no banco. Demonstrar que a mesma rota funciona em Demo para evitar falso positivo de rota inexistente | A rota existe em todos os ambientes; a primeira linha da ação, `DemoController.cs:20`, confere o ambiente e, fora de Demo, registra `demo semear recusado: fora-do-ambiente` e retorna 404. Esse log separa a recusa de uma rota inexistente. No ambiente Demo o mesmo caminho grava, se houver leitura ao vivo. Testes: `ApiTestes.DemoSemearForaDoAmbienteDemoResponde404` e `ApiTestes.DemoSemearExigeLeituraAoVivoEAncoraNela` | E09, `evidencias/demo-semear-404.txt` |
| C6 | Segredos fora dos arquivos versionados e dos logs; chave distinta por dispositivo, revogável; configuração protegida no IIS e ACL mínima. Valores de teste declarados como tais e revogados após a avaliação | Os segredos `segredo-de-teste-sala-01` a `-04` em `SalaDb.cs:65-68` são **valores de teste revogáveis**, públicos como vetor do contrato, nunca de produção (seção 4.1). Revogação por `POST /dispositivos/{id}/revogar` (`DispositivosController.cs:13`). `esp32/secrets.h` ignorado em `.gitignore:19`. `ChaveCoordenacao` não está no appsettings; o filtro lê a configuração em `ExigeChaveCoordenacaoAttribute.cs:16`, e o roteiro do IIS grava a chave só no pool (`infra/04-iis.ps1:72-86`). Teste do segredo alheio: `ApiTestes.SegredoDeOutraSalaNaoAssina`. Busca no histórico do git executada em 05/11/2026: só os vetores de teste e o `chave-teste` da suíte (`evidencias/osint.txt`, item 8) | E10, `evidencias/git-check-ignore.txt`, `evidencias/osint.txt` |
| C7 | GPO: RDP apenas grupo autorizado do AD, política de senha/bloqueio e firewall restrito à administração; auditoria de logon e da pasta de fotos (política + SACL), NTFS só permite escrita ao pool e administradores; logs protegidos | Roteiro pronto: política de senha (mínimo 12, histórico 24) e bloqueio após 5 tentativas por 15 min em `infra/06-gpo.ps1:11`; auditoria de logon e de sistema de arquivos em `:27`; SACL da pasta de fotos em `:30-33`; ACL NTFS só para SYSTEM, Administradores e pool em `infra/04-iis.ps1:40-46`. A aplicar e comprovar no laboratório Windows Server; R9 segue crítico até a evidência E11 | E11 |
| C8 | Reduzir superfície de rede: HTTPS para usuários, SQL somente interno, RDP somente rede administrativa, MQTT de saída sem abrir listener no servidor. LED local sem internet e aviso de leitura desatualizada; não fabricar dados atuais | Cota das rotas HTTP de escrita: `Program.cs:12` (30 por minuto por IP, 429 em `:14`); teste `ApiTestes.EscritaAcimaDeTrintaPorMinutoResponde429`. A ingestão MQTT não passa por essa cota. Roteiro de rede: firewall HTTP 80 só para a sub-rede local, perfis Domínio e Privado (`infra/04-iis.ps1:49`); binding HTTPS 443 com certificado do laboratório e regra de firewall `TermometroHTTPS` na porta 443, também só para a sub-rede local (`infra/README.md`, seção "HTTPS no laboratório"); porta 443 informada à API pelo pool (`infra/04-iis.ps1:88`) e redirecionamento HTTPS só em Production (`Program.cs:62`). HTTP 80 continua aberto para a inicialização do IIS e só redireciona; HSTS e `nosniff` ficam em C11. Aplicação no Windows Server, nmap, listener MQTT e LED: a executar no laboratório | E12 e E13; a cota está na suíte |
| C9 | Nenhum dado de aluno: foto de sala/equipamento vazio, texto sem nomes, painel sem imagens e revisão antes de publicar. Minimizar metadados, excluir cópias locais de teste ao encerrar e revisar retenção antes de uso real | O upload regrava o JPEG sem APP1 em `OcorrenciasController.cs:28` (`SemApp1` em `:69`). Teste: `ApiTestes.UploadRemoveSegmentoApp1` (o arquivo devolvido conserva `JFIF` e não contém `GPS-TESTE` nem o marcador APP1). O app pede `exif: false` em `ocorrencia.tsx:37` e orienta em `:85` e `:92`. Isso não impede um rosto na imagem: a revisão humana continua obrigatória | E14 |
| C10 | Autenticar e autorizar ações, revogação e cadastro de sala na API; menor privilégio nas contas/pool/SQL. Foto e ocorrência do professor não usam a chave da coordenação | Filtro `ExigeChaveCoordenacaoAttribute.cs:20`: cabeçalho `X-Chave-Coordenacao`, comparação em tempo constante, 401 `chave-invalida` se faltar, se divergir ou se a configuração estiver vazia. Aplicado em `AmbientesController.cs:17`, `AcoesController.cs:30` e `DispositivosController.cs:15`. Teste: `ApiTestes.CoordenacaoSemChaveNaoGravaELeituraContinuaComHmac` (401 sem gravar; `POST /leituras` e `POST /ocorrencias` seguem sem a chave). Identificação no aparelho (`app/services/professor.ts:19`) não é essa chave. **Lacuna declarada (R12):** `GET /ocorrencias/{id}/foto` (`OcorrenciasController.cs:42`) exige só o GUID; o tratamento previsto é exigir a chave ou um token de curta duração. Contas, pool e SQL: a comprovar no laboratório | E15 |
| C11 | HSTS e cabeçalhos de segurança: `UseHsts` em Production e `X-Content-Type-Options: nosniff` em todas as respostas | Pendente. `Program.cs` tem só `UseHttpsRedirection` (`:62`). O binding 443 e o firewall 443 já estão no roteiro de infraestrutura (C8); falta o código dos cabeçalhos. Ao implementar, cobrir com teste e reavaliar R14 | Teste automatizado e E12 |
| C12 | MQTT com TLS (porta 8883) e autenticação no broker; o ESP32 confere o certificado do broker | Pendente; obrigatório antes de qualquer piloto com decisão real. No laboratório vale a exceção da seção 7 | Configuração do broker e captura sem texto em claro |

### 6.1 Classificação dos controles

Tipo: **P** preventivo, **D** detectivo, **C** corretivo. Controles do Anexo A da ABNT NBR ISO/IEC 27001:2022. Estado: **implementado** (no código e com teste), **parcial** (parte no código e com teste, parte planejada), **roteiro pronto** (script existe, falta aplicar no laboratório) ou **pendente**.

| ID | Tipo | ISO/IEC 27001:2022, Anexo A | Custo e viabilidade | Responsável funcional | Estado |
|---|---|---|---|---|---|
| C1 | P, D | 8.24 Uso de criptografia; 8.5 Autenticação segura; 8.15 Registros (logs) | Baixo; HMAC já no código, sem dependência paga; a trava de concorrência é uma coluna e um teste | Backend | Parcial: HMAC, vínculo, revogação, janela e reenvio sequencial implementados e testados; atualização atômica planejada |
| C2 | P | 8.28 Codificação segura | Nenhum; padrão do EF Core | Backend | Implementado |
| C3 | P | 8.28 Codificação segura; 8.3 Restrição de acesso à informação | Baixo; já no código | Backend e infraestrutura | Implementado |
| C4 | P | 8.26 Requisitos de segurança da aplicação; 8.28 Codificação segura | Baixo; validação e limite MQTT já no código; decodificação sem dependência paga | Backend e app | Parcial: descrição, tamanho da foto, tipo, estrutura JPEG, faixa e limite de 1024 bytes no MQTT implementados e testados; decodificação dos pixels planejada |
| C5 | P | 8.31 Separação dos ambientes de desenvolvimento, teste e produção; 8.33 Informações de teste | Nenhum | Backend | Implementado |
| C6 | P, C | 5.17 Informações de autenticação; 8.4 Acesso ao código-fonte; 8.33 Informações de teste | Baixo; revogação já existe na API | Responsável pelo repositório e coordenação | Implementado; revogação dos valores de teste após 19/11/2026 |
| C7 | P, D | 5.15 Controle de acesso; 8.2 Direitos de acesso privilegiado; 8.5 Autenticação segura; 8.15 Registros (logs); 8.16 Atividades de monitoramento | Baixo; recursos nativos do Windows Server, sem licença extra | Infraestrutura | Roteiro pronto |
| C8 | P | 8.20 Segurança de redes; 8.22 Segregação de redes; 8.6 Gestão de capacidade | Baixo; certificado autoassinado no laboratório, certificado público gratuito para uso real | Infraestrutura e backend | Cota HTTP implementada; rede, binding e firewall 443 com roteiro pronto; HSTS e `nosniff` em C11 |
| C9 | P, C | 5.34 Privacidade e proteção de dados pessoais; 8.10 Exclusão de informações; 8.11 Mascaramento de dados | Baixo; remoção de EXIF já no código, revisão humana custa tempo da equipe | App, backend e coordenação | Implementado (técnico); revisão humana a cada entrega |
| C10 | P | 5.15 Controle de acesso; 8.5 Autenticação segura; 8.3 Restrição de acesso à informação | Baixo para a chave; médio para estender à foto (consumidores do app e do desktop mudam) | Backend | Implementado, com a lacuna da foto declarada |
| C11 | P | 8.9 Gestão de configuração; 8.26 Requisitos de segurança da aplicação | Baixo; duas linhas no `Program.cs` e um teste | Backend e infraestrutura | Pendente |
| C12 | P | 8.24 Uso de criptografia; 8.21 Segurança dos serviços de rede | Médio; broker gerenciado ou próprio com TLS e certificado gravado no ESP32 | Dispositivo e backend | Pendente |

Não há obrigação de acrescentar autenticação AD ao app nesta entrega. Há obrigação de declarar a limitação e restringir o laboratório enquanto a autorização da API não estiver comprovada. Qualquer mudança de rota ou DTO requer ajustar os comandos do roteiro, os consumidores e o contrato na integração.

## 7. Riscos residuais e plano de tratamento

| Risco residual | Tratamento (ISO/IEC 27005) e responsável funcional | Prazo/gatilho | Condição de encerramento |
|---|---|---|---|
| Chave de demonstração visível no Wokwi e no repositório (R7, R10) | Reter no laboratório; coordenação/backend: chave única por dispositivo, revogação após avaliação. Para instalação física, provisionar chave fora do fonte em NVS e proteger acesso físico | Revogar imediatamente após a avaliação de 19/11/2026 ou antes se houver abuso | Mensagem assinada com a chave antiga é recusada; nenhuma chave de produção usada no Wokwi |
| Broker público sem SLA, sem TLS e sem controle de publicação (R8, R13) | Reter no laboratório; dispositivo/backend: LED autônomo, aviso de desatualização e POST /leituras como alternativa de demonstração. Modificar antes de uso real com C12 | Antes de qualquer piloto com decisões reais | Broker autenticado com TLS; queda ensaiada sem dado antigo parecer atual |
| Sem HSTS e sem `nosniff` (R14) | Modificar com C11 no backend; infraestrutura aplica o binding e o firewall 443 do roteiro (C8) | Antes de expor a API fora da sub-rede do laboratório | Teste automatizado dos cabeçalhos e nmap mostrando 443 com certificado |
| Duplicata concorrente e volume MQTT sem limite (R2, R8) | Modificar com a parte planejada de C1 (atualização atômica); limitar volume por dispositivo no assinante ou usar broker próprio (C12); backend | Antes de qualquer piloto com decisões reais | Teste de duas gravações paralelas com uma só aceita; inundação ensaiada no broker próprio sem perda de leituras legítimas |
| Foto pode conter pessoa apesar do aviso (R6) | Modificar e evitar: revisar cada imagem e texto antes da entrega; remover ocorrência indevida e cópias, registrar incidente sem reproduzir dado. Não usar rosto real para testar | Antes de qualquer envio, print ou publicação | Conjunto entregue sem identificação de aluno e sem metadados desnecessários |
| Upload, RDP e foto sem chave (R5, R9, R12) | Backend/infraestrutura: a API já recusa travessia, não-JPEG e escrita da coordenação sem chave. Aplicar C7 no laboratório; proteger o GET da foto (C10). Não expor o serviço fora do laboratório antes disso | Antes de disponibilizar a API fora do laboratório | E11 comprovada; GET da foto sem chave responde 401 |
| Exclusão não remove credencial antiga (R10) | Responsável pelo repositório: revisar histórico sem divulgar valores; se houve exposição, revogar primeiro e tratar cópias/histórico | Já: o repositório é público. Busca do item 8 de `evidencias/osint.txt` executada em 05/11/2026, sem achado além dos vetores de teste e do `chave-teste` da suíte; repetir antes da entrega de 19/11/2026 e a cada suspeita | Chave exposta inutilizada; revisão documentada em `evidencias/osint.txt` |

A coordenação mantém a decisão humana; alertas apoiam o protocolo escolar e não garantem proteção à saúde. A revogação existe na API e exige a chave. A autonomia do LED e a GPO dependem da coleta no laboratório para sustentar qualquer redução de R9.

## 8. Evidências

O anexo [ANEXO-EVIDENCIAS.md](ANEXO-EVIDENCIAS.md) traz, para cada evidência E01–E15 e para os cenários S1 a S3, o controle, o comando, o resultado esperado, o resultado obtido e a data. Os comandos completos ficam no roteiro [EVIDENCIAS.md](EVIDENCIAS.md), que não entra no PDF. As saídas que não dependem do laboratório estão em [evidencias/](evidencias/), geradas em 05/11/2026 no Windows da equipe, com banco SQLite em memória dos testes:

| Arquivo | Controle | Resultado obtido |
|---|---|---|
| `dotnet-test-seguranca.txt` | C1–C5, C9, C10, cota | Suíte completa 40/40; 18 testes de segurança listados, 18 aprovados |
| `recusas-log.txt` | C1 | Cinco motivos de recusa no log da API |
| `consulta-parametrizada.txt` | C2 | Id com `OR 1=1` vira parâmetro `@__id_0` de 18 caracteres; GET responde 404 |
| `upload-travessia.txt` | C3 | `../foto.jpg` e `../x.jpg` recusados com `nome-invalido` |
| `demo-semear-404.txt` | C5 | `fora-do-ambiente` e 404 |
| `git-check-ignore.txt` | C6 | `esp32/secrets.h` coberto por `.gitignore:19` |
| `osint.txt` | C6, C9 | Exercício de OSINT da seção 4.1, com a busca no histórico do git |
| `cenarios-ataque.txt` | C1–C4, C6, C9, C10 | Cenários S1 a S3 da seção 4.2: 23 passos conferem (16 ataques barrados, 2 lacunas declaradas confirmadas, 5 de referência ou da coordenação) |

nmap, IIS, GPO e LED são coletados no laboratório Windows Server. Um 404 de rota que não existe não substitui o 404 do ambiente Demo, já coberto pelo teste.

O relatório e o anexo de evidências, com os prints do laboratório, formam um **único PDF**, com capa, matriz, controles, riscos residuais e evidências legíveis. O diagrama `fluxo-dados.svg` entra como imagem. Um envio por equipe na tarefa de Cybersecurity do Teams até **19/11/2026**, conforme o Enunciado GS 2026.2, p. 28.

## 9. Referências

1. ABNT. **ABNT NBR ISO/IEC 27001:2022** — Segurança da informação, segurança cibernética e proteção à privacidade — Sistemas de gestão da segurança da informação — Requisitos. Anexo A.
2. ABNT. **ABNT NBR ISO/IEC 27002:2022** — Segurança da informação, segurança cibernética e proteção à privacidade — Controles de segurança da informação.
3. ISO/IEC. **ISO/IEC 27005:2022** — Information security, cybersecurity and privacy protection — Guidance on managing information security risks.
4. NIST. **SP 800-30 Rev. 1** — Guide for Conducting Risk Assessments. Gaithersburg: NIST, 2012.
5. OWASP Foundation. **OWASP Top 10:2021**. Disponível em: https://owasp.org/Top10/.
6. OWASP Foundation. **OWASP API Security Top 10:2023**. Disponível em: https://owasp.org/API-Security/.
7. MITRE. **Common Weakness Enumeration (CWE)**. Disponível em: https://cwe.mitre.org/.
8. SHOSTACK, Adam. **Threat Modeling: Designing for Security**. Indianapolis: Wiley, 2014 (modelo STRIDE).
9. BRASIL. **Lei nº 13.709, de 14 de agosto de 2018** (Lei Geral de Proteção de Dados Pessoais), art. 14 (dados de crianças e adolescentes) e art. 46 (medidas de segurança).
10. FIAP. **Enunciado da Global Solution 2026.2**, Cybersecurity, p. 28: critérios de ativos e ameaças (2,5), vulnerabilidades e riscos (3,0), controles (2,5) e evidências técnicas (2,0).
11. Arquivos de implementação e linhas citados nas seções 4 a 6, conferidos em 05/11/2026.
