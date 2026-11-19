# Entrega — Termômetro da Sala

Prazo: **19/11/2026, 23h55**. Um integrante envia cada disciplina na tarefa do Teams da disciplina (enunciado, p. 34: "A entrega será via tarefas no Teams"). Repositório: https://github.com/sbaecker2005/termometro-da-sala

O briefing pede ZIP só em C# e em Mobile. Nas outras, o envio é link, PDF ou vídeo, e não há arquivo para compactar.

## Estado da entrega - 18/11/2026

**Modelo geral:** as fontes aplicam ajuste por sala ao modelo compartilhado, com o terceirão apenas na demonstração. Resultados atuais (19/11): API **138/138**, ML **48 testes e 56 subtestes**, desktop **22/22 + 6 autotestes**, Mobile **24/24** e TypeScript sem erros. [Registro técnico](docs/resultados-testes-2026-11-17/ml-modelo-geral-v1.txt). Tela e PNGs do desktop conferidos, com [evidências](desktop/docs/modelo-geral-v1/VALIDACAO.md). SQL real: **98/98** verificações da coleta e **33/33** de HTTP/MQTT/SQL, [registro](docs/resultados-testes-2026-11-17/sql-modelo-geral-v1.txt). Insomnia 29/29 permanece como evidência manual anterior.

Meta: nota 10 em todas as disciplinas. Os itens abaixo só ficam concluídos com a evidência indicada, sem promessa de nota. O enunciado oficial (PDF "GS CitizenTech - 3ES Ago - FIAP 2026") foi conferido nas páginas 27, 29 e 30 para SOA, C# e ML e na página 34 para Operating Systems. O [fechamento de ML, SOA e C#](docs/FECHAMENTO-ML-SOA-CSHARP.md) relaciona os critérios às provas.

| Disciplina | Preparado para revisar | O grupo ainda precisa concluir |
|---|---|---|
| SOA | API 138/138 (camada `IServicoAmbientes` e `Ambiente.Alterar`); cinco migrações; SQL Express 98/98 e MQTT/HTTP/SQL 33/33 nesta revisão. Insomnia 29/29 em 19/11 com um print por requisição | Publicar código e evidências no GitHub, conferir o acesso e enviar o link |
| C# | Fontes: 22/22 testes e seis autotestes (19/11, com a correção das ações de exemplo perto da meia-noite); tela e horário, mapa C01-C20/S1-S4, frio azul, PNG Agora/+1 h e rolagem conferidos na versão atual; entrevista e versões conferidas; PR mesclado e release v0.3.0 com ZIP | Enviar ZIP + TXT |
| ML | 48 testes e 56 subtestes (19/11); validação independente em 2026 (jan-out, modelo congelado, sem retreino): MAE 0,881 °C contra 1,340 °C da persistência ([ml/avaliacao-2026.json](ml/avaliacao-2026.json)); modelo geral com ajuste por sala e casos compartilhados com a API; modelos/notebook preservados. Histórico: 19/19 células reexecutadas, 31,95 s, modelos estruturalmente idênticos e integração visível no desktop | Vídeo publicado (https://youtu.be/ing8essnuOU, 1 min 48 s); enviar notebook, vídeo e repositório no Teams |
| Mobile | 24 testes e TypeScript sem erros nas fontes; ZIP do commit `5a44933` conferido em pasta limpa (19/11); prévia web conferida em 390 × 844 e 320 × 740, incluindo mapa, formulário, erro/vazio e identificação persistida após recarregar | Ensaiar Expo Go, câmera e retorno dos Ajustes no aparelho, teclado/fontes ampliadas, persistência nativa e integração HTTP real; gravar até 2 min e enviar ZIP + vídeo |
| Operating Systems | Scripts e roteiros; ensaio de 16/11 na VM; vídeo do Gui (6 min 39 s) cobre rede/AD/DNS com PTR, grupos, GPO, RDP, HTTPS, pool, SQL e migrações, alertas, tarefa no boot, falha do pool com troca de PID, auditoria, DFS e log do IIS; 15 prints em `infra/evidencias/vm-2026-11/` | Publicar o vídeo e testar o link; enviar repositório e vídeo no Teams. Opcional: saídas do `10-coletar-evidencias.ps1` (`ipconfig`, `nslookup`, `Test-NetConnection`) |
| Cybersecurity | Relatório e anexo reconciliados com o laboratório de 17 e 18/11 (cenários 23/23 contra o IIS); evidência técnica atendida pelos trechos de consulta parametrizada e contra LFI com os prints da VM (HTTPS, GPO, auditoria, RDP restrito, 4624), conforme o enunciado, que aceita Prepared Statements, sanitização contra LFI ou nmap; PDF do Gui concluído | Enviar o PDF no Teams |
| TOGAF | Modelo validado: 111 elementos, 190 relações, seis visões e 12 autotestes; seis diagramas inspecionados; PDF de 22 páginas gerado; pitch publicado em https://youtu.be/BLGYxb4P4Rc | Aprovar o PDF final e revisar as fontes de impacto da seção 7; enviar no Teams |
| Physical Computing | Firmware, contrato, roteiro e 110 verificações estruturais do circuito aprovadas | Compilar/ensaiar no Wokwi, preencher resultados e URL real; gravar até 4 min e enviar link + txt |

Resultados de 17 a 19/11: API 138/138, desktop 22/22 e seis verificações do autoteste, Mobile 24/24 e TypeScript sem erros, ML 48 testes e 56 subtestes aprovados. Os resultados anteriores ficam como histórico. As releases `v0.1.0` e `v0.2.0` do desktop não têm pacotes anexos.

Provas da rodada: [API](api/evidencias/dotnet-test-atual.txt) (138 testes; [TRX de 17/11](docs/resultados-testes-2026-11-17/api-modelo-geral-v1.trx) com 134), [desktop após o novo visual](docs/resultados-testes-2026-11-17/desktop-visual-20261117.trx), [autoteste](docs/resultados-testes-2026-11-17/desktop-autoteste-visual-20261117.txt), [Mobile](docs/resultados-testes-2026-11-17/mobile.txt), [ML](docs/resultados-testes-2026-11-17/ml-pytest-python311.txt), [modelo ArchiMate](docs/resultados-testes-2026-11-17/arquitetura-modelo.txt), [12 autotestes do modelo](docs/resultados-testes-2026-11-17/arquitetura-autoteste.txt) e [circuito](docs/resultados-testes-2026-11-17/esp32-circuito.txt). A [validação Windows](desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md) e a [fixture local após reabrir](docs/resultados-testes-2026-11-17/desktop-persistencia-local.json) documentam os ensaios do desktop. Os resultados locais não comprovam o Wokwi nem a câmera física. O [registro do ambiente ML](docs/resultados-testes-2026-11-17/ml-pytest.txt) contém os comandos reproduzíveis; a tentativa inicial sem pytest foi preservada no histórico. A execução final em Python 3.11.9 terminou com código 0, em 20,62 s, sem alterar notebook, modelo ou dados.

A inspeção visual da prévia web Mobile em 17/11 aprovou a identificação e suas validações, lista de seis salas, detalhe em Frio, mapa Agora/+1 h com carregamento e seleção pela lista, descrição e foto obrigatórias no formulário, persistência da identificação após recarregar, erro com nova tentativa e lista vazia. Os três cenários terminaram sem erros de console observados. A prévia usa o mock de quatro sensores e vinte cadeiras (a mesma sala da demonstração da API, em `app/domain/mapa.ts`); o mapa simulado acompanha o nível de cada sala. Ela não comprova HTTP real, câmera, teclado ou persistência nativa.

Os vídeos e a submissão não são substituídos por revisão de código. Os ZIPs de C# e Mobile já foram regenerados do commit final (19/11). O PDF de TOGAF depende do aceite do conteúdo pelo grupo.

Artefatos de envio (os ZIPs e os PDFs não estão versionados no repositório):

- `entrega-desktop.zip`: gerado pelo comando abaixo, com `$pasta = 'desktop'`. Configuração pública, sem arquivos privados. Desktop: **22/22 + seis autotestes**.
- `entrega-app.zip`: gerado pelo mesmo comando, com `$pasta = 'app'`. Mobile **24/24** e TypeScript sem erros.
- [entrega-pacotes.sha256](entrega-pacotes.sha256): hashes dos dois ZIPs. Os valores atuais são de uma geração anterior à v0.3.0 do desktop e ao mapa de 20 cadeiras do app; atualizar depois de gerar os ZIPs do commit final. Fontes sem `bin`, `obj`, dependências instaladas ou configurações privadas.
- Cybersecurity - PDF final: gerar pelos comandos de [`seguranca/pdf/README.md`](seguranca/pdf/README.md) (`montar.py` + pandoc/xelatex); saída `seguranca/pdf/relatorio-gestao-de-riscos.pdf`. Relatório e anexo reconciliados com o laboratório de 17 e 18/11 (cenários 23/23 no IIS), API 134/134 em 18/11, trechos de código e prints no anexo.
- TOGAF - PDF final: `arquitetura/arquitetura-togaf.pdf`, gerado em 18/11 por `arquitetura/gerar_pdf_html.py` (22 páginas, capa e as 6 visões, com o layout novo da visão de Aplicação); falta só a aprovação do grupo. `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md` foram atualizados em 18/11 com API 134/134, desktop 19/19 e ML 46 testes; em 19/11, a tela e a fala do pitch sobre a suíte da API passaram a 138 testes, e a linha datada de 18/11 ficou como está.

Para gerar PDFs de revisão do relatório e do documento de arquitetura, existe `python ferramentas/gerar-pdfs-revisao.py --runtime "CAMINHO_DO_RUNTIME/dependencies"`, em Python com as bibliotecas do runtime. O gerador mantém a identificação **de revisão**; a versão final passa por nova inspeção visual.

Comandos de verificação, na raiz de uma cópia revisada:

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

Esperado ao repetir: todos os comandos com código 0; API incluindo os casos de intercalação/rollback e as regressões de contrato, ajuste por sala e mapa; Mobile com 24 testes, incluindo a lógica de retomada da permissão e a recusa de níveis herdados; ML com 48 testes e 56 subtestes aprovados no ambiente compatível. Os comandos aprovados estão ligados aos registros acima. Testes de funções não comprovam o retorno real dos Ajustes ou a câmera no aparelho. A conferência estrutural do circuito não substitui compilação e execução no Wokwi. Requisitos de Node e Python estão nos READMEs correspondentes. Guardar o resultado real, sem substituir falha por contagem histórica.

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
- [x] Exercitar SQL Server Express e cinco migrações em banco exclusivo; coleta SQL com 98 verificações OK (`conferencia.tsv`) e ensaio HTTP/MQTT/SQL com 33 verificações aprovadas (as 21 anteriores estão incluídas nas 33)
- [x] Executar a coleção no Insomnia real: 29 requisições, 29/29 testes e zero falhas em 19/11, com a API atual e um print por requisição ([rodada de 19/11](api/evidencias/insomnia-2026-11-19/README.md); a de 17/11 fica como histórico: [captura](api/evidencias/insomnia-2026-11-17/README.md))
- [x] Publicar o código e as evidências atuais no GitHub; depois do envio, não alterar a `main` (ou criar a tag `gs-2026.2-entrega`) (publicado em 18/11: main em 0c59c4d)
- [ ] Enviar o link no Teams

## Cybersecurity

**Enviar:** um PDF, gerado por `seguranca/pdf/README.md` (relatório + anexo com trechos de código e prints). A fonte é `seguranca/RELATORIO-RISCOS.md`, com o anexo `seguranca/ANEXO-EVIDENCIAS.md` (comandos em `seguranca/EVIDENCIAS.md`).

| Item | Pontos | Onde |
|---|---:|---|
| Mapeamento de ativos e ameaças | 2,5 | `seguranca/RELATORIO-RISCOS.md`, seções 3 (ativos) e 4 (ameaças) |
| Análise de vulnerabilidades e riscos | 3,0 | seções 4 (vulnerabilidades e OSINT) e 5 (matriz de risco) |
| Definição de controles | 2,5 | seção 6 |
| Evidências técnicas | 2,0 | seção 8, `seguranca/EVIDENCIAS.md` e `seguranca/evidencias/` |

- [x] Preencher os nomes completos e RMs no relatório
- [x] Na VM: cenários S1 a S3 contra o IIS em 18/11, 23/23 (`seguranca/evidencias/cenarios-ataque-laboratorio.txt`); GPO e RDP restrito (E11, parcial)
- [x] Evidências técnicas: atendidas pelo PDF e pelo vídeo do Gui (19/11). O enunciado pede Prepared Statements, sanitização contra LFI **ou** nmap; os dois primeiros estão no PDF com os prints da VM. Nmap (E12), correlação 4663 (E11), E13 e E08 ficam como reforço opcional
- [x] Gerar o PDF por `seguranca/pdf/README.md` e enviar no Teams (18/11: gerado sem pandoc por `seguranca/pdf/gerar_pdf_html.py`, com a mesma formatação; 49 páginas com os prints do anexo; entregue no Teams em 19/11 às 20:09)

## C# Windows Forms

**Enviar:** o ZIP do `desktop/` (comando do topo com `$pasta = 'desktop'`, que gera o `entrega-desktop.zip`) e o `desktop/integrantes.txt` (RM, nome completo e link). O txt já entra no ZIP.

O briefing desta disciplina não divide a nota em pontos. O que a página 29 pede:

| Item | Onde |
|---|---|
| Lógica, validação e persistência | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs`, `desktop/TermometroDaSala.Desktop/Servicos/` |
| POO do domínio | `desktop/TermometroDaSala.Desktop/Dominio/` |
| Interface Windows Forms | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs` e `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.Designer.cs` |
| README (problema, pessoa real, como rodar, tecnologias, equipe) | `desktop/README.md` |
| Versionamento semântico com pull request | `desktop/CHANGELOG.md`, releases `v0.1.0`/`v0.2.0` (a `v0.3.0` está no changelog) e [PR 1](https://github.com/sbaecker2005/termometro-da-sala/pull/1) |
| Arquivo txt com RM, nome e link | `desktop/integrantes.txt` |

- [x] Preencher nomes completos, RMs e link em `desktop/integrantes.txt` e a equipe no README
- [x] Relato de entrevista e autorização para citar nome/falas confirmados pelo grupo em 17/11/2026
- [x] Conferir PR e versões: PR 1 aberto (`desktop/v0.2.0` para `desktop/v0.1.0`), sem reviews/merge; releases v0.1.0 e v0.2.0 existentes sem anexos
- [x] Publicar os ajustes atuais (v0.3.0): `main` com a v0.3.0; release [v0.3.0](https://github.com/sbaecker2005/termometro-da-sala/releases/tag/v0.3.0) com o ZIP conferido
- [x] PR [Desktop v0.3.0](https://github.com/sbaecker2005/termometro-da-sala/pull/2) mesclado e tag `v0.3.0` criada (19/11); a aprovação no GitHub não ficou registrada porque o autor não pode aprovar o próprio PR
- [x] Conferir visualmente a `TelaCoordenacao` no Windows: chave sem valor visível, cadastro validado, ação registrada, CSV, mapa, revogação e retorno da API; ver relatório de 17/11
- [x] Executar a suíte atual (22/22 em 19/11) e o autoteste do desktop (seis verificações); validar sala-99 e ação persistidas após fechar e reabrir no modo local, conforme fixture da rodada. Edição/exclusão ficam na API, no ensaio de SOA; não são comandos da interface desktop nem exigência explícita do enunciado de C#
- [x] Regenerar o ZIP do commit final, conferir numa pasta limpa e atualizar o hash em `entrega-pacotes.sha256` (19/11: gerado do `5a44933`, 60 entradas; extraído em pasta limpa: build sem erro, 22/22 testes e autoteste com saída 0)
- [x] Enviar ZIP e txt no Teams, um integrante (entregue em 19/11 às 20:07)

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

O briefing pede, além do notebook, o dataset com a origem, o modelo treinado, a métrica e o README com as decisões. Esses arquivos estão no repositório, então o link vai junto. Texto para a tarefa:

```text
Notebook: termometro_da_sala.ipynb (anexo)
Vídeo (até 2 min): https://youtu.be/ing8essnuOU
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
- [x] Gravar o vídeo, publicar como não listado, testar em aba anônima e colocar o link no texto acima (19/11: https://youtu.be/ing8essnuOU, 1 min 48 s, abre sem login)
- [x] Enviar notebook, link do vídeo e link do repositório (entregue em 19/11 às 20:07)

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
- [x] Regenerar o ZIP do commit final, conferir nova extração com instalação limpa, TypeScript e 24 testes e atualizar o hash em `entrega-pacotes.sha256` (19/11: gerado do `5a44933`, 40 entradas; `npm ci`, TypeScript sem erro e 24/24 testes; `authors` com os três integrantes)
- [ ] Enviar ZIP e vídeo no Teams

## Operating Systems

**Enviar:** na tarefa do Teams da disciplina (enunciado, p. 34: "A entrega será via tarefas no Teams"), um integrante, até 19/11 às 23h55. Vão o link do GitHub e o do vídeo de até 7 min; o bloco "Resumo para a entrega" abaixo reúne os dados do ambiente. Roteiro do vídeo: `infra/ROTEIRO-VIDEO.md`. A sessão bruta não é o vídeo: scripts `01` a `10` em `infra/`, com os reinícios do README e do `TESTE-DE-FALHA.md` antes do rec e o `10-coletar-evidencias.ps1` depois, para os txt. O vídeo gravado tem 6 min 39 s, sem instalação e sem reinício.

| Item | Pontos | Onde |
|---|---:|---|
| Rede e identidade (IP fixo, hostname, DNS, AD) | 2 | `infra/01-rede.ps1`, `02-ad-dns.ps1`, `03-usuarios-grupos.ps1`; passos 1–3 do README |
| Aplicação no IIS (site, pool, dependências, pasta, NTFS) | 2 | `04-iis.ps1`, `05-sql.ps1`, `08-publicar-api.ps1` |
| Alertas de CPU, disco e memória | 2 | `07-monitoramento.ps1` |
| Recuperação do serviço e do pool, teste de falha | 1 | `09-teste-de-falha.ps1`, `infra/TESTE-DE-FALHA.md` |
| Logs de auditoria | 1 | `06-gpo.ps1` e a seção de auditoria do README |
| Funcionamento completo | 2 | painel em `api/TermometroDaSala.Api/wwwroot/index.html` e `GET /ambientes`, no vídeo |

- [x] VM montada e ensaio de 16/11 com os scripts `01` a `10`, registrado em `infra/EVIDENCIAS.md` (IP, data, PIDs do teste do pool)
- [x] Evidências do ambiente: vídeo do Gui e 15 prints em `infra/evidencias/vm-2026-11/` (19/11). Opcional: rodar `10-coletar-evidencias.ps1` na VM para guardar as saídas de `ipconfig`, `nslookup` e `Test-NetConnection` em `infra/evidencias/`, sem mostrar segredos
- [x] Preencher os nomes completos e RMs no README, em `infra/EVIDENCIAS.md` e no bloco abaixo
- [x] Vídeo gravado (6 min 39 s, limite 7 min)
- [x] Publicar o vídeo com acesso liberado ao professor (19/11: publicado pelo Gui; link a registrar no bloco abaixo)
- [ ] Enviar o link do repositório e o do vídeo na tarefa do Teams, um integrante

### Resumo para a entrega

Os campos vêm dos scripts, do `infra/README.md` e do ensaio de 16/11 registrado em `infra/EVIDENCIAS.md`. O que ainda depende da equipe está marcado como PENDENTE. Nenhuma resposta leva senha, chave da coordenação, connection string ou segredo HMAC.

| Campo | Resposta |
|---|---|
| Projeto | Termômetro da Sala |
| Equipe (nome e RM) | Samuel David Baecker - RM 559269; Lucas de Almeida Villar - RM 560005; Guilherme Augusto Caseiro - RM 559765 |
| Repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Roteiro de configuração | https://github.com/sbaecker2005/termometro-da-sala/tree/main/infra (README, scripts `01` a `10` e `TESTE-DE-FALHA.md`) |
| Evidências (prints e saídas) | https://github.com/sbaecker2005/termometro-da-sala/blob/main/infra/EVIDENCIAS.md (registro do ensaio de 16/11 e do vídeo; prints da VM em `infra/evidencias/vm-2026-11/`; saídas de comando ainda não coletadas) |
| Vídeo (link e duração) | [PENDENTE: link do vídeo]; 6 min 39 s (limite 7 min) |
| Sistema e virtualização | Windows Server 2022 Standard Avaliação, Desktop Experience, build 20348; VMware Workstation Pro 26H1, rede NAT (VMnet8) |
| Hostname | `SRV-TERMOMETRO` |
| IP fixo do servidor e do cliente | Servidor `192.168.16.10/24`; cliente de laboratório = host do VMware pela VMnet8, `192.168.16.1` (`infra/EVIDENCIAS.md`) |
| Domínio do AD | `termometro.local` (NetBIOS `TERMOMETRO`), floresta nova criada pelo `02-ad-dns.ps1` |
| DNS | DNS integrado ao AD no próprio servidor; registro A `api.termometro.local` apontando para o servidor; zona reversa e PTR do servidor; cliente de laboratório (host pela VMnet8) usando o DNS do servidor; ingresso de cliente no domínio não demonstrado |
| Comandos de diagnóstico (saídas ainda não coletadas) | `hostname`, `ipconfig /all`, `nslookup api.termometro.local`, `nslookup` reverso do IP, `Test-NetConnection api.termometro.local -Port 443`, `Get-ADDomain` |
| Usuários e grupos | OU `Termometro`; grupo `Coordenacao` com `coordenador`; grupo `AdminsTermometro` com `admin.termometro` (único com RDP) |
| GPO | `Termometro-Seguranca`, ligada na OU Domain Controllers (e na raiz pelo `06-gpo.ps1`): senha de 12 caracteres com complexidade, histórico 24, expiração 90 dias, bloqueio após 5 erros por 15 min, bloqueio de tela em 600 s, RDP com NLA só para `AdminsTermometro`, auditoria de Logon e File System |
| File Server e DFS | Compartilhamento `Manuais` (leitura para `Coordenacao`) publicado no namespace `\\termometro.local\Escola\Manuais` |
| Acesso remoto | RDP administrativo restrito a `AdminsTermometro`, porta 3389 liberada só para o IP do cliente. RDP administrativo restrito demonstrado no vídeo (`admin.termometro` entra, `coordenador` é recusado por autorização); coleção RDS completa não implantada; a pergunta ao professor está sem resposta registrada |
| Site no IIS | `Termometro`, pasta `C:\Sites\Termometro`, bindings HTTP 80 e HTTPS 443 com host `api.termometro.local` |
| URL da aplicação | `https://api.termometro.local/` (painel) e `https://api.termometro.local/ambientes` (lista das salas) |
| Pool de aplicativos | `TermometroPool`: No Managed Code, `ApplicationPoolIdentity`, `AlwaysRunning`, idle timeout 0, preload e Application Initialization ligados, Rapid-Fail Protection habilitado |
| Dependências | Hosting Bundle .NET 8 (AspNetCoreModuleV2), SQL Server Express 2022 (instância `SQLEXPRESS`, serviço na conta virtual `NT SERVICE\MSSQL$SQLEXPRESS`, banco `Termometro`, migrações `Inicial`, `Enums`, `SegredosPorDispositivo`, `AlertaFrio` e `MapaTermico`) |
| Permissões NTFS | `IIS AppPool\TermometroPool` com leitura e execução em `C:\Sites\Termometro` e Modify em `C:\Dados\Termometro\Fotos`; SYSTEM e Administrators com controle total; herança desligada; usuários da escola sem acesso às fotos |
| Banco | Login `IIS APPPOOL\TermometroPool` só com `db_datareader` e `db_datawriter`; a migração é aplicada pelo administrador (`Banco__Migrar=false` no pool) |
| Monitor de Desempenho | Conjunto `Termometro-Alertas`, amostra a cada 15 s, registro no log Application |
| Alerta de CPU | `\Processor(_Total)\% Processor Time` acima de 80% |
| Alerta de disco | `\LogicalDisk(C:)\% Free Space` abaixo de 15% |
| Alerta de memória | `\Memory\Available MBytes` abaixo de 512 MB |
| Histórico de recursos | Conjunto `Termometro-Recursos`, mesmos contadores, BLG em `C:\PerfLogs\Termometro\Recursos` |
| Recuperação do serviço | W3SVC com política de recuperação do SCM configurada (3 reinícios, 5 s, contador zerado em 24 h; `sc.exe failure` e `failureflag 1`). O isolamento em processo próprio (`type= own`) foi tentado e o serviço não iniciou nesta build; revertido para compartilhado |
| Recuperação do pool | O WAS sobe um worker novo quando o `w3wp.exe` do `TermometroPool` cai; `AlwaysRunning` e preload sobem sem esperar requisição |
| Teste de falha | `09-teste-de-falha.ps1 -Parte Pool` mata só o worker do pool; o PID muda sozinho e depois `/` e `/ambientes` respondem 200 (demonstrado no vídeo, 04:12 a 04:45). `-Parte Servico` não foi executada (limitação documentada em `infra/EVIDENCIAS.md`, E11) |
| Resultado do teste (PIDs antes e depois, horários) | Pool: PID 7336 antes, 7332 depois, pool `Started`, HTTP 200 em `/` e `/ambientes`, 16/11/2026 por volta das 22h40 (America/Sao_Paulo). Serviço: não executado (limitação) |
| Eventos de falha e recuperação | System: WAS (worker do pool) e Service Control Manager 7031, 7034 e 7036; Application e Diagnosis-PLA: disparo dos alertas. Observado em 16/11/2026: System/Service Control Manager com 137 eventos 7031 (reinício em 5000 ms), 7024 e 7001, entre 22:25:48 e 22:34:39, causados pela tentativa de isolar o W3SVC; W3SVC voltou a Running às 22:35:36. Eventos WAS do pool e disparo de alertas: não coletados |
| Logs de auditoria | Security: 4624 e 4625 (RDP permitido e negado), 4656 e 4663 (acesso à pasta de fotos) |
| Logs do IIS | `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log`, horário em UTC, com `sc-status` 200 em `/` e `/ambientes` depois da recuperação |
| Leitura de dados no vídeo | MQTT pelo `broker.hivemq.com` (saída 1883 pela NAT) ou `POST /leituras` pelo Insomnia com `Mqtt__Desligado=true`; usado: [PENDENTE: confirmar no vídeo qual das duas] |

## Physical Computing

**Enviar:** o link do projeto no Wokwi, o link do vídeo de até 4 min e o `esp32/integrantes.txt`. Roteiro: `esp32/ROTEIRO-VIDEO.md`. O briefing não divide a nota em pontos.

| Item | Onde |
|---|---|
| Circuito ESP32 no Wokwi | `esp32/sketch.ino`, `esp32/diagram.json`, `esp32/libraries.txt`, `esp32/secrets.exemplo.h` |
| Vídeo com a solução, não um circuito à parte | `esp32/ROTEIRO-VIDEO.md` |
| Nomes dos integrantes | `esp32/integrantes.txt` |

- [x] Publicar o projeto no Wokwi e não versionar `secrets.h` (18/11: https://wokwi.com/projects/481082666157493249, público, com o segredo de demonstração)
- [x] Preencher os nomes completos e RMs em `esp32/integrantes.txt` e `esp32/README.md`
- [ ] Gravar o vídeo de até 4 min mostrando LCD (temperatura e nível), buzzer em Perigo e botão de reconhecimento no Wokwi, além do LED e da publicação assinada
- [ ] Enviar os dois links e o txt no Teams

## Testing (TOGAF) e pitch

**Enviar:** quatro coisas, na mesma tarefa do Teams. O pitch de até 4 min entra nesta disciplina. Roteiro do pitch, com 3min30s: `arquitetura/PITCH.md`; slides (8, em PDF): `arquitetura/slides-pitch.pdf`; roteiro slide por slide para gravar: `arquitetura/ROTEIRO-PITCH-SLIDES.md`.

1. O modelo ArchiMate: `arquitetura/termometro-da-sala.archimate`, o arquivo que abre no Archi. Ele tem seis visões: Motivação, Negócio, Aplicação, Tecnologia, Implementação e migração e "Visão em quatro camadas", que junta as quatro.
2. Um PNG por visão, exportado no Archi com a visão aberta (**File > Export > View As Image**, formato PNG): `visao-motivacao.png`, `visao-negocio.png`, `visao-aplicacao.png`, `visao-tecnologia.png` e `visao-implementacao.png`, todos em `arquitetura/`. A visão em quatro camadas já está em `arquitetura/diagrama-archi.png`. Antes de exportar, abra cada visão e confira que nenhuma linha passa por cima do nome de um elemento; se passar, arraste o elemento, salve o modelo e exporte.
3. O PDF do documento descritivo, exportado de `arquitetura/DOCUMENTO.md`, com capa (projeto, equipe com nome e RM, disciplina) e as imagens das visões em PNG. Imagem SVG costuma sumir na exportação; por isso o PDF usa os PNGs.
4. O link do pitch no YouTube: https://youtu.be/BLGYxb4P4Rc (aberto sem login em 19/11).

Nenhum desses quatro pode ir com o marcador de preenchimento. O `.md` não substitui o PDF.

| Item | Peso | Onde |
|---|---:|---|
| Visão da arquitetura (motivação) | 20% | visão "Motivação" no `.archimate` e `visao-motivacao.png`; `arquitetura/DOCUMENTO.md` seção 4.1 |
| Visão de negócios | 20% | visão "Negócio" e `visao-negocio.png`; seção 4.2 |
| Visão de sistemas | 20% | visão "Aplicação" e `visao-aplicacao.png`; seção 4.3, com a parte de dados |
| Visão de infraestrutura | 20% | visão "Tecnologia" e `visao-tecnologia.png`; seção 4.4 |
| Nome do projeto, equipe (nome e RM) e problema | 10% | abertura de `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md` |
| Público e estimativa de impacto, com fonte | 10% | seção 7 de `arquitetura/DOCUMENTO.md` |

- [x] Entrevista e autorização das falas confirmadas pelo grupo em 17/11/2026
- [ ] Revisar as fontes de impacto da seção 7; o piloto em escola ainda não tem unidade definida. Não apresentar estimativas como resultados medidos
- [x] Preencher os nomes completos e RMs em `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md` e `arquitetura/README.md`
- [x] Abrir o `.archimate` no Archi, conferir as seis visões e exportar os PNGs com os nomes do item 2 e `diagrama-archi.png` (18/11: as seis visões reexportadas pela linha de comando do Archi saíram iguais às do repositório; falta só conferir a legibilidade da visão em quatro camadas)
- [ ] Conferir a legibilidade da "Visão em quatro camadas" e gravar as seis visões abertas no Archi para os cortes do pitch
- [x] Conferir as seis imagens no `DOCUMENTO.md`, incluindo Implementação e migração na seção 9; PDF de revisão exportado e inspecionado em 17/11
- [ ] Aprovar o conteúdo final e exportar o PDF definitivo do documento com os PNGs após completar as pendências (18/11: `arquitetura/arquitetura-togaf.pdf` gerado por `arquitetura/gerar_pdf_html.py`, 22 páginas com as seis visões e o layout novo da Aplicação, em que nenhuma linha passa sobre o nome de um elemento; falta a aprovação do grupo)
- [x] Gravar e publicar o pitch no YouTube: https://youtu.be/BLGYxb4P4Rc ("GS Testing", abre sem login)
- [x] Enviar `.archimate`, PNGs, PDF e link do YouTube no Teams (entregue em 19/11 às 20:11)

## Pendências da equipe

Itens que dependem de gravação, publicação ou coleta no laboratório e que não podem ser preenchidos pelo texto do repositório:

- [x] Vídeo de ML publicado: https://youtu.be/ing8essnuOU (1 min 48 s)
- [ ] Vídeo de Mobile (até 2 min, Expo Go) e ensaio no aparelho (câmera, Ajustes, teclado)
- [x] Vídeo de Operating Systems (6 min 39 s): publicado pelo Gui em 19/11
- [ ] Vídeo de Physical Computing (até 4 min); projeto no Wokwi já publicado em https://wokwi.com/projects/481082666157493249
- [x] Pitch de TOGAF publicado: https://youtu.be/BLGYxb4P4Rc
- [x] Evidências de OS e Cybersecurity: cobertas pelo vídeo e pelo PDF do Gui (19/11); as saídas do coletor de OS e o nmap ficam opcionais
- [x] PNGs das visões no Archi e PDF final de TOGAF; PDF final de Cybersecurity (18/11: PNGs conferidos e os dois PDFs gerados; TOGAF aguarda a aprovação do grupo; falta o envio)
- [x] `entrega-desktop.zip` e `entrega-app.zip` do commit final, com `entrega-pacotes.sha256` atualizado (regenerados em 19/11)
- [x] Publicar o código no GitHub e conferir o acesso; depois do envio, não alterar a `main` (ou criar a tag `gs-2026.2-entrega`) (18/11: main em 0c59c4d; falta só não alterar a `main` depois do envio, ou criar a tag)

## Campos ainda em branco

Todo campo em aberto tem um marcador entre colchetes: a palavra PENDENTE, a palavra preencher (RM, link, data, resultado), a palavra colar ou a palavra inserir (print), e os marcadores da entrevista (bairro da escola entrevistada, pessoa entrevistada, nome da escola, frase, nome, função, escola, número de salas e de turnos). Neste arquivo, só o texto da tarefa de ML e o bloco de resumo de OS têm campos a preencher.

As linhas mudam a cada edição, por isso a tabela abaixo aponta o arquivo, e o comando dá a linha. Rode no Windows PowerShell, na raiz do repositório, antes de cada envio. Uma disciplina só é enviada quando o comando não devolve nenhuma linha dos arquivos dela.

```powershell
Get-ChildItem -Recurse -File |
  Where-Object { $_.FullName -notmatch '\\(node_modules|bin|obj|\.git|\.expo|dist|saidas)\\' -and $_.Extension -notin '.sql', '.png', '.ps1' } |
  Select-String -CaseSensitive -Pattern '\[(PENDENTE|preencher|colar|inserir|bairro|pessoa entrevistada|nome da escola|nº de|alunos por dia|Frase|Demais|Espaço|Nome\]|função\]|escola\])' |
  ForEach-Object { '{0}:{1}  {2}' -f (Resolve-Path -Relative $_.Path), $_.LineNumber, $_.Line.Trim() }
```

| Disciplina | Arquivos | O que falta |
|---|---|---|
| Todas | `README.md` e o README de cada pasta | Unidade/bairro/autorização apenas se houver piloto futuro; a entrevista foi no CRAS Santo Amaro e está confirmada |
| SOA | `api/README.md`, `api/evidencias/README.md` | Publicar evidências e código e enviar o link; índice manual Insomnia e ensaios SQL/broker concluídos |
| Cybersecurity | `seguranca/RELATORIO-RISCOS.md`, `seguranca/EVIDENCIAS.md`, `seguranca/evidencias/osint.txt` | Prints e saídas de E08, E11, E12 e E13; revalidar antes da entrega a busca histórica já registrada em 05/11 em `osint.txt` |
| C# Windows Forms | `desktop/README.md`, `desktop/integrantes.txt` | Publicar ajustes (v0.3.0) e conferir pacote final; relato, PR/tags e telas já conferidos |
| Machine Learning | `ml/README.md`, `ml/gerar_notebook.py`, `ml/termometro_da_sala.ipynb` e o texto da tarefa, acima | Link do vídeo |
| Mobile e IoT | `app/package.json`, `app/README.md` | Identificação validada; regenerar o ZIP; ensaio nativo e gravação no celular seguem no checklist da disciplina |
| Operating Systems | `infra/README.md`, `infra/EVIDENCIAS.md` e o bloco de resumo, acima | Link do vídeo e resposta do professor sobre RDS; as saídas e os prints de cada item (ACL, firewall, limites restaurados, PIDs, horários, Rapid-Fail, origem dos dados) |
| Physical Computing | `esp32/integrantes.txt`, `esp32/README.md` | Link do Wokwi e do vídeo; resultado de cada passo do roteiro de simulação |
| Testing (TOGAF) e pitch | `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md`, `arquitetura/README.md` | Revisão das fontes/estimativas de impacto e pitch; números do piloto somente quando medidos |
| Planejamento | `docs/PLANO.md` | Definir unidade/bairro/autorização antes de um piloto futuro, sem tratar nenhuma escola como parceira confirmada |
