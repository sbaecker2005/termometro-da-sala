# Auditoria da entrega — 19/11/2026

Conferência de 08/11/2026 sobre os arquivos desta pasta. Prazo final: **19/11/2026, 23h55** (`ENTREGA.md:3`); preparação concluída até **18/11** é a meta interna (`docs/PLANO.md:204`). Não há aprovação da entrega completa: faltam identificação da equipe, vídeos, publicação do circuito, evidências da VM e arquivos finais de envio.

Atualização documental de 08/11: as incoerências locais abaixo foram reconciliadas em [RECONCILIACAO-08-11.md](RECONCILIACAO-08-11.md), com referências antes/depois. Este texto preserva o diagnóstico anterior às correções; resultados atuais informados pelo coordenador: API 85/0, desktop 18/0, app 20/0 e TypeScript sem erros. Ensaios e evidências externas continuam pendentes.

**Atualização de 17/11/2026:** o diagnóstico, as contagens e a tabela de pendências de 08/11 abaixo permanecem históricos, não uma lista de impedimentos atuais. A identificação da equipe já está preenchida. Com autorização de Samuel, passaram **101/101 testes da API, 19/19 do desktop, 6/6 verificações do autoteste e 23/23 testes Mobile**, além de TypeScript sem erros. Evidências em [resultados-testes-2026-11-17](resultados-testes-2026-11-17/); desktop e Mobile já foram verificados após o novo acabamento visual.

A [validação do desktop no Windows](../desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md) registra painel maximizado e em 958 × 707 com rolagem, cadastro, aviso longo de modo local, mapas Agora/+1 h/frio, vinte cadeiras C01–C20, quatro sensores S1–S4 e dois PNGs completos do novo visual. Às 02h58, cadastro e ação locais persistiram após fechar e reabrir com a API desligada; a fixture foi retirada e o retorno à API confirmado. O **ZIP desktop final atualizado** foi extraído em D: e passou **19 testes mais 6 verificações do autoteste**, saída 0, com [TRX atual](resultados-testes-2026-11-17/desktop-pacote-fechamento.trx) e [registro](../output/fechamento-samuel/CONFERENCIA-PACOTES.txt). São 47 entradas e 496.805 bytes; SHA-256 `ee0b62dc23c40aa58cc1e10ed2c3ce834c73493e2b198b2f47b65d41c4d3f6dd`. A configuração incluída contém apenas a URL pública local e foi gerada sem ler configs existentes. O pacote de fontes também publicou o desktop e compilou API/Ensaio isoladamente; nenhuma API ou banco foi iniciado nessa conferência. ZIPs, hashes e PDFs de revisão estão relacionados no [checklist atualizado](../ENTREGA.md).

Também foram repetidas em 17/11 as conferências Python de arquitetura e circuito: **111 elementos, 190 relações, seis visões**, equivalência dos arquivos e regras do Archi instalado aprovadas; **12 autotestes** e **110 verificações estáticas do circuito**, todos com saída 0. ML/pytest passou com **23 testes e 9 subtestes**, saída 0, em Python 3.11.9 portátil isolado em D:, com os pins preservados. [Ambiente, comandos e resultados](resultados-testes-2026-11-17/ml-pytest.txt); sete arquivos do notebook/modelos/dados permaneceram idênticos por SHA-256. O bloqueio inicial do runtime 3.12 foi superado e preservado como histórico. Esses resultados não representam execução no Archi GUI, no Wokwi ou em campo.

No código, a API passou a validar a descrição da ação após aparar espaços e o ID da sala com dois dígitos ASCII; o app consulta novamente a permissão ao tentar retomar a câmera e recusa propriedades herdadas como níveis térmicos. A ingestão compartilhada por HTTP/MQTT recebeu atualização condicional de `UltimoMedidoEm` com `ExecuteUpdateAsync`, na mesma transação da leitura. Em 17/11, o [ensaio real HTTP/MQTT/SQL Express](resultados-testes-2026-11-17/TermometroGS_MQTT_20261117_091654/resultado.txt) passou 21 verificações, incluindo dez corridas, revogação e rollback com retry; a [coleta SQL](../api/evidencias/TermometroGS_SOA_20261117_Validado/conferencia.tsv) passou 98 verificações sem divergências. Isso não comprova IIS/VM, Wokwi, broker público ou execução em campo, nem reduz automaticamente a classificação de risco no relatório de Cybersecurity. A correção posterior do Demo, ancorando o histórico simulado na leitura ao vivo mais recente, tem regressão reproduzida antes e aprovada na [suíte de 101 testes](resultados-testes-2026-11-17/api-fechamento-101.trx).

O [anexo de Cybersecurity](../seguranca/ANEXO-EVIDENCIAS.md) foi reconciliado com a coleta local de 07/11: E15 registra foto sem chave em HTTP 401, em Development/Kestrel. Essa evidência não comprova IIS/Production. Vídeos, publicação do circuito, evidências da VM e envio final permanecem dependentes das respectivas verificações e entregas.

O **ZIP Mobile final** foi extraído em D:, instalado do lock original e aprovado com TypeScript sem erros e 23/23 testes; 40/40 arquivos conferidos por SHA-256 entre fonte, ZIP e extração. [Registro do pacote](resultados-testes-2026-11-17/mobile-zip.txt). Os pacotes separados de Gui e Lucas estão no [checklist atualizado](../ENTREGA.md), com instruções por responsável.

**Enunciado e entrevista resolvidos:** localizado o PDF oficial `GS CitizenTech - 3ES Ago- FIAP 2026 .pdf`, SHA-256 `70AB581A0F9C914DC3E6A528BA30B35283779030C2B791A29D4AD0DE5D2A7201`; páginas 27, 29 e 30 conferidas para SOA/C#/ML. Samuel confirmou o relato e a autorização da entrevista em 17/11. Magister e Salgueiro são candidatas a futuro piloto; não há parceria, autorização ou execução confirmada. O PR 1 está aberto, sem reviews/merge, com base `desktop/v0.1.0`; releases v0.1.0/v0.2.0 existem. A `main` consultada continua em `7a8e4e35eeb3827200c27efc085f5158898879eb`, sem os ajustes locais.

**Fechamento local:** [Insomnia real](../api/evidencias/insomnia-2026-11-17/README.md) concluiu 29 requisições e 29/29 asserções, zero falhas. O iniciador passou em banco SQL novo, retornou previsão numérica e abriu o desktop; [resposta](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/previsao-ml.txt). Tela maximizada, mapa C01–C20/S1–S4, frio azul e PNG exportado foram inspecionados sem cortes; [relatório](../desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md).

**Reexecução integral e proteção de abertura:** [19/19 células do notebook](resultados-testes-2026-11-17/ml-notebook-integral.txt) passaram em 31,95 s, saída 0, via Python 3.11.9/IPython.display/Agg em cópia isolada no D:. Modelos JSON estruturalmente idênticos e 12 originais com hashes preservados; sem kernel Jupyter/nbclient ou inspeção visual das figuras regeneradas. O teste de segunda abertura revelou que `Get-NetTCPConnection -ErrorAction SilentlyContinue` podia ocultar falha da consulta de porta. A substituição por reserva TCP exclusiva recusa a porta ocupada e preserva a API original, comprovada no host e sandbox: [proteção da porta](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/protecao-porta.txt). Processos adicionais da tentativa anterior foram encerrados, registros/banco preservados, apresentação original mantida. Ambos os problemas encontrados nesta etapa — âncora temporal do Demo e proteção do iniciador — estão corrigidos e verificados.

**Pendências atuais de Samuel:** gravar/publicar o vídeo ML de até dois minutos; publicar fontes/evidências no repositório e enviar as entregas. Nas outras disciplinas continuam os ensaios Mobile nativo, VM e Wokwi, vídeos, revisão de fontes de impacto/pitch e PDFs finais. DPI ampliado não foi inspecionado e é limite de cobertura; não consta como exigência da página 29, assim como proteção de branch, aprovação/merge do PR, vídeo C# ou piloto físico. Os PDFs existentes são versões de revisão. Não há promessa de nota ou aprovação integral; itens históricos já resolvidos não devem ser reabertos apenas por aparecerem nas seções de 08/11 abaixo.

## Escopo e qualidade da prova

Única escrita desta auditoria: este documento. Foram lidos plano, entrega, contrato, roteiros, código compartilhado, clientes, testes, exemplos, migrações, modelos e evidências locais. Nenhum código, chamador, teste, fixture, configuração ou exportação foi alterado. Não houve acesso à rede, instalação, comandos de versionamento nem leitura de arquivos de ambiente ou credenciais privadas. Nenhum banco foi aberto ou modificado.

O ponto anterior indicado é a auditoria do registro `f3b508f`, no ramo `gs/alerta-frio`. Ela não existe como arquivo nesta pasta. Como a consulta ao histórico foi vedada, **não foi possível ler seu texto nem comparar conclusões literalmente**. Os itens “O que mudou” abaixo registram o estado atual e as correções anteriores comprovadas por `docs/ROTEIROS-CONFERENCIA.md:7`, sem atribuir à auditoria antiga frases que não foram lidas. A identificação desta pasta como main em `fac6d46` vem do contexto recebido; não foi verificada por comando.

Distinções usadas:

- **Executado aqui:** checagem local, sem aplicação, banco ou dispositivo real.
- **Registro existente:** saída ou resumo já presente; não é nova execução nem prova de IIS/Wokwi.
- **Só texto:** descrição, checklist ou roteiro sem resultado operacional correspondente.
- **Contagem estática:** casos declarados no código; não significa que passaram.

Verificações executadas aqui:

- `node --experimental-strip-types --test --test-isolation=none app/domain/*.test.ts`: **20 aprovados**, zero falhas. A execução padrão falhou antes de executar os casos por `spawn EPERM`; a alternativa no mesmo processo passou. Houve aviso de formato de módulo, sem falha dos testes.
- `python -B arquitetura/conferir_modelo.py`: **111 elementos, 190 relações, seis visões**, arquivos equivalentes e relações verificadas pelas regras do Archi instalado. `--autoteste`: **12 casos aprovados**.
- `python -B esp32/conferir_circuito.py`: **110 verificações aprovadas**. Isso confere o circuito descrito; não compila nem simula o firmware.
- `python -B ferramentas/indice_calor.py` e `python -B ferramentas/vetor_hmac.py`: asserts aprovados.
- Parser PowerShell: **dez scripts de infraestrutura sem erros de sintaxe**. Não executados.
- Inspeção JSON: notebook com **19 células de código executadas, 49 saídas e nenhum output de erro**; cópias de `modelo.json` idênticas byte a byte. As saídas não foram recalculadas.
- Contagem dos atributos de teste: API **50 Fact + 35 InlineData = 85 casos**; desktop **11 Fact + 7 InlineData = 18 casos**. Não foram executados nesta auditoria. O registro existente informa 81 e 9, respectivamente (`api/evidencias/dotnet-test.txt:5`, `api/evidencias/dotnet-test.txt:17`).

Opção menor para o coordenador: reconciliar documentos e completar provas e pacotes já previstos; nenhuma funcionalidade adicional é necessária para essa etapa.

## 1. SOA e Web Services

### O que pede

`docs/PLANO.md:191` exige domínio, API REST, SQL Server com EF Core e migrações, integração HTTP/MQTT, vetores HMAC/relógio e coleção Insomnia com prints. `ENTREGA.md:34` pede envio do link do repositório. Pesos em `ENTREGA.md:38`: domínio **3,0**, banco **3,0**, migrações **2,5** e testes manuais **1,5**. README e identificação: `ENTREGA.md:43`.

### O que existe e funciona

- API organizada em domínio, contratos, dados, serviços e controladores (`api/README.md:119`). HTTP chama o serviço comum em `api/TermometroDaSala.Api/Controllers/LeiturasController.cs:41`; a validação/gravação está em `api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs:26`. Provas existentes: `RecusasELeituraAceita` (`api/TermometroDaSala.Api.Tests/ApiTestes.cs:31`), `HmacDoContrato` e `RelogioInmet1700UtcE14hEmSaoPaulo` (`api/TermometroDaSala.Api.Tests/VetoresTestes.cs:14`, `api/TermometroDaSala.Api.Tests/VetoresTestes.cs:32`). Execução registrada, não repetida: `api/evidencias/dotnet-test.txt:5`.
- CRUD com proteção do histórico e ocorrências/fotos com autorização: testes `CrudDeAmbientesComLocationE409QuandoHaLeituras` e `OcorrenciaCriadaVoltaNaListaDaSalaEFotoSoComChave` (`api/TermometroDaSala.Api.Tests/ApiTestes.cs:518`, `api/TermometroDaSala.Api.Tests/ApiTestes.cs:493`).
- Cinco arquivos de migração, enumerados em `docs/ROTEIROS-CONFERENCIA.md:51`. A saída SQL local lista os cinco IDs (`api/evidencias/saidas/B2.txt:7`); B1 registra banco atualizado (`api/evidencias/saidas/B1.txt:6`). São evidências históricas de SQL Server local, não da VM. O teste de esquema está em `api/TermometroDaSala.Api.Tests/EsquemaTestes.cs:37`.
- Coleção `api/insomnia.json`, respostas HTTP e resumo em `api/evidencias/saidas/conferencia.tsv:1`: **97 linhas OK**, zero DIVERGE na contagem do arquivo. São verificações do coletor, não 97 testes unitários nem prova de execução manual no Insomnia. `/niveis` registra cinco faixas em `api/evidencias/saidas/24-resposta.txt:14`. Há imagens T1/T2 em `api/evidencias/saidas/`; sua existência não valida legibilidade ou conteúdo.

### O que falta

- **Só a equipe:** completar identificação (`api/README.md:23`), conferir prints reais e enviar o link (`ENTREGA.md:45`).
- **Execução local sem rede nem instalação:** reconciliar o índice das evidências com as saídas; recontar/reexecutar testes quando houver ambiente preparado sem gerar uma alegação de aprovação baseada só na contagem; conferir a coleção e seus prints. Repetir a coleta só com banco apropriado: o coletor preserva escritas e fotos (`api/evidencias/README.md:16`). A correção da concorrência é trabalho local possível, mas permanece apenas apontada nesta auditoria.
- **Depende de fora da máquina:** acesso à plataforma de envio e ao repositório remoto para validar a entrega publicada. Nova coleta SQL depende de servidor/autenticação disponíveis; os registros existentes não provam essa disponibilidade agora.

### Incoerências

- O índice afirma falha de B1, ausência de B2 e “0 OK, 1 DIVERGE” (`api/evidencias/README.md:19`, `api/evidencias/README.md:23`), mas as saídas vinculadas registram sucesso e cinco migrações (`api/evidencias/saidas/B1.txt:6`, `api/evidencias/saidas/B2.txt:7`). Não descartar as provas nem anunciar recaptura obrigatória sem reconciliar datas e versão.
- A entrega enumera três migrações (`ENTREGA.md:40`), o README enumera quatro (`api/README.md:248`) e o banco registrado tem cinco (`api/evidencias/saidas/B2.txt:7`).
- O contrato descreve quatro faixas em `/niveis` (`CONTRATO.md:214`), embora sua própria tabela já tenha Frio (`CONTRATO.md:114`) e a resposta tenha cinco (`api/evidencias/saidas/24-resposta.txt:14`). O plano ainda chama todo índice abaixo de 27 de Normal (`docs/PLANO.md:134`), contra o mínimo de 18 do contrato (`CONTRATO.md:115`).
- O comando Demo do README usa `--launch-profile http` (`api/README.md:42`), que fixa Development (`api/TermometroDaSala.Api/Properties/launchSettings.json:19`). O roteiro atual evita isso (`esp32/ROTEIRO-VIDEO.md:15`).
- O resumo registra 81 testes (`api/evidencias/dotnet-test.txt:28`), mas há quatro Fact adicionais na contagem atual. Exemplos ausentes da contagem de cinco Fact do mapa: `PrecaucaoDoPontoPreservaOrigemMesmoComRotulosIguais`, `PrevisaoCrescenteMudaTemperaturaNivelEGradeSemInventarSensorAusente` e `GradeCobreBordasFracionariasSemLacunas` (`api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`, `api/TermometroDaSala.Api.Tests/MapaTestes.cs:114`, `api/TermometroDaSala.Api.Tests/MapaTestes.cs:138`). O total atual esperado é 85, sem comprovação de aprovação nesta rodada.
- O contrato ancora a curva no ponto zero (`CONTRATO.md:184`), mas o código remove esse ponto antes de escolher a âncora, usando o último ponto positivo (`api/TermometroDaSala.Api/Controllers/DemoController.cs:74`, `api/TermometroDaSala.Api/Controllers/DemoController.cs:79`). O teste existente de semeadura (`api/TermometroDaSala.Api.Tests/ApiTestes.cs:111`) não deve ser apresentado como prova de toda essa equivalência documental.

### O que mudou em relação ao ponto anterior

Não procede tratar o roteiro atual como incapaz de iniciar em Demo: ele usa ambiente explícito e nenhum perfil. Há registros locais de cinco migrações e de frio/mapa, apesar do índice contraditório. A reprodutibilidade do pitch tem preparação específica agora; sua execução continua pendente (`docs/ROTEIROS-CONFERENCIA.md:78`). Comparação literal com a auditoria anterior indisponível, conforme escopo.

## 2. Cybersecurity

### O que pede

Ativos, ameaças concretas, vulnerabilidades, riscos, controles e provas de HMAC/replay/revogação, SQLi, LFI, upload, ausência de segredos privados e portas mínimas (`docs/PLANO.md:192`). PDF único com relatório e anexo (`ENTREGA.md:51`). Pesos: **2,5** ativos/ameaças, **3,0** análise de riscos, **2,5** controles e **2,0** evidências (`ENTREGA.md:55`).

### O que existe e funciona

- Relatório com 14 riscos, método, matriz e controles, incluindo parciais declarados (`seguranca/RELATORIO-RISCOS.md:15`, `seguranca/RELATORIO-RISCOS.md:239`). A classificação residual é avaliação textual da equipe, não medição de incidentes.
- Testes de consulta parametrizada, EXIF, upload, autorização, cabeçalhos e cota HTTP (`api/TermometroDaSala.Api.Tests/ApiTestes.cs:317`, `api/TermometroDaSala.Api.Tests/ApiTestes.cs:292`, `api/TermometroDaSala.Api.Tests/ApiTestes.cs:493`). Registro existente: 20 testes selecionados aprovados (`seguranca/evidencias/dotnet-test-seguranca.txt:33`).
- Logs/prints locais estão presentes. A duplicação concorrente é concretamente registrada: a mesma mensagem recebeu dois 201 com IDs diferentes (`api/evidencias/saidas/E03-concorrencia.txt:5`, `api/evidencias/saidas/E03-concorrencia.txt:8`). O serviço valida antes de gravar, sem operação condicional (`api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs:28`, `api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs:68`).
- Relatório distingue os 23 passos históricos de ataque da correção posterior da autorização da foto (`seguranca/RELATORIO-RISCOS.md:155`). A saída histórica não prova a nova execução do script.

### O que falta

- **Só a equipe:** identificação, revisão humana de imagens e texto, coleta de telas do app/Wokwi e consolidação/envio do PDF (`ENTREGA.md:60`, `seguranca/ANEXO-EVIDENCIAS.md:41`). Revogar todos os dispositivos de demonstração usados após a avaliação, inclusive os quatro pontos e o projeto separado do pitch.
- **Execução local sem rede nem instalação:** atualizar anexo e referências com as provas atuais; conferir se prints/saídas se correspondem; montar o PDF com editor já disponível e conferir paginação. Concorrência e validação completa dos pixels são lacunas de código explicitamente abertas (`seguranca/RELATORIO-RISCOS.md:239`, `seguranca/RELATORIO-RISCOS.md:242`), sem correção nesta tarefa. Uma nova busca de histórico não foi executada por restrição desta auditoria.
- **Depende de fora da máquina:** IIS/HTTPS, GPO, NTFS, auditoria de logon, nmap de duas origens, circuito e falha de conectividade; repetir S1–S3 no laboratório (`seguranca/ANEXO-EVIDENCIAS.md:38`, `seguranca/ANEXO-EVIDENCIAS.md:46`). Validar exposição do projeto após publicação. TLS/autenticação no broker são exigidos antes de piloto real, mas a exceção didática está declarada (`seguranca/RELATORIO-RISCOS.md:250`).

### Incoerências

- O anexo ainda usa 55 testes como suíte completa (`seguranca/ANEXO-EVIDENCIAS.md:28`); o relatório usa 81 (`seguranca/RELATORIO-RISCOS.md:15`); a contagem estática atual é 85, conforme fontes da seção SOA. Datas diferentes justificam registros históricos, mas não uma alegação uniforme de validação atual.
- O anexo diz não haver saída local do tamanho de upload (`seguranca/ANEXO-EVIDENCIAS.md:34`), enquanto `api/evidencias/saidas/E07-tamanho-resposta.txt` contém a coleta posterior. Analogamente, o roteiro de segurança já liga prints locais (`seguranca/EVIDENCIAS.md:105`), mas o anexo não incorpora toda a rodada posterior. Isso não preenche a coluna de laboratório automaticamente.
- A entrega ainda pede busca histórica como ausente (`ENTREGA.md:264`), embora o relatório documente a busca de 05/11 (`seguranca/RELATORIO-RISCOS.md:151`). Uma nova busca é revalidação, não ausência de qualquer prova anterior.
- “Segredo por dispositivo” isolado é a regra textual (`CONTRATO.md:73`), mas os quatro sensores da Sala 7 compartilham o valor de demonstração (`api/TermometroDaSala.Api/Controllers/DemoController.cs:33`) e o pitch replica o valor da porta nos novos IDs (`arquitetura/PITCH.md:54`). Revogação é individual; isolamento entre segredos desses quatro pontos não está demonstrado.

### O que mudou em relação ao ponto anterior

Roteiros atuais separam mural e escrita pelo desktop e não dependem do perfil HTTP para Demo (`docs/ROTEIROS-CONFERENCIA.md:41`). O risco concorrente continua aberto e tem evidência local concreta; a existência de prints locais não fecha a VM. Não foi possível confrontar o texto anterior.

## 3. C# Windows Forms

### O que pede

Terminal integrado à API com cadastro validado, nível/previsão, ações, revogação e domínio orientado a objetos (`docs/PLANO.md:193`). ZIP, txt da equipe, README, versionamento semântico/PR/changelog (`ENTREGA.md:66`, `docs/PLANO.md:211`). **Sem divisão de pontos informada** (`ENTREGA.md:68`).

### O que existe e funciona

- Classes separadas em `Dominio/`, `Servicos/` e `Telas/`, conforme `desktop/README.md:127`. Registro de ação está no desktop (`desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:147`). Modo local avisa que revogar não tem efeito remoto (`desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:86`).
- Testes de validação, persistência sem sobrescrever arquivo inválido, CSV, contrato HTTP, demonstração e Designer (`desktop/TermometroDaSala.Desktop.Tests/DesktopTests.cs:22`). Mapa tem testes próprios de coordenadas, apresentação, contrato HTTP e modo local (`desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:44`, `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:81`). São código e registro histórico de nove aprovados (`api/evidencias/dotnet-test.txt:17`), sem nova execução.
- Relato de entrevista já preenchido, com data e autorização declaradas (`desktop/README.md:29`); há imagem demonstrativa. PR e tags têm links no README (`desktop/README.md:257`), sem confirmação remota nesta auditoria.

### O que falta

- **Só a equipe:** completar identificação no README/txt; validar fidelidade do relato sem inventar piloto; demonstrar telas com API ligada, incluindo edição/exclusão, revogação e modo local (`desktop/README.md:88`).
- **Execução local sem rede nem instalação:** reconciliar caminhos da entrega, conferir versões declaradas, montar ZIP preservando pastas e testar extração/build/autoteste em ambiente com dependências já presentes (`ENTREGA.md:86`). Recontagem atual: 18 casos; o autoteste de seis grupos é outra medida, não conflito automático com xUnit.
- **Depende de fora da máquina:** confirmar PR revisado, proteção do ramo e tags reais (`docs/PLANO.md:211`), além do envio na plataforma. Ausência de consulta remota não prova ausência dessas configurações.

### Incoerências

- A entrega aponta `Form1.cs`, `Servicos.cs` e `Dominio.cs` (`ENTREGA.md:72`, `ENTREGA.md:73`), mas o changelog registra separação e renomeação para `TelaCoordenacao` e classes próprias (`desktop/CHANGELOG.md:20`, `desktop/CHANGELOG.md:23`). Os caminhos antigos não existem nesta pasta.
- Checklist manda fazer entrevista e abrir PR (`ENTREGA.md:80`, `ENTREGA.md:81`), embora relato e links já estejam preenchidos (`desktop/README.md:29`, `desktop/README.md:257`). Conferir os resultados reais; não repetir a pendência como ausência absoluta.
- Nove testes citados na entrega (`ENTREGA.md:82`) e no pitch (`arquitetura/PITCH.md:108`) correspondem ao resumo antigo; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` acrescenta nove casos aos nove de `desktop/TermometroDaSala.Desktop.Tests/DesktopTests.cs:8`. São 18 declarados atualmente, ainda sem execução nesta auditoria.
- README do desktop descreve deslocamento no mapa futuro (`desktop/README.md:7`), enquanto raiz e pitch descrevem persistência (`README.md:7`, `arquitetura/PITCH.md:34`). O código segue o deslocamento (`api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21`).

### O que mudou em relação ao ponto anterior

O pitch atual identifica o desktop como local de registrar a ação e manda Atualizar após as publicações (`arquitetura/PITCH.md:87`, `arquitetura/PITCH.md:88`). Esse ponto do roteiro está resolvido no texto; gravação e ensaio continuam pendentes. Não houve comparação literal com a auditoria anterior.

## 4. IA e Machine Learning

### O que pede

Dados reais com origem, divisão temporal, modelo exportado, baseline, MAE/F1 e integração na solução (`docs/PLANO.md:194`). Notebook, vídeo de até **2 min** e link do repositório (`ENTREGA.md:103`). Pesos: problema/dados **2,0**, preparação/análise **2,0**, modelo/treino **3,0**, avaliação **2,0**, integração **1,0** (`ENTREGA.md:116`).

### O que existe e funciona

- CSV com **34.786 registros horários**, notebook com saídas, grade de 410 configurações, modelo de árvore e métricas/baseline (`ml/README.md:30`, `ml/README.md:48`, `ml/README.md:63`). Inspeção local confirmou saídas sem erro e modelos idênticos; não repetiu treino nem cálculo das métricas.
- Comparação documentada: MAE **0,909 °C** contra **1,442 °C** da persistência; recall de cuidado **39,6%**, ainda com **29 falsos negativos** e **17 falsos positivos** em 2025 (`ml/README.md:63`, `ml/README.md:76`, `ml/README.md:78`). É resultado armazenado e documentado, não garantia de segurança.
- Paridade de 200 vetores e aplicação do modelo no Demo têm testes (`api/TermometroDaSala.Api.Tests/PrevisorModeloTestes.cs:24`, `api/TermometroDaSala.Api.Tests/PrevisorModeloTestes.cs:82`). Roteiro de 1min50s mostra GET real e teste com vetor fixo 27,16 (`ml/ROTEIRO-VIDEO.md:15`); não depende da previsão fixa do app simulado.
- Estudo por ponto simulado, separado do modelo servido, com artefatos e testes (`ml/README.md:101`, `ml/test_mapa.py:155`). A declaração de 22 testes aprovados está em `docs/MAPA-TERMICO-ML.md:119`; não executada aqui.

### O que falta

- **Só a equipe:** identificação no README, gerador e notebook; gravar vídeo com API real e publicar link (`ENTREGA.md:122`). Confirmar tamanho da equipe antes de considerar opcional a avaliação completa da sala.
- **Execução local sem rede nem instalação:** corrigir contagens documentais, manter notebook/gerador sincronizados depois dos dados de identificação e conferir saídas; conferir reprodução dos testes existentes num ambiente já preparado. Se regenerar, executar novamente: o gerador por si só não preserva provas de execução (`ml/README.md:139`). Para equipe de 4–5, falta demonstrar a avaliação **modelo + delta** contra sala +2 h e baseline, exigida condicionalmente por `docs/PLANO.md:194`; o exemplo com delta fixo (`ml/gerar_notebook.py:340`) e o estudo com modelo de pontos separado (`ml/README.md:106`) não comprovam essa avaliação.
- **Depende de fora da máquina:** gravação/publicação do vídeo, demonstração Wokwi/API e acesso ao repositório na plataforma. Validação real do delta, calibração e dados novos independentes dependem de campo, fora da entrega simulada mínima (`ml/README.md:97`).

### Incoerências

- README diz **15 células de código/37 saídas** (`ml/README.md:5`, `ml/README.md:144`); o arquivo `ml/termometro_da_sala.ipynb` contém **19/49**, incluindo seção 8. Verificação por JSON nesta auditoria; não inferir quebra do notebook por esse texto antigo.
- O plano pede F1 macro e MAE melhores que baseline (`docs/PLANO.md:194`); o README distingue F1 dos níveis presentes do relatório com Perigo de suporte zero (`ml/README.md:78`). Escolher a tabela certa ao avaliar o requisito, sem comparar macros com conjuntos de classes diferentes.
- A entrega coloca previsão no app/desktop no pitch (`ENTREGA.md:120`), enquanto a preparação do vídeo de ML exige Wokwi na Sala 3 (`ml/ROTEIRO-VIDEO.md:5`). O exemplo de circuito está na Sala 7 (`esp32/README.md:36`); a preparação específica da Sala 3 agora existe no pitch (`arquitetura/PITCH.md:49`). Não basta iniciar o exemplo padrão para gravar ML.

### O que mudou em relação ao ponto anterior

O roteiro atual limita o vídeo a 1min50s e comprova o modelo por GET e teste; não promete número fixo na hora da gravação (`ml/ROTEIRO-VIDEO.md:7`). A preparação atual do pitch resolve documentalmente os quatro IDs da Sala 3. Não foi possível verificar quais dessas conclusões já constavam da auditoria anterior.

## 5. Mobile e IoT

### O que pede

Quatro telas, identificação persistida, lista com carregando/erro/vazio, detalhe/previsão, ocorrência validada com câmera/permissão, TypeScript estrito, serviços e decisões (`docs/PLANO.md:195`). ZIP sem dependências e vídeo de até **2 min** (`ENTREGA.md:128`). Pesos: telas/fluxo **40%**, TypeScript **30%**, recurso nativo **20%**, organização/validação/documentação **10%**; `authors` completo é requisito de entrada (`ENTREGA.md:132`).

### O que existe e funciona

- Telas em `app/app/`; identificação em `app/app/index.tsx:38`; câmera/permissão em `app/app/salas/[id]/ocorrencia.tsx:24`; envio e erros em `app/app/salas/[id]/ocorrencia.tsx:54`. Serviços e validação separados, conforme `app/DECISOES.md:17`.
- **20 testes executados e aprovados nesta auditoria**, cobrindo níveis, validação de identificação/ocorrência, contrato de previsão e mapa (`app/domain/nivel.test.ts`, `app/domain/professor.test.ts`, `app/domain/ocorrencia.test.ts`, `app/domain/previsao.test.ts`, `app/domain/mapa.test.ts`, `app/domain/api-mapa.test.ts`). Não exercitam câmera real, navegação visual nem persistência em aparelho.
- Roteiro de **1min50s**, com cortes de erro/vazio e permissão negada/aceita (`app/ROTEIRO-VIDEO.md:7`, `app/ROTEIRO-VIDEO.md:15`). O modo simulado é declarado (`app/README.md:48`); não prova integração real.

### O que falta

- **Só a equipe:** preencher `authors` e identificação do README, testar e gravar no celular, conferir permissão/câmera e reabertura lembrando identificação; gerar e enviar ZIP/vídeo (`ENTREGA.md:138`).
- **Execução local sem rede nem instalação:** revisão dos contratos/documentação, conferência dos testes e ZIP com estrutura preservada. `tsc --noEmit` fica pendente: `app/node_modules` não existe nesta pasta; não instalar nem usar comando que baixe compilador. Testes Node aprovados não substituem os **30%** de TypeScript.
- **Depende de fora da máquina:** celular com Expo Go, câmera, conexão ao servidor da API para integração e publicação/envio do vídeo. Se as dependências não estiverem disponíveis em ambiente preparado, sua obtenção é dependência externa.

### Incoerências

- `app/README.md:7` descreve deslocamento no mapa futuro e `README.md:7` descreve persistência. Código segue o primeiro (`api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21`).
- `app/DECISOES.md:21` diz “cinco salas”, mas o mock contém seis (`app/services/api.ts:37`), cinco níveis mais a sala de leitura antiga (`app/services/api.ts:42`). O roteiro exige sala 3 simulada (`app/ROTEIRO-VIDEO.md:5`), enquanto Physical exige Sala 7 real na API (`esp32/ROTEIRO-VIDEO.md:3`). São ensaios distintos e isso é permitido; usar o simulado como prova de Wokwi seria incoerente.

### O que mudou em relação ao ponto anterior

O roteiro do app já foi conferido e permaneceu intacto: não prescreve semeadura nem perfil HTTP (`docs/ROTEIROS-CONFERENCIA.md:11`). Não herdar para ele o bloqueio de inicialização do antigo roteiro ESP32. Comparação literal anterior indisponível.

## 6. Operating Systems

### O que pede

VM com rede/AD/DNS, IIS/painel/API/SQL, GPO/NTFS/auditoria, alertas e recuperação automática separada de pool e W3SVC (`docs/PLANO.md:196`). Formulário com repositório, evidências e vídeo de até **7 min** (`ENTREGA.md:144`). Pesos: rede/identidade **2**, IIS **2**, alertas **2**, recuperação **1**, logs **1**, funcionamento completo **2** (`ENTREGA.md:148`).

### O que existe e funciona

- Dez scripts e roteiro com instalação/configuração/publicação/coleta (`infra/README.md:56`, `infra/README.md:321`). **Parser dos dez passou aqui**; prova operacional continua só texto. GPMC, PTR e DFS ainda têm passos manuais (`infra/README.md:231`, `infra/README.md:358`).
- Teste de falha documentado exige novo PID antes de consultas HTTP, duas URLs 200 e serviço isolado (`infra/TESTE-DE-FALHA.md:36`, `infra/TESTE-DE-FALHA.md:65`, `infra/TESTE-DE-FALHA.md:73`). Isso evita confundir restart manual ou requisição que inicia o pool com recuperação automática.
- Roteiro atual tem **6min30s**; cita cinco migrações e mural só de leitura, distinguindo app/desktop (`infra/ROTEIRO-VIDEO.md:9`, `infra/ROTEIRO-VIDEO.md:13`). Não há execução comprovada da VM: o arquivo declara que nenhuma evidência foi coletada (`infra/EVIDENCIAS.md:3`).

### O que falta

- **Só a equipe:** disponibilizar VM/cliente, preencher identificação e dados reais do formulário, ensaiar, gravar e enviar (`ENTREGA.md:155`). Confirmar com o responsável pela disciplina se RDP administrativo atende ao requisito de RDS (`infra/EVIDENCIAS.md:12`).
- **Execução local sem rede nem instalação:** reconciliar listas de migrações e referência de linha do formulário, preparar checklist dos passos manuais e conferir sintaxe. Scripts prontos não autorizam marcar E01–E13 como realizados.
- **Depende de fora da máquina:** ambiente Windows Server/cliente de domínio com recursos, dependências de IIS/.NET/SQL, certificados e acesso administrativo; execução completa de E01–E13 e falhas, coleta de PIDs/logs/BLG/ACLs, gravação e envio no formulário. Se RDS completo for exigido, precisa de outra VM membro (`infra/README.md:356`).

### Incoerências

- Roteiro já fala em cinco migrações (`infra/ROTEIRO-VIDEO.md:9`); README, ficha de prova e formulário ainda enumeram três (`infra/README.md:195`, `infra/EVIDENCIAS.md:101`, `ENTREGA.md:186`). A correção do roteiro não alcançou esses documentos.
- Formulário aponta IP para linha 8 da ficha (`ENTREGA.md:175`); IP está na linha 10 (`infra/EVIDENCIAS.md:10`), e linha 8 é data/fuso.
- Checklist fala só de E01–E12 na coleta (`infra/EVIDENCIAS.md:5`); funcionamento completo é E13 e exige prova própria (`infra/EVIDENCIAS.md:164`). Não concluir a entrega só com os txt automáticos.

### O que mudou em relação ao ponto anterior

Os erros de contagem de migrações e de atribuir escrita ao painel foram corrigidos no roteiro (`docs/ROTEIROS-CONFERENCIA.md:41`, `docs/ROTEIROS-CONFERENCIA.md:47`). O corte não inclui instalação nem reinicialização. As pendências reais continuam sendo executar VM e provar o resultado, não reescrever essas cenas. Comparação literal anterior indisponível.

## 7. Physical Computing

### O que pede

ESP32/DHT22/LEDs com NTP, cálculo local, publicação assinada e vetor no Serial (`docs/PLANO.md:197`). Link do Wokwi, vídeo de até **4 min** e txt da equipe; LCD, buzzer e reconhecimento aparecem no checklist atual (`ENTREGA.md:205`, `ENTREGA.md:215`). **Sem divisão de pontos informada.**

### O que existe e funciona

- Firmware, circuito JSON, bibliotecas e configuração de exemplo. Quatro DHT22 e pinagem sem conflito estão descritos em `esp32/README.md:50` e `esp32/README.md:61`. **110 verificações estáticas passaram aqui** (`esp32/conferir_circuito.py:74`).
- LCD, extremos, prioridade dos alertas, buzzer/Ciente e asserts de arranque estão descritos (`esp32/README.md:76`, `esp32/README.md:109`). Os asserts só comprovam firmware executado quando observados na simulação; não compilados/executados aqui.
- Integração de quatro sensores com HMAC/ordem/formato antigo tem teste da API (`api/TermometroDaSala.Api.Tests/ApiTestes.cs:434`), não comprova broker público nem Wokwi ao vivo. O roteiro atual comprova aceitação no GET da Sala 7 (`esp32/ROTEIRO-VIDEO.md:44`).

### O que falta

- **Só a equipe:** completar txt, criar/publicar projeto e link, executar tabela de ensaio, registrar falhas e gravar vídeo de 3min30s (`esp32/README.md:23`, `esp32/README.md:122`). Também ensaiar frio, sensor inválido, reenvio, falta de rede e retorno do bip, fora do corte principal (`esp32/ROTEIRO-VIDEO.md:49`).
- **Execução local sem rede nem instalação:** conferência de arquivos/pinagem/vetores já feita; reconciliar contagens/valores iniciais nos documentos e preparar checklist. Não há compilador ESP32 comprovado nesta pasta; não inferir compilação aprovada.
- **Depende de fora da máquina:** Wokwi, bibliotecas do circuito, Wi-Fi/NTP/broker, API/SQL disponível e captura da aceitação das quatro mensagens. Simular e gravar com antecedência; o roteiro declara que, sem publicação, não existe prova de integração (`esp32/ROTEIRO-VIDEO.md:49`). Hardware real/calibração são evolução, não condição para entregar a simulação.

### Incoerências

- Plano diz diagrama inicial 33 °C/55% (`docs/PLANO.md:163`); README do circuito registra janela 35/60 e demais 24/60 (`esp32/README.md:52`). O roteiro atual reconhece isso e exige passagem em 24/40 (`esp32/ROTEIRO-VIDEO.md:20`). Não é um erro ainda aberto na preparação desse roteiro.
- Plano sugere 33 → 37 → 41 (`docs/PLANO.md:165`); roteiro ESP32 usa 24 → 29 → 33 → 37 (`esp32/ROTEIRO-VIDEO.md:29`), e pitch para em 37/55 porque já é Perigo (`arquitetura/PITCH.md:91`). São valores atuais coerentes com a fórmula; a instrução antiga pode causar salto suspeito se seguida indiscriminadamente.
- Sala 7 do exemplo (`esp32/README.md:36`) difere da Sala 3 do pitch (`arquitetura/PITCH.md:78`). **Há preparação explícita agora**, portanto não é integração impossível; requer dois projetos/configurações e bancos separados.
- Documentação do mapa manda repetir semeadura após 15 min (`docs/MAPA-TERMICO.md:50`), mas o README proíbe repetir depois de leitura do circuito (`esp32/README.md:46`), coerente com o 409 do código (`api/TermometroDaSala.Api/Controllers/DemoController.cs:24`). A renovação vale para cenário só simulado, não para sala já em uso.
- Roteiro diz que a API acusa leitura antiga em 15 min (`esp32/ROTEIRO-VIDEO.md:47`); desktop marca leitura antiga após 10 min (`desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:108`). O mapa usa janela de 15 min (`docs/MAPA-TERMICO.md:17`). Não há um único prazo geral para todos os indicadores.

### O que mudou em relação ao ponto anterior

Preparação agora recria banco exclusivo, inicia Demo sem perfil, semeia antes de publicar e trata a leitura inicial da janela (`esp32/ROTEIRO-VIDEO.md:10`, `esp32/ROTEIRO-VIDEO.md:18`, `esp32/ROTEIRO-VIDEO.md:20`). Os passos respeitam o limiar de suspeita >5 °C. Isso resolve instruções, mas ainda falta repetir o ensaio duas vezes (`docs/ROTEIROS-CONFERENCIA.md:78`). Comparação literal anterior indisponível.

## 8. Testing (TOGAF) e pitch

### O que pede

ArchiMate em quatro camadas, documento descritivo, abertura com equipe/problema e estimativa de impacto com fontes (`docs/PLANO.md:198`). Envio de `.archimate`, PNGs, PDF e pitch não listado de até **4 min** (`ENTREGA.md:220`, `ENTREGA.md:222`). Pesos: quatro visões **20% cada**, abertura **10%**, público/impacto **10%** (`ENTREGA.md:231`).

### O que existe e funciona

- Dois formatos de modelo e seis PNGs presentes. Validador executado: **111 elementos, 190 relações, seis visões**, equivalência e regras de relação aprovadas; autoteste de 12 casos. Isso comprova estrutura, não legibilidade das imagens nem PDF final (`arquitetura/README.md:19`).
- Documento descreve fases, implementação, decisões, riscos, fontes e impacto. A conta de **3.356.808** usa 3,5 milhões e 67.468/70.346, com a premissa explicitada (`arquitetura/DOCUMENTO.md:254`, `arquitetura/DOCUMENTO.md:256`). A alternativa de base 3 milhões está declarada (`arquitetura/DOCUMENTO.md:258`). Os **21 dias ≥34 °C** não são chamados de contagem oficial de ondas (`arquitetura/DOCUMENTO.md:292`). São texto/conta e fontes citadas; nenhuma fonte externa foi aberta nesta auditoria.
- Entrevista já tem relato e crédito autorizado no material, sem reprodução de identificação aqui (`desktop/README.md:29`, `arquitetura/PITCH.md:28`). A escola do piloto segue indefinida (`arquitetura/DOCUMENTO.md:262`). Não são a mesma pendência.
- Pitch de **3min30s** com reset, SQL dos quatro sensores/seis cadeiras, parâmetros específicos do Wokwi e verificação do GET antes da semeadura (`arquitetura/PITCH.md:39`, `arquitetura/PITCH.md:49`, `arquitetura/PITCH.md:76`, `arquitetura/PITCH.md:80`). É roteiro, sem prova de execução.

### O que falta

- **Só a equipe:** identificação completa, confirmar dados do piloto quando existir, montar slides, conferir fidelidade da fala, ensaiar/gravar pitch e entregar os quatro itens (`ENTREGA.md:239`, `ENTREGA.md:245`). Não inventar dimensão de piloto para preencher checklist.
- **Execução local sem rede nem instalação:** reconciliar contagens e descrição do mapa futuro; abrir modelo no Archi já instalado, conferir linhas/legibilidade e reexportar se necessário; exportar PDF com editor disponível e verificar seis imagens/capa. Confirmar pacote final e tempos. Não há PDF final nesta pasta.
- **Depende de fora da máquina:** integração Wokwi/API/desktop/celular, dados reais do piloto e confirmação de fontes externas, publicação do pitch e acesso anônimo ao link. Repetir preparação completa duas vezes e cronometrar, incluindo os quatro controles em 15 s (`docs/ROTEIROS-CONFERENCIA.md:72`, `docs/ROTEIROS-CONFERENCIA.md:78`).

### Incoerências

- Pitch afirma **81 API/9 desktop** (`arquitetura/PITCH.md:103`, `arquitetura/PITCH.md:108`), igual ao documento (`arquitetura/DOCUMENTO.md:219`, `arquitetura/DOCUMENTO.md:220`), mas a contagem atual é **85/18**. O resumo antigo não cobre automaticamente os casos adicionais; atualizar a fala só após resultado real.
- Pitch e raiz dizem que +1 h mantém últimas leituras (`arquitetura/PITCH.md:34`, `README.md:7`); especificação e clientes dizem que desloca pela previsão +2 h (`docs/MAPA-TERMICO.md:33`, `app/README.md:7`). O código aplica deslocamento (`api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21`). O defeito de descrição permanece no pitch já conferido.
- Checklist manda fazer entrevista/colocar fala (`ENTREGA.md:238`) e lista essa identificação como ausente (`ENTREGA.md:270`), mas ela já consta (`desktop/README.md:29`, `arquitetura/PITCH.md:26`). Falta confirmar piloto e equipe, não declarar que nenhuma entrevista existe.
- Meta falada é ação em até 15 min (`arquitetura/PITCH.md:112`); documento define **mediana** até 15 min (`arquitetura/DOCUMENTO.md:271`). A primeira pode ser lida como teto para todos os eventos, mais forte que a métrica escrita.
- A conta inicial do plano é salas × alunos por sala (`docs/PLANO.md:198`), enquanto documento explica ponderação por matrículas/turnos (`arquitetura/DOCUMENTO.md:260`). A alternativa atual é justificada; não voltar à conta antiga sem ressalva. As duas bases populacionais são premissas explicitadas, não um erro aritmético.

### O que mudou em relação ao ponto anterior

Não manter como bloqueadores atuais a ausência de reset, a falta de cadastro espacial, a publicação somente na Sala 7 ou o registro da ação no mural: o pitch documenta a preparação da Sala 3 e escreve pelo desktop (`arquitetura/PITCH.md:49`, `arquitetura/PITCH.md:76`, `arquitetura/PITCH.md:88`). `docs/ROTEIROS-CONFERENCIA.md:23` registra por que posições e cadeiras precisaram ser acrescentadas. Continuam pendentes o ensaio real, o PDF, a gravação e a correção da descrição de +1 h. Não foi possível cotejar o texto anterior literalmente.

## Pendências consolidadas por prazo e peso

Todos os itens de envio vencem em **19/11/2026 às 23h55**; “até 18/11” abaixo é meta interna, não prazo adicional do professor. Dentro de cada prazo, itens com peso explícito mais abrangente vêm primeiro; não se atribui perda automática de toda a nota por uma evidência ausente. “Local” pressupõe ferramentas já instaladas; nenhuma instalação foi feita. Recursos futuros não exigidos não foram promovidos a bloqueadores da entrega.

| Prazo / ordem | Falta concreta | Responsabilidade / dependência | Disciplina e peso informado | Referência |
|---|---|---|---|---|
| Antes dos ensaios; até 18/11 | Disponibilizar VM e cliente, executar rede/AD/DNS, IIS/SQL/HTTPS, alertas, recuperação e logs; preencher E01–E13 e formulário com resultados reais | Equipe + ambiente externo preparado | Operating Systems: 2+2+2+1+1+2 = 10 pontos | `ENTREGA.md:148`; `infra/EVIDENCIAS.md:3` |
| Antes dos ensaios; até 18/11 | Confirmar se RDP restrito atende RDS; se não, preparar VM membro e coleção RDS | Equipe + responsável pela disciplina + recursos externos | Operating Systems: sem peso separado para RDS | `infra/EVIDENCIAS.md:12`; `infra/README.md:356` |
| Antes de fechar pacote; até 18/11 | Conferir TypeScript estrito com compilador/dependências disponíveis, sem download nesta tarefa | Ambiente preparado; execução local | Mobile: 30% | `ENTREGA.md:133`; `app/package.json:14` |
| Antes dos ensaios; até 18/11 | Validar app no celular: fluxo, persistência, erro/vazio, câmera negada/aceita; provar envio real se alegar integração | Equipe + celular/Expo/API | Mobile: 40% fluxo, 20% recurso nativo | `ENTREGA.md:132`; `app/ROTEIRO-VIDEO.md:15` |
| Antes dos ensaios; até 18/11 | Publicar circuito e preencher link, observar asserts, quatro mensagens aceitas e tabela de ensaio; testar frio/falhas fora do corte | Equipe + Wokwi/NTP/broker/API | Physical: sem divisão de pontos | `esp32/README.md:23`; `esp32/README.md:122` |
| Antes dos ensaios; até 18/11 | Repetir duas vezes cada preparação, em seus bancos descartáveis; no pitch cadastrar quatro sensores/seis cadeiras e configurar projeto separado; comprovar GET/201/sem suspeita | Equipe + SQL/EF/Wokwi/API | Physical, integração ML (1,0) e demonstração do pitch; sem peso isolado para preparo | `docs/ROTEIROS-CONFERENCIA.md:78`; `arquitetura/PITCH.md:76` |
| Antes dos ensaios; até 18/11 | Reconciliar índice SQL com B1/B2/97 linhas OK; distinguir coleta automática de prints manuais e validar imagens | Local + conferência visual da equipe | SOA: banco 3,0; migrações 2,5; testes manuais 1,5 | `api/evidencias/README.md:19`; `api/evidencias/saidas/B2.txt:7` |
| Antes de fechar material; até 18/11 | Reexecutar suítes em ambiente preparado e atualizar contagens/resumos/falas; esperado estaticamente 85 API e 18 desktop | Local, com dependências já presentes | SOA/desktop/Testing: sem peso isolado para quantidade de testes | `api/evidencias/dotnet-test.txt:5`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Antes de fechar material; até 18/11 | Reconciliar migrações 3/4/5, `/niveis` 4/5, Frio, comandos Demo, valores iniciais e prazo de leitura antiga | Local | SOA, Physical, Operating Systems: pesos dos respectivos critérios, sem peso documental isolado | `ENTREGA.md:40`; `CONTRATO.md:214`; `infra/EVIDENCIAS.md:101` |
| Antes de fechar material; até 18/11 | Alinhar descrição de mapa +1 h com deslocamento implementado; distinguir estudo de +2 h por ponto da previsão servida | Local | Mobile/desktop/Testing: sem peso específico para mapa | `README.md:7`; `docs/MAPA-TERMICO.md:33`; `ml/README.md:106` |
| Antes de fechar material; até 18/11 | Resolver/documentar divergência da âncora de semeadura e manter limite conhecido explícito | Local; somente apontado nesta auditoria | SOA/demonstração: sem peso isolado | `CONTRATO.md:184`; `api/TermometroDaSala.Api/Controllers/DemoController.cs:79` |
| Antes de fechar material; até 18/11 | Atualizar anexo com provas locais posteriores; repetir cenários no IIS, coletar GPO/nmap/auditoria/app/LED; revisar fotos | Local + equipe + VM/cliente/Wokwi | Cyber: evidências 2,0; controles 2,5 | `seguranca/ANEXO-EVIDENCIAS.md:34`; `seguranca/ANEXO-EVIDENCIAS.md:46` |
| Antes de fechar material; até 18/11 | Manter concorrência, pixels JPEG e segredo compartilhado explicitamente limitados; correções locais possíveis fora desta tarefa | Local; nenhuma correção autorizada aqui | Cyber: análise 3,0 e controles 2,5; sem desconto isolado informado | `seguranca/RELATORIO-RISCOS.md:239`; `seguranca/RELATORIO-RISCOS.md:242`; `api/TermometroDaSala.Api/Controllers/DemoController.cs:33` |
| Antes de fechar material; até 18/11 | Conferir apresentação do desktop com API, modo local, persistência, CRUD, CSV e revogação; atualizar caminhos antigos da entrega | Equipe + execução local preparada | C#: sem divisão de pontos | `desktop/README.md:88`; `ENTREGA.md:72` |
| Antes de fechar material; até 18/11 | Confirmar tamanho da equipe e, se 4–5, avaliação completa da sala + delta vs baseline; revalidar métricas/paridade e contagem do notebook | Equipe decide enquadramento; execução local preparada | ML: avaliação 2,0 e integração 1,0; requisito condicional do plano | `docs/PLANO.md:194`; `ml/gerar_notebook.py:340` |
| Antes de fechar material; até 18/11 | Completar identificação em READMEs, txt, relatório, documento, slides, `authors`, notebook e gerador; manter saídas ao regenerar | Só equipe fornece dados; aplicação local após fornecimento | Mobile: requisito de entrada; Testing: abertura 10%; demais sem peso isolado | `ENTREGA.md:260`; `ENTREGA.md:136`; `ENTREGA.md:235` |
| Antes de fechar material; até 18/11 | Validar entrevista já registrada; distinguir referência entrevistada do piloto ainda indefinido; alinhar mediana/teto de 15 min e dados de impacto | Equipe + dados externos do piloto; reconciliação local | Testing: público/impacto 10%; README C# sem peso separado | `desktop/README.md:29`; `arquitetura/DOCUMENTO.md:262`; `arquitetura/DOCUMENTO.md:271` |
| Até 18/11 | Conferir seis visões/PNGs no Archi, legibilidade e seis imagens no documento; exportar PDF com capa | Local com ferramentas disponíveis + conferência visual | Testing: quatro visões de 20% cada; abertura 10% | `ENTREGA.md:223`; `ENTREGA.md:240` |
| Até 18/11 | Consolidar relatório e anexo em PDF único, legível e sem identificação indevida nas imagens | Local com editor disponível + conferência da equipe | Cyber: produto final; sem peso separado para formato | `ENTREGA.md:51`; `ENTREGA.md:62` |
| Até 18/11 | Confirmar PR revisado, proteção de ramo e versões/tags declaradas; não criar duplicatas por checklist antigo | Equipe + consulta remota posterior | C#: requisito sem peso dividido | `docs/PLANO.md:211`; `desktop/README.md:257` |
| Até 18/11 | Gravar e publicar vídeos: ML 1:50/2:00, Mobile 1:50/2:00, Physical 3:30/4:00, infra 6:30/7:00, pitch 3:30/4:00; conferir links e acesso | Equipe + aparelhos/plataformas externas | ML, Mobile, Physical, Operating Systems e Testing; sem peso separado para cada vídeo | `ml/ROTEIRO-VIDEO.md:3`; `docs/ROTEIROS-CONFERENCIA.md:67`; `ENTREGA.md:225` |
| Até 18/11 | Gerar ZIPs do app/desktop sem dados privados/dependências, conferir estrutura e testar desktop extraído em pasta limpa | Execução local preparada + equipe | Mobile e C#: formatos obrigatórios, sem peso isolado | `ENTREGA.md:30`; `ENTREGA.md:86` |
| 19/11, 23h55 | Enviar SOA: link do repositório; Cyber: PDF; C#: ZIP e txt; ML: notebook e links de vídeo/repositório | Equipe + plataforma externa | Pesos nas seções 1–4 | `ENTREGA.md:34`; `ENTREGA.md:51`; `ENTREGA.md:66`; `ENTREGA.md:103` |
| 19/11, 23h55 | Enviar Mobile: ZIP/vídeo; Physical: Wokwi/vídeo/txt; Testing: modelo/PNGs/PDF/pitch | Equipe + plataforma externa | Pesos nas seções 5, 7 e 8 | `ENTREGA.md:128`; `ENTREGA.md:205`; `ENTREGA.md:220` |
| 19/11, 23h55 | Enviar Operating Systems pelo formulário com dados reais, repositório e vídeo, não apenas tarefa geral | Equipe + formulário externo | Operating Systems: 10 pontos distribuídos conforme seção 6 | `ENTREGA.md:144` |
| Após avaliação | Revogar IDs de todos os projetos de demonstração e revisar cópias de fotos | Equipe + ambientes utilizados | Segurança posterior; sem peso isolado informado | `esp32/README.md:142`; `CONTRATO.md:75` |
| Antes de piloto real, sem confundir com prazo da simulação | Calibração, dados reais/delta, confirmação independente do modelo e broker protegido; levantar números reais do local | Campo + infraestrutura externa | Evolução; sem peso de entrega informado | `ml/README.md:97`; `seguranca/RELATORIO-RISCOS.md:250`; `esp32/README.md:143` |

## Fechamento

- **Feito:** auditoria das oito disciplinas, provas discriminadas, incoerências com referências e tabela única de pendências por prazo/peso.
- **Arquivos:** somente `docs/AUDITORIA-ENTREGA.md`.
- **Testes:** 20 casos do app; 110 verificações do circuito; modelo ArchiMate e 12 autotestes; vetores de calor/HMAC; sintaxe de dez scripts. Inspeção de notebook, igualdade dos modelos e contagem estática dos testes. Resultados .NET e métricas foram apenas lidos dos registros existentes.
- **Pendências:** executar ensaios reais, completar identificação, consolidar provas, gerar pacotes/PDFs, gravar/publicar e enviar conforme tabela. Sem acesso ao texto da auditoria anterior sob a restrição de histórico.
- **Premissas que caíram:** roteiros atuais não usam perfil HTTP para Demo; pitch já prepara Sala 3 com planta e sensores; ação é no desktop; roteiro infra já cita cinco migrações; entrevista e links de PR/tags já têm texto; índice de coleta e contagens de testes não representam integralmente os arquivos atuais.

Não foram executados .NET, treino/notebook, TypeScript, banco, aplicações, VM, Wokwi ou exportação visual; fontes e links externos não foram revalidados.
Risco principal: roteiro corrigido e saída histórica não comprovam integração atual; concorrência de leituras e limitações do modelo continuam abertas.
