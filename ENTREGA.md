# Entrega — Termômetro da Sala

Prazo: **19/11/2026, 23h55**. Um integrante envia cada disciplina no Teams. Repositório: https://github.com/sbaecker2005/termometro-da-sala

O briefing pede ZIP só em C# e em Mobile. Nas outras, o envio é link, PDF ou vídeo, e não há arquivo para compactar.

O comando abaixo monta a lista do que entra no ZIP. `Compress-Archive` não tira pasta aninhada com `-Exclude`. Rode na raiz do repositório, no Windows PowerShell. Troque `app` e `entrega-app.zip` por `desktop` e `entrega-desktop.zip` na disciplina de C#.

```powershell
$raiz = (Get-Location).Path
$pasta = Join-Path $raiz 'app'
$incluir = [IO.Directory]::GetFiles($pasta, '*', 'AllDirectories') | Where-Object {
  $_ -notmatch '\\(bin|obj|node_modules|fotos)\\' -and
  [IO.Path]::GetExtension($_) -notin '.user','.jpg','.jpeg','.png','.gif','.webp','.bmp','.heic'
} | ForEach-Object { [WildcardPattern]::Escape($_.Substring($raiz.Length + 1)) }
if (Test-Path entrega-app.zip) { Remove-Item entrega-app.zip }
Compress-Archive -Path $incluir -DestinationPath entrega-app.zip
```

O escape existe porque a pasta `app/app/salas/[id]` faria o `-Path` tratar colchetes como curinga.

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
- [ ] Rodar a coleção do Insomnia e guardar os prints (a pasta `api/evidencias/` ainda não existe)
- [ ] Enviar o link no Teams

## Cybersecurity

**Enviar:** um PDF. A fonte é `seguranca/RELATORIO-RISCOS.md`, com o anexo `seguranca/EVIDENCIAS.md`. Exportar pelo editor, num PDF só.

| Item | Pontos | Onde |
|---|---:|---|
| Mapeamento de ativos e ameaças | 2,5 | `seguranca/RELATORIO-RISCOS.md`, seções 2 e 3 |
| Análise de vulnerabilidades e riscos | 3,0 | seções 3 e 4 |
| Definição de controles | 2,5 | seção 5 |
| Evidências técnicas | 2,0 | seção 5, `seguranca/EVIDENCIAS.md` e `seguranca/evidencias/` |

- [ ] Preencher o RM no relatório
- [ ] Na VM: nmap, IIS, GPO e os prints que o roteiro marca como pendentes
- [ ] Exportar o PDF e enviar no Teams

## C# Windows Forms

**Enviar:** o ZIP do `desktop/` (comando acima, com `desktop` e `entrega-desktop.zip`) e o `desktop/integrantes.txt` (RM, nome completo e link). O txt já entra no ZIP.

O briefing desta disciplina não divide a nota em pontos. O que a página 29 pede:

| Item | Onde |
|---|---|
| Lógica, validação e persistência | `desktop/TermometroDaSala.Desktop/Form1.cs`, `Servicos.cs` |
| POO do domínio | `desktop/TermometroDaSala.Desktop/Dominio.cs` |
| Interface Windows Forms | `Form1.cs`, `Form1.Designer.cs` |
| README (problema, pessoa real, como rodar, tecnologias, equipe) | `desktop/README.md` |
| Versionamento semântico, tags e changelog | `desktop/CHANGELOG.md` (`v0.1.0`) |
| Arquivo txt com RM, nome e link | `desktop/integrantes.txt` |

- [ ] Preencher RM e link em `desktop/integrantes.txt` e o RM no README
- [ ] Fazer a entrevista e escrever nome, escola e data no README
- [ ] Abrir o pull request da versão e criar as tags no GitHub (`v0.1.0` em diante)
- [ ] Gerar o ZIP e enviar no Teams, um integrante

## IA e Machine Learning

**Enviar:** o notebook `ml/termometro_da_sala.ipynb` e o link do vídeo de até 2 min. Roteiro: `ml/ROTEIRO-VIDEO.md`.

| Item | Pontos | Onde |
|---|---:|---|
| Problema e dados | 2,0 | `ml/README.md`, `ml/dados/inmet_a701.csv`, seção 1 do notebook |
| Preparação e análise | 2,0 | seções 2 e 3 do notebook |
| Modelo e treino | 3,0 | seção 4 do notebook e `ml/modelo.json` |
| Avaliação com métrica | 2,0 | tabela de MAE e F1 no README e no notebook |
| Integração na solução | 1,0 | `api/TermometroDaSala.Api/modelo/modelo.json`, previsão no app e no desktop |

- [ ] Preencher o RM no README, no notebook e em `ml/gerar_notebook.py` (gerar o notebook de novo depois, para os dois não divergirem)
- [ ] Gravar o vídeo e colar o link no Teams
- [ ] Enviar notebook e link

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

**Enviar:** o link do GitHub e o vídeo de até 7 min. Roteiro: `infra/ROTEIRO-VIDEO.md`.

| Item | Pontos | Onde |
|---|---:|---|
| Rede e identidade (IP fixo, hostname, DNS, AD) | 2 | `infra/01-rede.ps1`, `02-ad-dns.ps1`, `03-usuarios-grupos.ps1`; passos 1–3 do README |
| Aplicação no IIS (site, pool, dependências, pasta, NTFS) | 2 | `04-iis.ps1`, `05-sql.ps1`, `08-publicar-api.ps1` |
| Alertas de CPU, disco e memória | 2 | `07-monitoramento.ps1` |
| Recuperação do serviço e do pool, teste de falha | 1 | `09-teste-de-falha.ps1`, `infra/TESTE-DE-FALHA.md` |
| Logs de auditoria | 1 | `06-gpo.ps1` e a seção de auditoria do README |
| Funcionamento completo | 2 | painel em `api/TermometroDaSala.Api/wwwroot/index.html` e `GET /ambientes`, no vídeo |

- [ ] Montar a VM e preencher `infra/EVIDENCIAS.md` (prints, PIDs, IP, data)
- [ ] Preencher a equipe e os RMs no README e em `infra/EVIDENCIAS.md`
- [ ] Gravar o vídeo de até 7 min e publicar o link
- [ ] Enviar o link do repositório no Teams

## Physical Computing

**Enviar:** o link do projeto no Wokwi, o link do vídeo de até 4 min e o `esp32/integrantes.txt`. Roteiro: `esp32/ROTEIRO-VIDEO.md`. O briefing não divide a nota em pontos.

| Item | Onde |
|---|---|
| Circuito ESP32 no Wokwi | `esp32/sketch.ino`, `esp32/diagram.json`, `esp32/libraries.txt`, `esp32/secrets.exemplo.h` |
| Vídeo com a solução, não um circuito à parte | `esp32/ROTEIRO-VIDEO.md` |
| Nomes dos integrantes | `esp32/integrantes.txt` |

- [ ] Publicar o projeto no Wokwi (o link ainda não está no repositório) e não versionar `secrets.h`
- [ ] Preencher o RM em `esp32/integrantes.txt`
- [ ] Gravar o vídeo de até 4 min
- [ ] Enviar os dois links e o txt no Teams

## Testing (TOGAF) e pitch

**Enviar:** o diagrama ArchiMate e o documento descritivo. O pitch de até 4 min, no YouTube, entra nesta disciplina. Roteiro do pitch: `arquitetura/PITCH.md`.

| Item | Peso | Onde |
|---|---:|---|
| Visão da arquitetura (motivação) | 20% | `arquitetura/DOCUMENTO.md` seção 4.1; `arquitetura/diagrama.svg`; `termometro-da-sala.xml` |
| Visão de negócios | 20% | seção 4.2 |
| Visão de sistemas | 20% | seção 4.3 |
| Visão de infraestrutura | 20% | seção 4.4 |
| Nome do projeto, equipe (nome e RM) e problema | 10% | abertura de `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md` |
| Público e estimativa de impacto, com fonte | 10% | seção 7 de `arquitetura/DOCUMENTO.md` |

- [ ] Fazer a entrevista e colocar a fala no pitch e o piloto da escola na seção 7
- [ ] Preencher os RMs em `arquitetura/DOCUMENTO.md` e `arquitetura/PITCH.md`
- [ ] Gravar o pitch de até 4 min, publicar no YouTube e enviar o link
- [ ] Enviar diagrama e documento no Teams

## Campos ainda em branco

O marcador de RM, link e ensaio é a palavra preencher entre colchetes. O de bairro, na ficha, é a expressão bairro da escola entrevistada entre colchetes. O de print no roteiro da VM é a palavra colar entre colchetes. Este arquivo só aponta a linha; não é um campo para preencher.

### RM

- `api/README.md:21`
- `app/package.json:7`
- `desktop/README.md:23` (também o espaço dos demais integrantes, se houver)
- `desktop/integrantes.txt:2`
- `esp32/integrantes.txt:1`
- `ml/README.md:17`
- `ml/gerar_notebook.py:20`
- `ml/termometro_da_sala.ipynb:20`
- `infra/README.md:17`
- `infra/EVIDENCIAS.md:5`
- `seguranca/RELATORIO-RISCOS.md:6`
- `arquitetura/DOCUMENTO.md:5`
- `arquitetura/PITCH.md:17` (também os demais integrantes)
- `arquitetura/PITCH.md:61`
- `arquitetura/PITCH.md:62` (demais integrantes e RM, sem a palavra preencher)

### Bairro

- `README.md:10`
- `api/README.md:10`
- `app/README.md:10`
- `desktop/README.md:8`
- `esp32/README.md:8`
- `ml/README.md:8`
- `infra/README.md:8`
- `seguranca/README.md:8`
- `arquitetura/README.md:8`
- `arquitetura/DOCUMENTO.md:12`
- `docs/PLANO.md:49`
- `arquitetura/PITCH.md:23` e `arquitetura/PITCH.md:71` (bairro na fala e no slide, junto da pessoa)

### Pessoa entrevistada

- `desktop/README.md:21`
- `arquitetura/PITCH.md:23`
- `arquitetura/PITCH.md:70`
- `arquitetura/PITCH.md:71`
- `arquitetura/DOCUMENTO.md:169` (número de salas, turnos e alunos da escola)

### Prints

Marcador colar, o espaço do print em `infra/EVIDENCIAS.md`. Onde a linha tem mais de um, o arquivo entra uma vez:

- `infra/EVIDENCIAS.md:23`
- `infra/EVIDENCIAS.md:34`
- `infra/EVIDENCIAS.md:46`
- `infra/EVIDENCIAS.md:64`
- `infra/EVIDENCIAS.md:74`
- `infra/EVIDENCIAS.md:84`
- `infra/EVIDENCIAS.md:98`
- `infra/EVIDENCIAS.md:111`
- `infra/EVIDENCIAS.md:121`
- `infra/EVIDENCIAS.md:130`
- `infra/EVIDENCIAS.md:134`
- `infra/EVIDENCIAS.md:145`
- `infra/EVIDENCIAS.md:154`
- `infra/EVIDENCIAS.md:158`

Marcador preencher no que o print precisa anotar, no mesmo arquivo:

- `infra/EVIDENCIAS.md:74` (ACL dos filhos)
- `infra/EVIDENCIAS.md:111` (firewall)
- `infra/EVIDENCIAS.md:130` (limites restaurados)
- `infra/EVIDENCIAS.md:134` (PIDs, horários, confirmação e Rapid-Fail)
- `infra/EVIDENCIAS.md:145` (PID e o que fazer se a recuperação falhar)
- `infra/EVIDENCIAS.md:158` (origem dos dados)

### Link e ensaio que não são print

- `desktop/integrantes.txt:3` (link)
- `arquitetura/DOCUMENTO.md:16` (link)
- `infra/EVIDENCIAS.md:6` (data do ensaio)
- `infra/EVIDENCIAS.md:7` (versão do Windows e hipervisor)
- `infra/EVIDENCIAS.md:8` (IP)
- `infra/EVIDENCIAS.md:9` (link do repositório e do vídeo)
- `infra/EVIDENCIAS.md:10` (se o professor exigiu RDS)
