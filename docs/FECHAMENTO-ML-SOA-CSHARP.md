# Fechamento de ML, SOA e C#

Equipe: Samuel David Baecker — RM 559269; Lucas de Almeida Villar — RM 560005; Guilherme Augusto Caseiro — RM 559765.

Prazo: **19/11/2026 às 23h55**, horário de São Paulo. Este roteiro foi preparado inicialmente no cloud Linux e continuado no computador Windows em 17/11/2026.

## Atualização técnica — modelo geral com ajuste por sala

A versão local agora usa o mesmo modelo de rua com ajuste observacional separado por sala. O terceirão, com 20 alunos e 1 professor, é somente a demonstração. A estimativa de potência dos ocupantes é auxiliar e não altera a previsão em graus. O [documento V1](../output/especificacao-v1/Termometro-da-Sala-V1.docx) foi atualizado e suas quatro páginas foram conferidas.

Nesta rodada: **API 138/138 (19/11); ML 48 testes e 56 subtestes; desktop 22/22 e autoteste 6/6; Mobile 24/24 e TypeScript sem erros**. Os [comandos, hashes, resultados e limites](resultados-testes-2026-11-17/ml-modelo-geral-v1.txt) registram a versão atual. Modelos exportados e notebook mantiveram os hashes; não houve retreino. A precisão do ajuste em salas reais ainda precisa de dados locais e avaliação temporal.

**Validação complementar concluída nesta versão:** tela Windows, mapa Agora/+1 h e os dois PNGs exportados foram inspecionados, incluindo C01–C20, S1–S4, Frio azul, rolagem até C20 e precauções completas. [Relatório visual e imagens](../desktop/docs/modelo-geral-v1/VALIDACAO.md). SQL Server real repetido: **98/98 verificações do coletor e 33/33 HTTP/MQTT/SQL**, incluindo árvore exportada e ajuste por sala; [comandos e evidências](resultados-testes-2026-11-17/sql-modelo-geral-v1.txt). Nenhuma correção na API ou na interface foi necessária nessa rodada.

Os pacotes desta versão são identificados pelos manifestos atuais em `entrega-pacotes.sha256`, `output/fechamento-samuel/SHA256SUMS.txt` e `output/revisao-grupo/pacotes-grupo.sha256`; [conferência das cópias](../output/fechamento-samuel/CONFERENCIA-PACOTES.txt). Em 17/11, a cópia isolada aprovou API 134, desktop 19 + 6 autotestes, ML 46 + 53 subtestes e Mobile 23 + TypeScript (histórico). A configuração pública de conexão da API passou a ser incluída no handoff para permitir executar os testes da cópia sem configuração privada. Os resultados e hashes antigos abaixo são históricos. Commit e publicação ficam com Samuel, responsável pelo repositório; não houve novo envio ao grupo. A evidência manual do Insomnia não foi repetida: permanece o ensaio anterior de 29/29.

## Critérios oficiais conferidos — 17/11/2026

O enunciado `GS CitizenTech - 3ES Ago- FIAP 2026 .pdf`, com 37 páginas, foi localizado em Downloads e conferido nas páginas 27 (SOA), 29 (C#) e 30 (ML). SHA-256: `70AB581A0F9C914DC3E6A528BA30B35283779030C2B791A29D4AD0DE5D2A7201`.

| Disciplina | Critério oficial | Evidência e entrega |
|---|---|---|
| SOA | Domínio 3,0; banco 3,0; migrações 2,5; testes manuais Insomnia 1,5 | Código, cinco migrações, SQL real e índice em `api/evidencias/README.md`; entregar link GitHub com README e evidências |
| C# | WinForms funcional, lógica/validação/persistência, POO, interface intuitiva, README e versionamento semântico com PR | Suíte 22/22 e autoteste 6/6 (19/11), relatório visual, entrevista confirmada, PR/tags existentes; entregar ZIP do projeto e TXT da equipe |
| ML | Problema/dados 2,0; preparação/análise 2,0; modelo/treino 3,0; avaliação 2,0; integração 1,0 | Notebook, dados com origem, modelo, métricas, README e previsão na solução; entregar `.ipynb` e link do vídeo de até 2 minutos |

Samuel confirmou em 17/11 a fidelidade do relato e a autorização para citar a entrevista com Monalisa Daniela Pereira Amaral, CRAS Santo Amaro, de 07/11/2026. Magister e Salgueiro são candidatas a um piloto futuro; unidade, bairro, contato, autorização e execução ainda não estão confirmados. O enunciado dessas três disciplinas permite dados públicos/simulados em ML e não exige piloto físico, foto real de escola, DPI ampliado, proteção de branch ou vídeo de C#.

## Revisão do grupo — 17/11/2026

- **Conferido no Windows:** SDK/runtime .NET 8 disponíveis, API ligada ao SQL Server Express em banco exclusivo de ensaio e cinco migrações aplicadas. Cadastro, ação, CSV, revogação, mapa Agora/+1 h e entrada/retorno do modo local foram exercitados pela interface.
- **Novo visual revalidado:** cabeçalho azul-escuro, botões turquesa e cartões claros, com painel maximizado e em 958 × 707 com rolagem, validação/cancelamento de cadastro e aviso longo de modo local legíveis. Mapa com vinte cadeiras C01–C20, quatro sensores S1–S4, frio em azul, Agora/+1 h e dois PNGs completos. O [relatório do ensaio](../desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md) preserva o histórico e as imagens. A previsão de +1 h é uma aproximação; as cenas são simuladas.
- **Suítes concluídas com autorização de Samuel:** API **101/101**, desktop **19/19** e autoteste **6/6**, Mobile **23/23** e TypeScript sem erros. Resultados em [resultados-testes-2026-11-17](resultados-testes-2026-11-17/); os registros desktop e Mobile já incluem o acabamento visual. Cadastro e ação locais também persistiram após fechar e reabrir o desktop com a API desligada, às 02h58. A conferência com DPI ampliado ainda não foi registrada.
- **Conferências Python concluídas:** ML **23 testes e 9 subtestes aprovados**, saída 0, em Python 3.11.9 portátil isolado em D:, com os pins do projeto preservados. Sete arquivos do notebook/modelos/dados mantiveram os mesmos hashes. [Comandos, ambiente e resultado](resultados-testes-2026-11-17/ml-pytest.txt). Modelo ArchiMate válido nos dois arquivos, com 111 elementos, 190 relações e seis visões, inclusive regras do Archi instalado; 12 autotestes e 110 verificações estáticas do circuito aprovados. O bloqueio inicial do runtime 3.12 foi superado e arquivado como histórico. Esses checks não comprovam Archi GUI, Wokwi ou execução em campo.
- **ZIP desktop aprovado em 17/11 (histórico; o ZIP é regenerado do commit final em 19/11, com 22 testes):** `entrega-desktop.zip` foi atualizado com as últimas evidências e extraído em pasta exclusiva de D:. A cópia passou **19 testes e 6 verificações do autoteste**, saída 0; [TRX atual](resultados-testes-2026-11-17/desktop-pacote-fechamento.trx) e [registro completo](../output/fechamento-samuel/CONFERENCIA-PACOTES.txt). São 47 entradas, 496.805 bytes, SHA-256 `ee0b62dc23c40aa58cc1e10ed2c3ce834c73493e2b198b2f47b65d41c4d3f6dd`. O JSON de configuração foi gerado só com a URL pública local, sem copiar configs existentes. A consulta de vulnerabilidades NuGet não foi repetida nesta extração. O [handoff de fontes](../output/fechamento-samuel/COMECE-AQUI.txt) também compilou API/Ensaio e publicou o desktop em pasta isolada; não iniciou banco nessa conferência. Mobile e seu pacote continuam separados desse resultado. O [checklist geral](../ENTREGA.md) registra as oito disciplinas.
- **Insomnia concluído:** 29 requisições e **29/29 asserções aprovadas**, zero falhas, em API/SQL locais; [captura e condições do ensaio](../api/evidencias/insomnia-2026-11-17/README.md). **Com a equipe:** gravar/publicar o vídeo de ML, publicar as fontes/evidências no repositório, revisar o acesso aos links e enviar os arquivos.

**Fechamento técnico adicional:** a API passou **101/101 testes**, incluindo a regressão que repete o Demo em dias diferentes sem perder a janela recente da previsão: [TRX](resultados-testes-2026-11-17/api-fechamento-101.trx). A coleta em SQL Server Express registrou **98 verificações OK e zero divergências**: [conferência](../api/evidencias/TermometroGS_SOA_20261117_Validado/conferencia.tsv). O ensaio com broker MQTT, assinante real, HTTP e SQL Server passou **21 verificações**, incluindo dez corridas HTTP/MQTT, revogação, rollback e repetição após falha controlada: [resultado](resultados-testes-2026-11-17/TermometroGS_MQTT_20261117_091654/resultado.txt). Não é prova de IIS/VM, Wokwi ou piloto físico.

**Publicação verificada:** a `main` consultada em 17/11 permanece em `7a8e4e35eeb3827200c27efc085f5158898879eb`. O [PR 1](https://github.com/sbaecker2005/termometro-da-sala/pull/1) está aberto, de `desktop/v0.2.0` para `desktop/v0.1.0`, sem merge/reviews; releases `v0.1.0` e `v0.2.0` existem, sem pacotes anexos. Os ajustes locais de 17/11 foram publicados depois na `main`. A existência de PR e versões foi confirmada; aprovação de PR e proteção de branch são práticas de revisão, não requisitos adicionais explícitos da página 29.

**Notebook reexecutado integralmente:** as **19 células de código passaram em 31,95 s**, saída 0, em cópia isolada no D:, com Python 3.11.9, IPython.display e Matplotlib Agg. Os modelos JSON de rua e pontos foram estruturalmente idênticos aos originais; a paridade API foi idêntica byte a byte. As demais diferenças de hashes dos artefatos regenerados desapareceram ao normalizar CRLF/LF. Os **12 arquivos originais** conferidos mantiveram seus hashes, incluindo notebook, modelos e dados. [Relatório e comando de reprodução](resultados-testes-2026-11-17/ml-notebook-integral.txt). Essa execução sequencial não usa kernel Jupyter/nbclient; não houve inspeção visual das figuras regeneradas nem regravação das saídas originais.

O status desta passagem é **em revisão pelo grupo**, com pendências explícitas antes do envio.

## 1. Conferir ou instalar .NET 8 no Windows

Abra o PowerShell **do Windows**, na pasta extraída do projeto:

```powershell
.\ferramentas\preparar-windows.ps1
```

O script verifica o SDK 8, instala o pacote oficial `Microsoft.DotNet.SDK.8` pelo winget somente se estiver ausente e confere o Windows Desktop Runtime 8. A instalação pode solicitar elevação pelo Windows. Se o winget não estiver disponível, use o **SDK para Windows** em <https://dotnet.microsoft.com/download/dotnet/8.0> e execute o script novamente.

Somente o runtime não basta para compilar e testar. SDK e runtime já foram confirmados no computador Windows deste ensaio em 17/11/2026. Nos demais computadores da equipe, execute a verificação acima.

## 2. Verificar os projetos sem precisar de SQL Server

As suítes da versão atual foram executadas em 17/11 com autorização de Samuel: API 134/134, desktop 19/19 e autoteste 6/6 aprovados. Os TRX e o registro atual estão em `docs/resultados-testes-2026-11-17`, com sufixo `modelo-geral-v1`. Para reprodução pelo coordenador, os comandos gerais na raiz são:

```powershell
dotnet test .\api\TermometroDaSala.sln
if ($LASTEXITCODE -ne 0) { throw 'Testes da API falharam.' }
dotnet test .\desktop\TermometroDaSala.Desktop.sln
if ($LASTEXITCODE -ne 0) { throw 'Testes Windows do desktop falharam.' }
dotnet run --project .\desktop\TermometroDaSala.Desktop -- --autoteste
if ($LASTEXITCODE -ne 0) { throw 'Autoteste do desktop falhou.' }
```

A API usa SQLite em memória nos testes. A suíte inclui quatro testes que compilam os serviços reais do desktop e os ligam aos controllers reais da API: cadastro/ação, recusa sem chave, leitura/previsão/revogação e mapa atual/futuro. Eles não carregam as telas nem validam SQL Server. Os testes do projeto Windows e o autoteste exigem o Windows. As contagens acima vêm da execução atual; o ensaio visual e o banco SQL isolado têm evidência própria no relatório do desktop.

## 3. Preparar um banco SQL Server isolado para o ensaio

**Neste computador, a apresentação completa já foi preparada com sucesso pelo comando abaixo**, usando SQL Express instalado, banco novo, chave temporária somente em memória, previsão e desktop do kit:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\ferramentas\abrir-apresentacao.ps1
```

O script recusa a execução se a porta 5080 estiver ocupada; não encerra processos anteriores. Em 17/11 às 09h45, retornou rua **30,3 °C**, delta **12,5 °C**, sala prevista **42,8 °C/Perigo**, `semLeiturasRecentes=false`: [resposta real da API](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/previsao-ml.txt). São dados de demonstração preparados por HTTP, não medições reais da escola. A [tela maximizada](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/desktop-previsao-ml.png) confirmou a previsão +2 h; [mapa](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/mapa-tela.png) e [PNG exportado](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/mapa-exportado.png) foram inspecionados, com C01–C20, S1–S4, frio em azul, legenda e lista completas sem cortes. Para encerrar depois da apresentação, use os PIDs específicos no `PROCESSOS.txt` da pasta da execução. Os bancos são preservados.

As instruções manuais abaixo são a alternativa para outros computadores:

**Proteção do iniciador corrigida e testada:** a checagem anterior de porta com `Get-NetTCPConnection -ErrorAction SilentlyContinue` podia ignorar uma falha da consulta. Foi substituída por reserva TCP exclusiva (`TcpListener`/`ExclusiveAddressUse`), que interrompe a abertura quando não consegue reservar a porta. A segunda abertura foi recusada com saída 1 tanto no host quanto no sandbox, preservando a API original PID 12128: [prova](../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/protecao-porta.txt). Os processos adicionais da tentativa anterior foram encerrados; o banco e os registros dessa tentativa foram preservados. A apresentação original permaneceu disponível. A regressão está corrigida, sem ação pendente para Samuel.

Confira se há uma instalação de SQL Server/LocalDB. **O SDK .NET não instala SQL Server.** Se ainda faltar, instale SQL Server Express 2022 com LocalDB pelo instalador oficial da Microsoft, ou use a alternativa Docker documentada em `api/README.md`.

Para LocalDB, no PowerShell do Windows, crie uma instância própria quando necessário e um banco novo por ensaio. Nenhum banco anterior é apagado:

```powershell
$instancias = @(SqlLocalDB info)
if ($LASTEXITCODE -ne 0) { throw 'LocalDB nao encontrado ou indisponivel.' }
if ($instancias -notcontains 'TermometroGS') {
    SqlLocalDB create TermometroGS
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao criar a instancia LocalDB.' }
}
SqlLocalDB start TermometroGS
if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar LocalDB.' }
$bancoGS = 'TermometroGS_' + (Get-Date -Format 'yyyyMMdd_HHmmss')
$env:ConnectionStrings__Sala = "Server=(localdb)\TermometroGS;Database=$bancoGS;Integrated Security=True;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = 'Demo'
$env:DOTNET_ENVIRONMENT = 'Demo'
$env:Mqtt__Desligado = 'true'
$env:Banco__Migrar = 'true'
$chaveGS = Read-Host 'Defina uma chave apenas para este ensaio local' -AsSecureString
$env:ChaveCoordenacao = [Net.NetworkCredential]::new('', $chaveGS).Password
$env:TERMOMETRO_CHAVE_COORDENACAO = $env:ChaveCoordenacao
```

Não fotografe ou exiba variáveis de chave/conexão. A chave acima será herdada pelo desktop iniciado na mesma janela. No Insomnia, configure-a no ambiente privado antes da gravação, sem mostrá-la na tela.

Ainda nessa janela, compile e abra o desktop, aplique as migrações e inicie a API:

```powershell
dotnet build .\desktop\TermometroDaSala.Desktop.sln
if ($LASTEXITCODE -ne 0) { throw 'Build do desktop falhou.' }
Start-Process .\desktop\TermometroDaSala.Desktop\bin\Debug\net8.0-windows\TermometroDaSala.Desktop.exe
Push-Location .\api
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Restauracao do dotnet-ef falhou.' }
    dotnet ef database update --project TermometroDaSala.Api
    if ($LASTEXITCODE -ne 0) { throw 'Migracao SQL Server falhou.' }
    dotnet ef migrations list --project TermometroDaSala.Api --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Conferencia das migracoes falhou.' }
    dotnet run --project TermometroDaSala.Api --no-build --no-launch-profile --urls http://localhost:5080
} finally { Pop-Location }
```

Use **Atualizar** no desktop depois de a API iniciar: ele pode ter aberto primeiro no modo local. Deixe o terminal da API aberto. Abra um segundo PowerShell na raiz para os próximos passos.

## 4. ML: preparar a previsão e gravar até dois minutos

Na segunda janela, execute uma vez sobre o banco novo:

```powershell
.\ferramentas\preparar-cena-ml.ps1
dotnet test .\api\TermometroDaSala.Api.Tests\TermometroDaSala.Api.Tests.csproj --filter FullyQualifiedName~PrevisorModeloTestes
if ($LASTEXITCODE -ne 0) { throw 'Paridade do modelo falhou.' }
```

O script envia uma **leitura simulada por HTTP**, assinada com o vetor público da sala 3, prepara 18 leituras históricas e consulta a previsão real da API. Não é evidência de funcionamento do Wokwi. Se repetido no mesmo segundo, aguarde um segundo para evitar reenvio. Use apenas o banco de ensaio: a preparação substitui leituras simuladas dessa sala. Fora de Demo, o POST de preparo é recusado pela API.

Mostre no vídeo: problema; INMET A701 2022–2025; divisão temporal; árvore e baseline; MAE **0,909 °C contra 1,442 °C**; limite do recall **19 de 48 horas de cuidado detectadas**; e a previsão aparecendo em `GET /ambientes/sala-03/previsao` e no desktop após **Atualizar**. O valor de agora varia. **27,16 °C** pertence ao vetor fixo do notebook/teste, não é uma promessa para qualquer horário.

Use `ml/ROTEIRO-VIDEO.md`. Envie notebook, link do vídeo de até dois minutos e link do repositório com dataset, origem, modelo, métricas e README. Não regenere o notebook para mudar nomes: suas 19 células de código e 49 saídas já foram preservadas.

## 5. SOA: coletar as evidências no Insomnia

Importe `api/insomnia.json` e configure o ambiente local. Siga `api/README.md`, seção do Insomnia, e `api/evidencias/README.md`.

Capture método, rota, status e resposta para: leitura válida/recusada; cadastro/consulta/alteração/exclusão de sala; ação; ocorrência/foto; listagem; previsão; níveis e mapa. Confira também as cinco migrações, tabelas e registros no SQL Server. Use um banco diferente do ensaio de ML se quiser repetir as requisições na ordem original: a sala-05 deve estar livre e a revogação da sala-01 altera o estado.

Saídas automáticas e testes SQLite não substituem as evidências manuais do Insomnia. Envie o link do GitHub com README, código e provas atuais, sem credenciais.

## 6. C#: validar telas e ZIP

Com API e desktop ligados, confira cadastro com erro por campo e cadastro válido, leitura e previsão, mapa atual/futuro, registro de ação, exportação CSV e revogação confirmada. Depois pare **somente a API deste ensaio**, atualize e confira o aviso de modo local; reinicie e confira a volta à API. Não presuma sincronização dos dados locais.

Editar/excluir salas existem na API e ficam na demonstração de SOA; não há esses botões no desktop. O enunciado de C# pede protótipo funcional, lógica/POO/persistência/interface, README, versionamento semântico com PR e ZIP/TXT; não exige esses dois botões nem um vídeo.

Gere o ZIP sem `bin`, `obj` ou configurações privadas, extraia numa pasta nova e execute build, testes e autoteste nessa cópia. Envie o ZIP do projeto e `desktop/integrantes.txt`. Confira o PR e as tags já existentes; não é necessário criar outra versão apenas para repetir uma tag.

## Pendências de Samuel e da publicação

- **Validação local concluída:** versão atual com tela/PNG, SQL 98/98 + HTTP/MQTT/SQL 33/33, testes e conferência das cópias indexados no início deste documento. Insomnia 29/29 permanece como evidência manual anterior. Esses ensaios concluídos não são trabalho pendente do grupo.
- **Samuel:** gravar e publicar o vídeo final de ML, com no máximo dois minutos, usando `ml/ROTEIRO-VIDEO.md`; inserir o link com acesso para o professor.
- **Samuel (responsável pelo repositório):** revisar e publicar as fontes/evidências atuais no repositório, com o fluxo de commit/PR/merge que coordena. Usar os pacotes e manifestos atuais, não os hashes históricos das seções anteriores.
- **Um integrante:** revisar o acesso aos links e enviar SOA (link GitHub), C# (ZIP e TXT) e ML (notebook e vídeo, com repositório) nas tarefas corretas até o prazo.

DPI ampliado não foi inspecionado; isso é limite de cobertura visual, não uma pendência oficial de entrega. Piloto/calibração e validação independente futura do modelo continuam necessários antes do uso real, mas não substituem nem bloqueiam a demonstração simulada exigida. Mobile, VM, Wokwi, Cyber e pitch mantêm os responsáveis e as pendências próprias em `ENTREGA.md`.
