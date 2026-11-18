# Entrega — Termômetro da Sala

Prazo: **19/11/2026, 23h55**. Um integrante envia cada disciplina no Teams, menos Operating Systems, que vai por formulário. Repositório: https://github.com/sbaecker2005/termometro-da-sala

O briefing pede ZIP só em C# e em Mobile. Nas outras, o envio é link, PDF, vídeo ou formulário, e não há arquivo para compactar.

## Revisão do grupo - 17/11/2026

**Atualização do modelo geral:** as fontes locais aplicam ajuste por sala ao modelo compartilhado, com o terceirão apenas na demonstração. Rodada atual: API **134/134**, ML **46 testes e 53 subtestes**, desktop **19/19 + 6 autotestes**, Mobile **23/23** e TypeScript sem erros. [Registro técnico](docs/resultados-testes-2026-11-17/ml-modelo-geral-v1.txt); [documento V1](output/especificacao-v1/Termometro-da-Sala-V1.docx). **Tela e PNGs conferidos nesta versão**, com [evidências atuais](desktop/docs/modelo-geral-v1/VALIDACAO.md). SQL real: **98/98 + 33/33 HTTP/MQTT/SQL**, [registro](docs/resultados-testes-2026-11-17/sql-modelo-geral-v1.txt). Pacotes atualizados com configurações públicas e [verificação das cópias](docs/resultados-testes-2026-11-17/pacotes-modelo-geral-v1.txt); hashes nos manifestos externos ligados abaixo. Insomnia 29/29 permanece como evidência manual anterior.

Meta: nota 10 em todas as disciplinas. Este kit prepara a revisão; os itens abaixo só ficam concluídos com a evidência indicada, sem promessa de nota. O enunciado oficial `GS CitizenTech - 3ES Ago- FIAP 2026 .pdf` foi localizado em Downloads; suas páginas 27, 29 e 30 foram conferidas para SOA, C# e ML. SHA-256 `70AB581A0F9C914DC3E6A528BA30B35283779030C2B791A29D4AD0DE5D2A7201`. O [fechamento de Samuel](docs/FECHAMENTO-ML-SOA-CSHARP.md) relaciona os critérios às provas.

| Disciplina | Preparado para revisar | O grupo ainda precisa concluir |
|---|---|---|
| SOA | API 134/134; cinco migrações; SQL Express 98/98 e MQTT/HTTP/SQL 33/33 nesta revisão. Insomnia anterior com 29/29 asserções | Revisar/publicar código e evidências pelo Cérebro, conferir acesso e enviar link |
| C# | Fontes/cópia isolada: 19/19 testes e seis autotestes; tela e horário, mapa C01-C20/S1-S4, frio azul, PNG Agora/+1 h e rolagem conferidos na versão atual; entrevista, PR e versões já conferidos | Revisão final do grupo, publicação pelo Cérebro e envio do ZIP + TXT atuais |
| ML | 46 testes e 53 subtestes; modelo geral com ajuste por sala e casos compartilhados com a API; modelos/notebook preservados. Histórico: 19/19 células reexecutadas, 31,95 s, modelos estruturalmente idênticos e integração visível no desktop | Gravar vídeo de até 2 min mostrando a integração atual; conferir links e enviar. Precisão real por sala depende de dados de campo |
| Mobile | Instalação limpa em D:, 23/23 testes e TypeScript sem erros, também no ZIP final extraído; prévia web conferida em 390 × 844 e 320 × 740, incluindo mapa, formulário, erro/vazio e identificação persistida após recarregar | Ensaiar Expo Go, câmera e retorno dos Ajustes no aparelho, teclado/fontes ampliadas, persistência nativa e integração HTTP real; gravar até 2 min e enviar ZIP + vídeo |
| Operating Systems | Scripts e roteiros de instalação, coleta de evidências e vídeo | Montar/ensaiar VM e cliente; comprovar AD/DNS/IIS, monitoramento, recuperação, auditoria e fluxo; preencher evidências e formulário; gravar até 7 min e enviar |
| Cybersecurity | Relatório/anexo reconciliados, PDF de revisão inspecionado e cinco regressões SQLite de replay/rollback aprovadas; riscos e limites preservados | Repetir cenários no laboratório; coletar IIS/GPO/RDP/NTFS, nmap de duas origens e funcionamento sem broker; completar evidências, aprovar PDF final e enviar |
| TOGAF | Modelo validado: 111 elementos, 190 relações, seis visões e 12 autotestes; seis diagramas e PDF de revisão inspecionados | Conferir abertura no Archi, entrevista e fontes de impacto; aprovar PDF final; gravar pitch de até 4 min, liberar acesso e enviar |
| Physical Computing | Firmware, contrato, roteiro e 110 verificações estruturais do circuito aprovadas | Compilar/ensaiar no Wokwi, preencher resultados e URL real; gravar até 4 min e enviar link + txt |

Samuel autorizou executar as suítes e corrigir falhas em 17/11. Resultados atuais: API 134/134, desktop 19/19 e seis verificações do autoteste, Mobile 23/23 e TypeScript sem erros, ML 46 testes e 53 subtestes aprovados. Commit e merge continuam com o Cérebro. Os resultados anteriores ficam como histórico. Na consulta anterior, a `main` estava em `7a8e4e35eeb3827200c27efc085f5158898879eb` (08/11), sem os ajustes locais; as releases consultadas não continham pacotes anexos. Não houve nova consulta/publicação nesta revisão do modelo.

Provas da rodada: [API](docs/resultados-testes-2026-11-17/api-fechamento-101.trx), [desktop após o novo visual](docs/resultados-testes-2026-11-17/desktop-visual-20261117.trx), [autoteste](docs/resultados-testes-2026-11-17/desktop-autoteste-visual-20261117.txt), [Mobile](docs/resultados-testes-2026-11-17/mobile.txt), [ML](docs/resultados-testes-2026-11-17/ml-pytest-python311.txt), [modelo ArchiMate](docs/resultados-testes-2026-11-17/arquitetura-modelo.txt), [12 autotestes do modelo](docs/resultados-testes-2026-11-17/arquitetura-autoteste.txt) e [circuito](docs/resultados-testes-2026-11-17/esp32-circuito.txt). A [validação Windows](desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md) e a [fixture local após reabrir](docs/resultados-testes-2026-11-17/desktop-persistencia-local.json) documentam os ensaios do desktop. Os resultados locais não comprovam IIS/VM, Wokwi ou câmera física. O [registro do ambiente ML](docs/resultados-testes-2026-11-17/ml-pytest.txt) contém os comandos reproduzíveis; a tentativa inicial sem pytest foi preservada no histórico. A execução final em Python 3.11.9 portátil no D: terminou com código 0, em 20,62 s, sem alterar notebook, modelo ou dados.

A inspeção visual da prévia web Mobile em 17/11 aprovou a identificação e suas validações, lista de seis salas, detalhe em Frio, mapa Agora/+1 h com carregamento e seleção pela lista, descrição e foto obrigatórias no formulário, persistência da identificação após recarregar, erro com nova tentativa e lista vazia. Os três cenários terminaram sem erros de console observados. A prévia usa o mock de dois sensores e duas cadeiras; não comprova o cenário API de 20 cadeiras/quatro sensores, HTTP real, câmera, teclado ou persistência nativa.

Os PDFs de revisão preservam pendências reais: não são os PDFs finais para envio. Os vídeos, a implantação de laboratório e a submissão não são substituídos por revisão de código. Os pacotes foram atualizados para esta revisão; novas mudanças nas fontes exigem regeneração e conferência. PDFs de Cyber/TOGAF dependem da revisão do grupo e das evidências dessas disciplinas.

Artefatos desta versão; usar os manifestos atuais como referência de integridade:

- [entrega-desktop.zip](entrega-desktop.zip): projeto, integrantes e provas visuais desta versão; configuração pública gerada sem ler arquivos privados. Desktop da cópia isolada: **19/19 + seis autotestes**. [Conferência e relação das entradas](output/fechamento-samuel/CONFERENCIA-PACOTES.txt).
- [Handoff de ML/SOA/C#](output/fechamento-samuel/COMECE-AQUI.txt): instruções para Samuel/Cérebro, TXT de integrantes e [fontes/evidências atuais](output/fechamento-samuel/fontes-ml-soa-csharp.zip). API com configuração pública para testes, fixture de salas compartilhada, modelo geral, documento V1, provas SQL e PNGs. Cópia isolada: API **134/134**, ML **46 + 53 subtestes**, desktop **19 + seis autotestes**; harness SQL compilado. [Integridade](output/fechamento-samuel/SHA256SUMS.txt).
- [entrega-app.zip](entrega-app.zip): inclui os textos de previsão atualizados; Mobile **23/23** e TypeScript sem erros na cópia isolada. Dependências em D: reutilizadas somente após igualdade dos arquivos de pacote/lock; não houve nova instalação nesta rodada. [Registro atual](docs/resultados-testes-2026-11-17/pacotes-modelo-geral-v1.txt).
- [entrega-pacotes.sha256](entrega-pacotes.sha256): hashes atuais dos dois ZIPs de disciplina. Fontes sem `bin`, `obj`, dependências instaladas ou configurações privadas; configuração pública e evidências sintéticas selecionadas estão identificadas nos manifestos.
- [Gui-pendencias.zip](output/revisao-grupo/Gui-pendencias.zip): COMECE-AQUI, Cyber/OS, API e fixture atuais, provas de apoio e PDF de revisão; tarefas que exigem laboratório permanecem explícitas.
- [Lucas-pendencias.zip](output/revisao-grupo/Lucas-pendencias.zip): COMECE-AQUI, Mobile/IoT, API de apoio atual e ZIP atualizado do app. [Hashes dos pacotes do grupo](output/revisao-grupo/pacotes-grupo.sha256).
- [Mensagem para o grupo](output/revisao-grupo/MENSAGEM-CHALLENGE.txt): divisão confirmada por Samuel e pergunta sobre quem consolida TOGAF e grava/publica o pitch após a revisão conjunta. O envio no WhatsApp é registrado separadamente quando observado.
- [Cybersecurity - PDF de revisão](output/pdf/cybersecurity-revisao.pdf): relatório e anexo atualizados, 29 páginas, 596.582 bytes, com riscos e evidências de laboratório pendentes preservados.
- [TOGAF - PDF de revisão](output/pdf/arquitetura-togaf-revisao.pdf): documento e seis diagramas, 26 páginas, 530.247 bytes. As 55 páginas dos dois PDFs foram inspecionadas em folhas de contato; capas, trechos atualizados, tabelas e diagramas foram ampliados na revisão independente, sem cortes ou sobreposições observados.

Para regenerar os PDFs depois de completar as evidências, use `python ferramentas/gerar-pdfs-revisao.py --runtime "CAMINHO_DO_RUNTIME/dependencies"` em Python com as bibliotecas do runtime. O gerador mantém a identificação **de revisão**: a versão final depende da aprovação dos conteúdos pelo grupo e deve passar por nova inspeção visual.

Comandos de verificação, na raiz de uma cópia revisada (execuções autorizadas por Samuel em 17/11; para o Mobile e o ambiente Python, usar D: para as dependências):

```powershell
dotnet test .\api\TermometroDaSala.sln -c Release -m:1
dotnet test .\desktop\TermometroDaSala.Desktop.sln -c Release -m:1
dotnet run --project .\desktop\TermometroDaSala.Desktop -c Release -- --autoteste
npm --prefix .\app ci
npm --prefix .\app run typecheck
npm --prefix .\app test
python -m pytest ml -o cache_dir=ml/.pytest_cache
python arquitetura/conferir_modelo.py
python arquitetura/conferir_modelo.py --autoteste
python esp32/conferir_circuito.py
```

Esperado ao repetir: todos os comandos com código 0; API incluindo os casos de intercalação/rollback e as regressões de contrato, ajuste por sala e mapa; Mobile incluindo a lógica de retomada da permissão e a recusa de níveis herdados; ML com 46 testes e 53 subtestes aprovados no ambiente compatível. Os comandos aprovados estão ligados aos registros acima. Testes de funções não comprovam o retorno real dos Ajustes ou a câmera no aparelho. A conferência estrutural do circuito não substitui compilação e execução no Wokwi. Requisitos de Node e Python estão nos READMEs correspondentes. Guardar o resultado real, sem substituir falha por contagem histórica.

O comando abaixo regenera o ZIP com a pasta e as subpastas. Ele deixa de fora artefatos de compilação, dependências instaladas, fotos, logs e configurações privadas; o `app/assets` (ícones e splash) entra. Rode na raiz do repositório, no Windows PowerShell. Na disciplina de C#, troque `app` por `desktop` na primeira linha. Depois, atualize o hash e repita a conferência.

```powershell
$pasta = 'app'
$raiz = (Get-Location).Path
$zip = Join-Path $raiz "entrega-$pasta.zip"
if (Test-Path $zip) { Remove-Item $zip }
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$arquivo = [IO.Compression.ZipFile]::Open($zip, 'Create')
try {
  [IO.Directory]::GetFiles((Join-Path $raiz $pasta), '*', 'AllDirectories') | Where-Object {
    $_ -notmatch '\\(bin|obj|\.vs|node_modules|\.expo|dist|fotos|\.git|\.codex|\.agents)\\' -and
    [IO.Path]::GetFileName($_) -notlike '.env*' -and
    [IO.Path]::GetFileName($_) -notlike 'appsettings.*.local.json' -and
    [IO.Path]::GetFileName($_) -ne 'secrets.h' -and
    [IO.Path]::GetExtension($_) -notin @('.user', '.log')
  } | ForEach-Object {
    $nome = $_.Substring($raiz.Length + 1).Replace('\', '/')
    [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($arquivo, $_, $nome)
  }
} finally { $arquivo.Dispose() }
```

O `Compress-Archive` não serve aqui: com uma lista de arquivos ele grava tudo na raiz do ZIP, sem as pastas, e os dois `index.tsx` do app colidem. O comando acima grava cada arquivo com o caminho relativo (`app/app/salas/[id].tsx`) e com barra `/`, que abre no Windows, no macOS e no Linux.

Antes de enviar, abra o ZIP e confira que existem `app/assets/icon.png`, `app/app/salas/[id].tsx` e `app/package.json` (ou, no desktop, `desktop/integrantes.txt` e os `.csproj`).

## SOA e Web Services

**Enviar:** o link do GitHub, na tarefa de SOA.

| Item | Pontos | Onde |
|---|---:|---|
| Estruturação do domínio | 3,0 | `api/TermometroDaSala.Api/Dominio/` |
| Conexão com banco de dados | 3,0 | `api/TermometroDaSala.Api/Dados/SalaDb.cs`, `appsettings.json` e a seção de banco em `api/README.md` |
| Migrações | 2,5 | `api/TermometroDaSala.Api/Dados/Migrations/` (`Inicial`, `Enums`, `SegredosPorDispositivo`, `AlertaFrio`, `MapaTermico`) |
| Testes manuais (Insomnia) | 1,5 | `api/insomnia.json` e o roteiro em `api/README.md` |

README com execução, descrição e nomes: `api/README.md`.

- [x] Preencher os nomes completos e RMs em `api/README.md`
- [x] Exercitar SQL Server Express e cinco migrações em banco exclusivo; 98 verificações automáticas OK e ensaio HTTP/MQTT/SQL com 21 verificações aprovadas
- [x] Executar a coleção no Insomnia real: 29 requisições, 29/29 asserções e zero falhas; [captura e condições](api/evidencias/insomnia-2026-11-17/README.md)
- [ ] Publicar pelo Cérebro o código e as evidências atuais no GitHub
- [ ] Enviar o link no Teams

## Cybersecurity

**Enviar:** um PDF. A fonte é `seguranca/RELATORIO-RISCOS.md`, com o anexo `seguranca/ANEXO-EVIDENCIAS.md` (comandos em `seguranca/EVIDENCIAS.md`). Exportar pelo editor, num PDF só.

| Item | Pontos | Onde |
|---|---:|---|
| Mapeamento de ativos e ameaças | 2,5 | `seguranca/RELATORIO-RISCOS.md`, seções 3 (ativos) e 4 (ameaças) |
| Análise de vulnerabilidades e riscos | 3,0 | seções 4 (vulnerabilidades e OSINT) e 5 (matriz de risco) |
| Definição de controles | 2,5 | seção 6 |
| Evidências técnicas | 2,0 | seção 8, `seguranca/EVIDENCIAS.md` e `seguranca/evidencias/` |

- [x] Preencher os nomes completos e RMs no relatório
- [ ] Na VM: rodar `seguranca/cenarios-ataque.ps1` em banco descartável, com a chave da coordenação definida sem exibi-la; coletar nmap, IIS e GPO e preencher os prints e resultados em `seguranca/ANEXO-EVIDENCIAS.md` pelo roteiro `seguranca/EVIDENCIAS.md`
- [ ] Exportar o PDF e enviar no Teams

## C# Windows Forms

**Enviar:** o ZIP do `desktop/` (comando do topo com `$pasta = 'desktop'`, que gera o `entrega-desktop.zip`) e o `desktop/integrantes.txt` (RM, nome completo e link). O txt já entra no ZIP.

O briefing desta disciplina não divide a nota em pontos. O que a página 29 pede:

| Item | Onde |
|---|---|
| Lógica, validação e persistência | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs`, `desktop/TermometroDaSala.Desktop/Servicos/` |
| POO do domínio | `desktop/TermometroDaSala.Desktop/Dominio/` |
| Interface Windows Forms | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs` e `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.Designer.cs` |
| README (problema, pessoa real, como rodar, tecnologias, equipe) | `desktop/README.md` |
| Versionamento semântico com pull request | `desktop/CHANGELOG.md`, releases `v0.1.0`/`v0.2.0` e [PR 1](https://github.com/sbaecker2005/termometro-da-sala/pull/1) |
| Arquivo txt com RM, nome e link | `desktop/integrantes.txt` |

- [x] Preencher nomes completos, RMs e link em `desktop/integrantes.txt` e a equipe no README
- [x] Relato de entrevista e autorização para citar nome/falas confirmados por Samuel em 17/11/2026
- [x] Conferir PR e versões: PR 1 aberto (`desktop/v0.2.0` para `desktop/v0.1.0`), sem reviews/merge; releases v0.1.0 e v0.2.0 existentes sem anexos
- [ ] Publicar os ajustes atuais pelo Cérebro e conferir correspondência entre fontes publicadas e pacote; a página 29 não exige proteção de branch, aprovação/merge do PR nem vídeo de C#
- [x] Conferir visualmente a `TelaCoordenacao` no Windows: chave sem valor visível, cadastro validado, ação registrada, CSV, mapa, revogação e retorno da API; ver relatório de 17/11
- [x] Executar a suíte atual (19/19) e o autoteste do desktop (seis verificações); validar sala-99 e ação persistidas após fechar e reabrir no modo local, conforme fixture da rodada. Edição/exclusão ficam na API, no ensaio de SOA; não são comandos da interface desktop nem exigência explícita do enunciado de C#
- [x] Gerar o ZIP final e conferir numa pasta limpa: 19 testes e seis autotestes aprovados; hashes conferidos
- [ ] Enviar ZIP e txt no Teams, um integrante

Conferência do ZIP do desktop: extrair numa pasta vazia, fora do repositório, compilar e rodar o autoteste. O resultado tem que ser `0`. Rode na raiz do repositório, depois de gerar o `entrega-desktop.zip`.

```powershell
$teste = Join-Path $env:TEMP ('conferencia-desktop-' + [Guid]::NewGuid().ToString('N'))
Expand-Archive .\entrega-desktop.zip -DestinationPath $teste
$proj = Join-Path $teste 'desktop\TermometroDaSala.Desktop\TermometroDaSala.Desktop.csproj'
dotnet build $proj
dotnet run --project $proj --no-build -- --autoteste
"autoteste: $LASTEXITCODE"
Get-Content (Join-Path $teste 'desktop\integrantes.txt') -Encoding UTF8
```

Se o build falhar, ou se o autoteste não devolver `0`, ou se o `integrantes.txt` ainda tiver o marcador de preenchimento, o ZIP não vai.

## Inteligência Artificial & Machine Learning

**Enviar:** o notebook `ml/termometro_da_sala.ipynb`, o link do vídeo de até 2 min e o link do repositório, na mesma tarefa. Roteiro: `ml/ROTEIRO-VIDEO.md`.

Reexecução integral de 17/11: **19/19 células, 31,95 s, saída 0**, com Python 3.11.9, IPython.display e Matplotlib Agg, em cópia isolada no D:. Modelos JSON estruturalmente idênticos, artefatos textuais iguais após normalizar CRLF/LF e 12 originais preservados por hash. [Relatório e comando](docs/resultados-testes-2026-11-17/ml-notebook-integral.txt). O ensaio não utilizou kernel Jupyter/nbclient nem inspeção visual das figuras regeneradas; o notebook original e suas saídas permaneceram intactos.

O iniciador da apresentação também passou a [recusar uma segunda abertura com a porta ocupada](output/apresentacao/TermometroGS_Apresentacao_20261117_094533/protecao-porta.txt), preservando a API original; a falha de consulta silenciosa de porta foi corrigida com reserva TCP exclusiva. Não resta essa correção para o grupo.

O briefing pede, além do notebook, o dataset com a origem, o modelo treinado, a métrica e o README com as decisões. Esses arquivos estão no repositório, então o link vai junto. Texto para a tarefa:

```text
Notebook: termometro_da_sala.ipynb (anexo)
Vídeo (até 2 min): [preencher]
Repositório: https://github.com/sbaecker2005/termometro-da-sala
Pasta de ML (README, dataset INMET A701, modelo.json): https://github.com/sbaecker2005/termometro-da-sala/tree/main/ml
```

| Item | Pontos | Onde |
|---|---:|---|
| Problema e dados | 2,0 | `ml/README.md`, `ml/dados/inmet_a701.csv`, seção 1 do notebook |
| Preparação e análise | 2,0 | seções 2 e 3 do notebook |
| Modelo e treino | 3,0 | seção 4 do notebook e `ml/modelo.json` |
| Avaliação com métrica | 2,0 | tabela de MAE e F1 no README e no notebook |
| Integração na solução | 1,0 | `api/TermometroDaSala.Api/modelo/modelo.json`, aplicado também no Demo ao dia do CSV; mostrar no sistema rodando `GET /ambientes/sala-03/previsao` com `indiceRuaPrevisto` calculado pelo modelo. Teste `DemoAplicaOModeloAoDiaDoCsvEDaOMesmoValor` (27,16 °C, igual à seção 6). O vídeo de ML para nesse GET e no `dotnet test --filter PrevisorModeloTestes` (`ml/ROTEIRO-VIDEO.md`, meta 1 min 50 s, limite 2 min). A previsão no app e no desktop fica no pitch |

- [x] Preencher nomes completos e RMs no README, no notebook e em `ml/gerar_notebook.py` (identificação sincronizada, preservando as saídas já executadas)
- [ ] Gravar o vídeo e colar o link no texto acima
- [ ] Enviar notebook, link do vídeo e link do repositório

## Mobile e IoT

**Enviar:** o ZIP da pasta `app/` (comando do topo; não inclui `node_modules`) e o vídeo de até 2 min. Roteiro: `app/ROTEIRO-VIDEO.md`.

| Item | Peso | Onde |
|---|---:|---|
| Telas, navegação, lista (carregando, erro, vazio), formulário, identificação lembrada | 40% | `app/app/` (`index.tsx`, `salas/index.tsx`, `salas/[id].tsx`, `salas/[id]/ocorrencia.tsx`) |
| TypeScript estrito, sem `any` | 30% | `app/tsconfig.json`; conferir com `npx tsc --noEmit` |
| Recurso nativo (câmera) e permissão | 20% | `app/app/salas/[id]/ocorrencia.tsx` |
| `services/`, validação, README e DECISOES | 10% | `app/services/`, `app/domain/`, `app/README.md`, `app/DECISOES.md` |
| Array `authors` com nome e RM | porta de entrada | `app/package.json` |

- [x] Preencher os nomes completos e RMs em `app/package.json` e `app/README.md`
- [ ] Gravar o vídeo no Expo Go
- [x] Gerar o ZIP final e conferir nova extração em D: com instalação limpa, TypeScript e 23 testes aprovados
- [ ] Enviar ZIP e vídeo no Teams

## Operating Systems

**Enviar:** pelo formulário da disciplina (não pelo Teams), um integrante, até 19/11 às 23h55. Vão o link do GitHub e o do vídeo de até 7 min, e as respostas do bloco "Respostas do formulário" abaixo. Roteiro do vídeo: `infra/ROTEIRO-VIDEO.md`. A sessão bruta não é o vídeo: scripts `01` a `10` em `infra/`, com os reinícios do README e do `TESTE-DE-FALHA.md` antes do rec e o `10-coletar-evidencias.ps1` depois, para os txt. O arquivo enviado é o corte de 6:30, sem instalação e sem reinício.

| Item | Pontos | Onde |
|---|---:|---|
| Rede e identidade (IP fixo, hostname, DNS, AD) | 2 | `infra/01-rede.ps1`, `02-ad-dns.ps1`, `03-usuarios-grupos.ps1`; passos 1–3 do README |
| Aplicação no IIS (site, pool, dependências, pasta, NTFS) | 2 | `04-iis.ps1`, `05-sql.ps1`, `08-publicar-api.ps1` |
| Alertas de CPU, disco e memória | 2 | `07-monitoramento.ps1` |
| Recuperação do serviço e do pool, teste de falha | 1 | `09-teste-de-falha.ps1`, `infra/TESTE-DE-FALHA.md` |
| Logs de auditoria | 1 | `06-gpo.ps1` e a seção de auditoria do README |
| Funcionamento completo | 2 | painel em `api/TermometroDaSala.Api/wwwroot/index.html` e `GET /ambientes`, no vídeo |

- [ ] Montar a VM e rodar os scripts `01` a `10` em `infra/` conforme `infra/README.md` (reinícios antes do rec; `10-coletar-evidencias.ps1` depois do corte) e preencher `infra/EVIDENCIAS.md` com as saídas e os prints (PIDs, IP, data); não mostrar segredos
- [x] Preencher os nomes completos e RMs no README, em `infra/EVIDENCIAS.md` e no bloco do formulário
- [ ] Gravar o corte de 6:30 de `infra/ROTEIRO-VIDEO.md` (limite 7 min, sem instalação e sem reinício) e publicar o link com acesso liberado ao professor
- [ ] Trocar os campos com o marcador de preencher do bloco abaixo pelos valores reais do `infra/EVIDENCIAS.md`
- [ ] Preencher e enviar o formulário, um integrante

### Respostas do formulário

Os campos com valor fixo vêm dos scripts e do `infra/README.md`. Os que dependem da VM ou da equipe estão com o marcador de preencher e saem do `infra/EVIDENCIAS.md` depois do ensaio. Se o formulário perguntar algo que não está aqui, a resposta vem do mesmo README, e nenhuma resposta leva senha, chave da coordenação, connection string ou segredo HMAC.

| Campo | Resposta |
|---|---|
| Projeto | Termômetro da Sala |
| Equipe (nome e RM) | Samuel David Baecker - RM 559269; Lucas de Almeida Villar - RM 560005; Guilherme Augusto Caseiro - RM 559765 |
| Repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Roteiro de configuração | https://github.com/sbaecker2005/termometro-da-sala/tree/main/infra (README, scripts `01` a `10` e `TESTE-DE-FALHA.md`) |
| Evidências (prints e saídas) | https://github.com/sbaecker2005/termometro-da-sala/blob/main/infra/EVIDENCIAS.md e a pasta `infra/evidencias/` |
| Vídeo (link e duração) | [preencher] |
| Sistema e virtualização | Windows Server 2022, avaliação, Desktop Experience; versão e hipervisor: [preencher] |
| Hostname | `SRV-TERMOMETRO` |
| IP fixo do servidor e do cliente | [preencher] (o mesmo de `infra/EVIDENCIAS.md:10`) |
| Domínio do AD | `termometro.local` (NetBIOS `TERMOMETRO`), floresta nova criada pelo `02-ad-dns.ps1` |
| DNS | DNS integrado ao AD no próprio servidor; registro A `api.termometro.local` apontando para o servidor; zona reversa e PTR do servidor; cliente com DNS no servidor e ingressado no domínio |
| Comandos de diagnóstico (saídas em `infra/evidencias/`) | `hostname`, `ipconfig /all`, `nslookup api.termometro.local`, `nslookup` reverso do IP, `Test-NetConnection api.termometro.local -Port 443`, `Get-ADDomain` |
| Usuários e grupos | OU `Termometro`; grupo `Coordenacao` com `coordenador`; grupo `AdminsTermometro` com `admin.termometro` (único com RDP) |
| GPO | `Termometro-Seguranca`: senha de 12 caracteres com complexidade, histórico 24, expiração 90 dias, bloqueio após 5 erros por 15 min, bloqueio de tela em 600 s, RDP com NLA só para `AdminsTermometro`, auditoria de Logon e File System |
| File Server e DFS | Compartilhamento `Manuais` (leitura para `Coordenacao`) publicado no namespace `\\termometro.local\Escola\Manuais` |
| Acesso remoto | RDP administrativo restrito a `AdminsTermometro`, porta 3389 liberada só para o IP do cliente. RDS completo exigido pelo professor? [preencher] |
| Site no IIS | `Termometro`, pasta `C:\Sites\Termometro`, bindings HTTP 80 e HTTPS 443 com host `api.termometro.local` |
| URL da aplicação | `https://api.termometro.local/` (painel) e `https://api.termometro.local/ambientes` (lista das salas) |
| Pool de aplicativos | `TermometroPool`: No Managed Code, `ApplicationPoolIdentity`, `AlwaysRunning`, idle timeout 0, preload e Application Initialization ligados, Rapid-Fail Protection habilitado |
| Dependências | Hosting Bundle .NET 8 (AspNetCoreModuleV2), SQL Server Express 2022 (instância `SQLEXPRESS`, banco `Termometro`, migrações `Inicial`, `Enums`, `SegredosPorDispositivo`, `AlertaFrio` e `MapaTermico`) |
| Permissões NTFS | `IIS AppPool\TermometroPool` com leitura e execução em `C:\Sites\Termometro` e Modify em `C:\Dados\Termometro\Fotos`; SYSTEM e Administrators com controle total; herança desligada; usuários da escola sem acesso às fotos |
| Banco | Login `IIS APPPOOL\TermometroPool` só com `db_datareader` e `db_datawriter`; a migração é aplicada pelo administrador (`Banco__Migrar=false` no pool) |
| Monitor de Desempenho | Conjunto `Termometro-Alertas`, amostra a cada 15 s, registro no log Application |
| Alerta de CPU | `\Processor(_Total)\% Processor Time` acima de 80% |
| Alerta de disco | `\LogicalDisk(C:)\% Free Space` abaixo de 15% |
| Alerta de memória | `\Memory\Available MBytes` abaixo de 512 MB |
| Histórico de recursos | Conjunto `Termometro-Recursos`, mesmos contadores, BLG em `C:\PerfLogs\Termometro\Recursos` |
| Recuperação do serviço | W3SVC isolado em processo próprio, com reinício automático pelo Service Control Manager: 3 tentativas, 5 s entre elas, contador zerado em 24 h (`sc.exe failure` e `failureflag 1`) |
| Recuperação do pool | O WAS sobe um worker novo quando o `w3wp.exe` do `TermometroPool` cai; `AlwaysRunning` e preload sobem sem esperar requisição |
| Teste de falha | `09-teste-de-falha.ps1 -Parte Pool` mata só o worker do pool; `-Parte Servico` mata só o processo do W3SVC. Nos dois, o PID muda sozinho e só depois `/` e `/ambientes` respondem HTTP 200 |
| Resultado do teste (PIDs antes e depois, horários) | [preencher] |
| Eventos de falha e recuperação | System: WAS (worker do pool) e Service Control Manager 7031, 7034 e 7036; Application e Diagnosis-PLA: disparo dos alertas. IDs e horários observados: [preencher] |
| Logs de auditoria | Security: 4624 e 4625 (RDP permitido e negado), 4656 e 4663 (acesso à pasta de fotos) |
| Logs do IIS | `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log`, horário em UTC, com `sc-status` 200 em `/` e `/ambientes` depois da recuperação |
| Leitura de dados no vídeo | MQTT pelo `broker.hivemq.com` (saída 1883 pela NAT) ou `POST /leituras` pelo Insomnia com `Mqtt__Desligado=true`; usado: [preencher] |

## Physical Computing

**Enviar:** o link do projeto no Wokwi, o link do vídeo de até 4 min e o `esp32/integrantes.txt`. Roteiro: `esp32/ROTEIRO-VIDEO.md`. O briefing não divide a nota em pontos.

| Item | Onde |
|---|---|
| Circuito ESP32 no Wokwi | `esp32/sketch.ino`, `esp32/diagram.json`, `esp32/libraries.txt`, `esp32/secrets.exemplo.h` |
| Vídeo com a solução, não um circuito à parte | `esp32/ROTEIRO-VIDEO.md` |
| Nomes dos integrantes | `esp32/integrantes.txt` |

- [ ] Publicar o projeto no Wokwi (o link ainda não está no repositório) e não versionar `secrets.h`
- [x] Preencher os nomes completos e RMs em `esp32/integrantes.txt` e `esp32/README.md`
- [ ] Gravar o vídeo de até 4 min mostrando LCD (temperatura e nível), buzzer em Perigo e botão de reconhecimento no Wokwi, além do LED e da publicação assinada
- [ ] Enviar os dois links e o txt no Teams

## Testing (TOGAF) e pitch

**Enviar:** quatro coisas, na mesma tarefa do Teams. O pitch de até 4 min entra nesta disciplina. Roteiro do pitch, com 3min30s: `arquitetura/PITCH.md`.

1. O modelo ArchiMate: `arquitetura/termometro-da-sala.archimate`, o arquivo que abre no Archi. Ele tem seis visões: Motivação, Negócio, Aplicação, Tecnologia, Implementação e migração e "Visão em quatro camadas", que junta as quatro.
2. Um PNG por visão, exportado no Archi com a visão aberta (**File > Export > View As Image**, formato PNG): `visao-motivacao.png`, `visao-negocio.png`, `visao-aplicacao.png`, `visao-tecnologia.png` e `visao-implementacao.png`, todos em `arquitetura/`. A visão em quatro camadas já está em `arquitetura/diagrama-archi.png`. Antes de exportar, abra cada visão e confira que nenhuma linha passa por cima do nome de um elemento; se passar, arraste o elemento, salve o modelo e exporte.
3. O PDF do documento descritivo, exportado de `arquitetura/DOCUMENTO.md`, com capa (projeto, equipe com nome e RM, disciplina) e as imagens das visões em PNG. Imagem SVG costuma sumir na exportação; por isso o PDF usa os PNGs.
4. O link do pitch no YouTube, como não listado, testado numa aba anônima.

Nenhum desses quatro pode ir com o marcador de preenchimento. O `.md` não substitui o PDF.

| Item | Peso | Onde |
|---|---:|---|
| Visão da arquitetura (motivação) | 20% | visão "Motivação" no `.archimate` e `visao-motivacao.png`; `arquitetura/DOCUMENTO.md` seção 4.1 |
| Visão de negócios | 20% | visão "Negócio" e `visao-negocio.png`; seção 4.2 |
| Visão de sistemas | 20% | visão "Aplicação" e `visao-aplicacao.png`; seção 4.3, com a parte de dados |
| Visão de infraestrutura | 20% | visão "Tecnologia" e `visao-tecnologia.png`; seção 4.4 |
| Nome do projeto, equipe (nome e RM) e problema | 10% | abertura de `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md` |
| Público e estimativa de impacto, com fonte | 10% | seção 7 de `arquitetura/DOCUMENTO.md` |

- [x] Samuel confirmou a entrevista e a autorização das falas em 17/11/2026
- [ ] Revisar as fontes de impacto da seção 7; Magister e Salgueiro são candidatas ao piloto, sem autorização/execução confirmadas. Não apresentar estimativas como resultados medidos
- [x] Preencher os nomes completos e RMs em `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md` e `arquitetura/README.md`
- [ ] Abrir o `.archimate` no Archi, conferir as seis visões e exportar os PNGs com os nomes do item 2 e `diagrama-archi.png`
- [ ] Conferir a legibilidade da "Visão em quatro camadas" e gravar as seis visões abertas no Archi para os cortes do pitch
- [x] Conferir as seis imagens no `DOCUMENTO.md`, incluindo Implementação e migração na seção 9; PDF de revisão exportado e inspecionado em 17/11
- [ ] Aprovar o conteúdo final e exportar o PDF definitivo do documento com os PNGs após completar as pendências
- [ ] Cronometrar o ensaio do pitch (meta 3min30s; se passar de 3min40s, cortar na demonstração), gravar e publicar no YouTube como não listado
- [ ] Enviar `.archimate`, PNGs, PDF e link do YouTube no Teams

## Campos ainda em branco

Todo campo em aberto tem um marcador entre colchetes: a palavra preencher (RM, link, data, resultado), a palavra colar ou a palavra inserir (print), e os marcadores da entrevista (bairro da escola entrevistada, pessoa entrevistada, nome da escola, frase, nome, função, escola, número de salas e de turnos). Neste arquivo, só o texto da tarefa de ML e o bloco de respostas do formulário de OS são campos a preencher.

As linhas mudam a cada edição, por isso a tabela abaixo aponta o arquivo, e o comando dá a linha. Rode no Windows PowerShell, na raiz do repositório, antes de cada envio. Uma disciplina só é enviada quando o comando não devolve nenhuma linha dos arquivos dela.

```powershell
Get-ChildItem -Recurse -File |
  Where-Object { $_.FullName -notmatch '\\(node_modules|bin|obj|\.git|\.expo|dist|saidas)\\' -and $_.Extension -notin '.sql', '.png', '.ps1' } |
  Select-String -CaseSensitive -Pattern '\[(preencher|colar|inserir|bairro|pessoa entrevistada|nome da escola|nº de|alunos por dia|Frase|Demais|Espaço|Nome\]|função\]|escola\])' |
  ForEach-Object { '{0}:{1}  {2}' -f (Resolve-Path -Relative $_.Path), $_.LineNumber, $_.Line.Trim() }
```

| Disciplina | Arquivos | O que falta |
|---|---|---|
| Todas | `README.md` e o README de cada pasta | Unidade/bairro/autorização apenas se houver piloto futuro; a entrevista foi no CRAS Santo Amaro e está confirmada |
| SOA | `api/README.md`, `api/evidencias/README.md` | Publicar evidências/código pelo Cérebro e enviar link; índice manual Insomnia e ensaios SQL/broker concluídos |
| Cybersecurity | `seguranca/RELATORIO-RISCOS.md`, `seguranca/EVIDENCIAS.md`, `seguranca/evidencias/osint.txt` | Os 15 prints, de E01 a E15; revalidar antes da entrega a busca histórica já registrada em 05/11 em `osint.txt` |
| C# Windows Forms | `desktop/README.md`, `desktop/integrantes.txt` | Publicar ajustes e conferir pacote final; relato, PR/tags e telas já conferidos |
| Machine Learning | `ml/README.md`, `ml/gerar_notebook.py`, `ml/termometro_da_sala.ipynb` e o texto da tarefa, acima | Link do vídeo |
| Mobile e IoT | `app/package.json`, `app/README.md` | Identificação e ZIP validados; ensaio nativo e gravação no celular seguem no checklist da disciplina |
| Operating Systems | `infra/README.md`, `infra/EVIDENCIAS.md` e o bloco do formulário, acima | Data do ensaio, versão do Windows, hipervisor, IP, links do repositório e do vídeo, exigência de RDS; os prints e o que cada um anota (ACL, firewall, limites restaurados, PIDs, horários, Rapid-Fail, origem dos dados) |
| Physical Computing | `esp32/integrantes.txt`, `esp32/README.md` | Link do Wokwi e do vídeo; resultado de cada passo do roteiro de simulação |
| Testing (TOGAF) e pitch | `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md`, `arquitetura/README.md` | Revisão conjunta das fontes/estimativas de impacto e pitch; números do piloto somente quando medidos |
| Planejamento | `docs/PLANO.md` | Confirmar unidade/bairro/autorização antes de futuro piloto, sem tratar as candidatas como parceiras confirmadas |
