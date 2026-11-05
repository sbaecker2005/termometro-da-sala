# Entrega — Termômetro da Sala

Prazo: **19/11/2026, 23h55**. Um integrante envia cada disciplina no Teams, menos Operating Systems, que vai por formulário. Repositório: https://github.com/sbaecker2005/termometro-da-sala

O briefing pede ZIP só em C# e em Mobile. Nas outras, o envio é link, PDF, vídeo ou formulário, e não há arquivo para compactar.

O comando abaixo gera o ZIP com a pasta e as subpastas. Ele deixa de fora só `bin`, `obj`, `.vs`, `node_modules`, `.expo`, `dist`, a pasta `fotos`, os arquivos `.env*` e o `*.user`; o `app/assets` (ícones e splash) entra. Rode na raiz do repositório, no Windows PowerShell. Na disciplina de C#, troque `app` por `desktop` na primeira linha.

```powershell
$pasta = 'app'
$raiz = (Get-Location).Path
$zip = Join-Path $raiz "entrega-$pasta.zip"
if (Test-Path $zip) { Remove-Item $zip }
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$arquivo = [IO.Compression.ZipFile]::Open($zip, 'Create')
try {
  [IO.Directory]::GetFiles((Join-Path $raiz $pasta), '*', 'AllDirectories') | Where-Object {
    $_ -notmatch '\\(bin|obj|\.vs|node_modules|\.expo|dist|fotos)\\' -and
    [IO.Path]::GetFileName($_) -notlike '.env*' -and
    [IO.Path]::GetExtension($_) -ne '.user'
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
| Migrações | 2,5 | `api/TermometroDaSala.Api/Dados/Migrations/` (`Inicial`, `Enums`, `SegredosPorDispositivo`) |
| Testes manuais (Insomnia) | 1,5 | `api/insomnia.json` e o roteiro em `api/README.md` |

README com execução, descrição e nomes: `api/README.md`.

- [ ] Preencher o RM em `api/README.md`
- [ ] Rodar o banco e a coleção do Insomnia e colar cada print no campo dele em `api/evidencias/README.md` (a pasta já tem `dotnet-test.txt` e o script das migrações)
- [ ] Enviar o link no Teams

## Cybersecurity

**Enviar:** um PDF. A fonte é `seguranca/RELATORIO-RISCOS.md`, com o anexo `seguranca/ANEXO-EVIDENCIAS.md` (comandos em `seguranca/EVIDENCIAS.md`). Exportar pelo editor, num PDF só.

| Item | Pontos | Onde |
|---|---:|---|
| Mapeamento de ativos e ameaças | 2,5 | `seguranca/RELATORIO-RISCOS.md`, seções 3 (ativos) e 4 (ameaças) |
| Análise de vulnerabilidades e riscos | 3,0 | seções 4 (vulnerabilidades e OSINT) e 5 (matriz de risco) |
| Definição de controles | 2,5 | seção 6 |
| Evidências técnicas | 2,0 | seção 8, `seguranca/EVIDENCIAS.md` e `seguranca/evidencias/` |

- [ ] Preencher o RM no relatório
- [ ] Na VM: rodar `seguranca/cenarios-ataque.ps1` em banco descartável, com a chave da coordenação definida sem exibi-la; coletar nmap, IIS e GPO e preencher os prints e resultados em `seguranca/ANEXO-EVIDENCIAS.md` pelo roteiro `seguranca/EVIDENCIAS.md`
- [ ] Exportar o PDF e enviar no Teams

## C# Windows Forms

**Enviar:** o ZIP do `desktop/` (comando do topo com `$pasta = 'desktop'`, que gera o `entrega-desktop.zip`) e o `desktop/integrantes.txt` (RM, nome completo e link). O txt já entra no ZIP.

O briefing desta disciplina não divide a nota em pontos. O que a página 29 pede:

| Item | Onde |
|---|---|
| Lógica, validação e persistência | `desktop/TermometroDaSala.Desktop/Form1.cs`, `Servicos.cs` |
| POO do domínio | `desktop/TermometroDaSala.Desktop/Dominio.cs` |
| Interface Windows Forms | `Form1.cs`, `Form1.Designer.cs` e `TelaCoordenacao.cs` |
| README (problema, pessoa real, como rodar, tecnologias, equipe) | `desktop/README.md` |
| Versionamento semântico, tags e changelog | `desktop/CHANGELOG.md` (`v0.1.0`) |
| Arquivo txt com RM, nome e link | `desktop/integrantes.txt` |

- [ ] Preencher RM e link em `desktop/integrantes.txt` e o RM no README
- [ ] Fazer a entrevista e escrever nome, escola e data no README
- [ ] Abrir o pull request da versão e criar as tags no GitHub (`v0.1.0` em diante)
- [ ] Gravar ou printar a `TelaCoordenacao`: chave sem valor visível, cadastro/edição/exclusão de sala, ocorrências e revogação; conferir os 6 testes do desktop
- [ ] Gerar o ZIP e conferir numa pasta limpa (comandos abaixo)
- [ ] Enviar ZIP e txt no Teams, um integrante

Conferência do ZIP do desktop: extrair numa pasta vazia, fora do repositório, compilar e rodar o autoteste. O resultado tem que ser `0`. Rode na raiz do repositório, depois de gerar o `entrega-desktop.zip`.

```powershell
$teste = Join-Path $env:TEMP 'conferencia-desktop'
if (Test-Path $teste) { Remove-Item $teste -Recurse -Force }
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
| Integração na solução | 1,0 | `api/TermometroDaSala.Api/modelo/modelo.json`, aplicado também no Demo ao dia do CSV; mostrar no sistema rodando `GET /ambientes/sala-03/previsao` com `indiceRuaPrevisto` calculado pelo modelo. Teste `DemoAplicaOModeloAoDiaDoCsvEDaOMesmoValor` (27,16 °C, igual à seção 6); previsão no app e no desktop |

- [ ] Preencher o RM no README, no notebook e em `ml/gerar_notebook.py` (gerar o notebook de novo depois, para os dois não divergirem)
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

- [ ] Preencher o RM em `app/package.json`
- [ ] Gravar o vídeo no Expo Go
- [ ] Gerar o ZIP e enviar ZIP e vídeo no Teams

## Operating Systems

**Enviar:** pelo formulário da disciplina (não pelo Teams), um integrante, até 19/11 às 23h55. Vão o link do GitHub e o do vídeo de até 7 min, e as respostas do bloco "Respostas do formulário" abaixo. Roteiro do vídeo: `infra/ROTEIRO-VIDEO.md`.

| Item | Pontos | Onde |
|---|---:|---|
| Rede e identidade (IP fixo, hostname, DNS, AD) | 2 | `infra/01-rede.ps1`, `02-ad-dns.ps1`, `03-usuarios-grupos.ps1`; passos 1–3 do README |
| Aplicação no IIS (site, pool, dependências, pasta, NTFS) | 2 | `04-iis.ps1`, `05-sql.ps1`, `08-publicar-api.ps1` |
| Alertas de CPU, disco e memória | 2 | `07-monitoramento.ps1` |
| Recuperação do serviço e do pool, teste de falha | 1 | `09-teste-de-falha.ps1`, `infra/TESTE-DE-FALHA.md` |
| Logs de auditoria | 1 | `06-gpo.ps1` e a seção de auditoria do README |
| Funcionamento completo | 2 | painel em `api/TermometroDaSala.Api/wwwroot/index.html` e `GET /ambientes`, no vídeo |

- [ ] Montar a VM, executar `infra/10-coletar-evidencias.ps1` conforme `infra/README.md` e preencher `infra/EVIDENCIAS.md` com as saídas e os prints (PIDs, IP, data); não mostrar segredos
- [ ] Preencher a equipe e os RMs no README e em `infra/EVIDENCIAS.md`
- [ ] Gravar o vídeo de até 7 min e publicar o link com acesso liberado ao professor
- [ ] Trocar os campos com o marcador de preencher do bloco abaixo pelos valores reais do `infra/EVIDENCIAS.md`
- [ ] Preencher e enviar o formulário, um integrante

### Respostas do formulário

Os campos com valor fixo vêm dos scripts e do `infra/README.md`. Os que dependem da VM ou da equipe estão com o marcador de preencher e saem do `infra/EVIDENCIAS.md` depois do ensaio. Se o formulário perguntar algo que não está aqui, a resposta vem do mesmo README, e nenhuma resposta leva senha, chave da coordenação, connection string ou segredo HMAC.

| Campo | Resposta |
|---|---|
| Projeto | Termômetro da Sala |
| Equipe (nome e RM) | [preencher] |
| Repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Roteiro de configuração | https://github.com/sbaecker2005/termometro-da-sala/tree/main/infra (README, scripts `01` a `10` e `TESTE-DE-FALHA.md`) |
| Evidências (prints e saídas) | https://github.com/sbaecker2005/termometro-da-sala/blob/main/infra/EVIDENCIAS.md e a pasta `infra/evidencias/` |
| Vídeo (link e duração) | [preencher] |
| Sistema e virtualização | Windows Server 2022, avaliação, Desktop Experience; versão e hipervisor: [preencher] |
| Hostname | `SRV-TERMOMETRO` |
| IP fixo do servidor e do cliente | [preencher] (o mesmo de `infra/EVIDENCIAS.md:8`) |
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
| Dependências | Hosting Bundle .NET 8 (AspNetCoreModuleV2), SQL Server Express 2022 (instância `SQLEXPRESS`, banco `Termometro`, migrações `Inicial`, `Enums` e `SegredosPorDispositivo`) |
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
- [ ] Preencher o RM em `esp32/integrantes.txt`
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

- [ ] Fazer a entrevista e colocar a fala no pitch e o piloto da escola na seção 7
- [ ] Preencher os RMs em `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md`
- [ ] Abrir o `.archimate` no Archi, conferir as seis visões e exportar os PNGs com os nomes do item 2 e `diagrama-archi.png`
- [ ] Conferir a legibilidade da "Visão em quatro camadas" e gravar as seis visões abertas no Archi para os cortes do pitch
- [ ] Conferir as seis imagens no `DOCUMENTO.md`, incluindo Implementação e migração na seção 9
- [ ] Exportar o PDF do documento com os PNGs
- [ ] Cronometrar o ensaio do pitch (meta 3min30s; se passar de 3min40s, cortar na demonstração), gravar e publicar no YouTube como não listado
- [ ] Enviar `.archimate`, PNGs, PDF e link do YouTube no Teams

## Campos ainda em branco

Todo campo em aberto tem um marcador entre colchetes: a palavra preencher (RM, link, data, resultado), a palavra colar ou a palavra inserir (print), e os marcadores da entrevista (bairro da escola entrevistada, pessoa entrevistada, nome da escola, frase, nome, função, escola, número de salas e de turnos). Neste arquivo, só o texto da tarefa de ML e o bloco de respostas do formulário de OS são campos a preencher.

As linhas mudam a cada edição, por isso a tabela abaixo aponta o arquivo, e o comando dá a linha. Rode no Windows PowerShell, na raiz do repositório, antes de cada envio. Uma disciplina só é enviada quando o comando não devolve nenhuma linha dos arquivos dela.

```powershell
Get-ChildItem -Recurse -File |
  Where-Object { $_.FullName -notmatch '\\(node_modules|bin|obj|\.git|\.expo|dist)\\' -and $_.Extension -notin '.sql', '.png' } |
  Select-String -CaseSensitive -Pattern '\[(preencher|colar|inserir|bairro|pessoa entrevistada|nome da escola|nº de|alunos por dia|Frase|Demais|Espaço|Nome\]|função\]|escola\])' |
  ForEach-Object { '{0}:{1}  {2}' -f (Resolve-Path -Relative $_.Path), $_.LineNumber, $_.Line.Trim() }
```

| Disciplina | Arquivos | O que falta |
|---|---|---|
| Todas | `README.md` e o README de cada pasta | Bairro da escola entrevistada, na ficha |
| SOA | `api/README.md`, `api/evidencias/README.md` | RM; prints do banco (B1 a B5), do Insomnia (01 a 27) e das telas (T1 e T2) |
| Cybersecurity | `seguranca/RELATORIO-RISCOS.md`, `seguranca/EVIDENCIAS.md`, `seguranca/evidencias/osint.txt` | RMs dos integrantes; os 15 prints, de E01 a E15; a saída e a data da busca no histórico do repositório, em `osint.txt` |
| C# Windows Forms | `desktop/README.md`, `desktop/integrantes.txt` | RM e demais integrantes; pessoa entrevistada e relato; prints das telas; link do pull request |
| Machine Learning | `ml/README.md`, `ml/gerar_notebook.py`, `ml/termometro_da_sala.ipynb` e o texto da tarefa, acima | RM (gerar o notebook de novo depois); link do vídeo |
| Mobile e IoT | `app/package.json`, `app/README.md` | RM e demais integrantes |
| Operating Systems | `infra/README.md`, `infra/EVIDENCIAS.md` e o bloco do formulário, acima | Equipe e RMs; data do ensaio, versão do Windows, hipervisor, IP, links do repositório e do vídeo, exigência de RDS; os prints e o que cada um anota (ACL, firewall, limites restaurados, PIDs, horários, Rapid-Fail, origem dos dados) |
| Physical Computing | `esp32/integrantes.txt`, `esp32/README.md` | Nomes completos e RMs; link do Wokwi e do vídeo; resultado de cada passo do roteiro de simulação |
| Testing (TOGAF) e pitch | `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md`, `arquitetura/README.md` | RMs; as seis imagens das visões; salas, turnos e alunos por dia da escola do piloto; data de acesso das referências 7 e 8; fala, frase, nome, função e escola da pessoa entrevistada; demais integrantes no slide 1 |
| Planejamento | `docs/PLANO.md` | Bairro da escola entrevistada |
