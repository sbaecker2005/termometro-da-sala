# Evidências da API

## Validação atual — 17 a 19/11/2026

| Prova | Resultado observado | Registro |
|---|---|---|
| Suíte API atual, com a camada `IServicoAmbientes` e as regras de `Ambiente.Alterar()` | **138 aprovados, zero falhas, zero ignorados** | [registro](dotnet-test-atual.txt) |
| Suíte API com o modelo geral e ajuste por sala (17/11) | **134 aprovados, zero falhas, zero ignorados** | [TRX](../../docs/resultados-testes-2026-11-17/api-modelo-geral-v1.trx) |
| Coleta automática com SQL Server Express real, banco exclusivo da revisão | **98 verificações OK, zero divergências**, incluindo CRUD, cinco migrações, foto sintética, autorizações, mapa com 12 leituras, limite e reenvio concorrente | [conferência](TermometroGS_SOA_ModeloGeral_20261117_134420/conferencia.tsv) |
| HTTP + broker MQTT + assinante da API + SQL Server reais | **33 verificações aprovadas**: as 21 anteriores de concorrência/revogação/rollback e 12 do modelo, ajuste por sala, horizonte, persistência e mapa +1 h | [resultado](../../docs/resultados-testes-2026-11-17/TermometroGS_ModeloGeral_20261117_135412_6ef80b60/resultado.txt), [comandos e limites](../../docs/resultados-testes-2026-11-17/sql-modelo-geral-v1.txt) |
| Insomnia 13.4.0, Scratch Pad local — rodada anterior ao ajuste geral | **29 requisições e 29 asserções aprovadas, zero falhas**, runner 978 ms; revogação 200 e upload 201 também conferidos individualmente | [captura final](insomnia-2026-11-17/colecao-final.png), [condições e reprodução](insomnia-2026-11-17/README.md) |
| Desktop integrado, persistência e PNGs | Cadastro/ação/CSV/revogação, modo local e retorno; vinte cadeiras, quatro sensores e frio em azul | [relatório Windows](../../desktop/docs/VALIDACAO-WINDOWS-2026-11-17.md) |

Os registros automáticos acima **não são prints de execução manual no Insomnia**. O ensaio MQTT usa broker local em loopback; não comprova Wokwi, internet/broker público nem IIS/VM. A foto do ensaio é uma ilustração JPEG sintética, sem pessoas, e não uma foto de escola/piloto.

O coletor cria um banco e diretório exclusivos por execução, falha se o nome já existir e preserva os registros. Para repetir a partir da raiz do kit:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\api\evidencias\gerar-evidencias.ps1 -Autoverificar
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\api\evidencias\gerar-evidencias.ps1
dotnet run --project .\ferramentas\EnsaioSqlMqtt\EnsaioSqlMqtt.csproj -c Validacao --no-restore --disable-build-servers
```

O primeiro comando não inicia API nem acessa banco. Os seguintes exigem SQL Express local e portas livres; usam bancos novos, não apagam bancos anteriores e não são comandos de produção. Resultados esperados: autoverificação aprovada, 98 verificações OK e 33 verificações aprovadas, respectivamente. A falha SQL deliberada no log do ensaio MQTT é a prova do rollback e tem retry aprovado. O ensaio ampliado usa banco `TermometroGS_ModeloGeral_` com timestamp e sufixo GUID curto, HTTP 5082 e MQTT 18884 em loopback. As fixtures do modelo são sintéticas; não atestam precisão em escolas reais.

## Histórico preservado — 07 e 08/11/2026

As tabelas `saidas/` abaixo pertencem à coleta anterior e continuam como histórico. Para a entrega atual, priorize os registros de 17/11 acima. Quantidades e disponibilidade de infraestrutura citadas no histórico não substituem a validação atual.

Índice das provas de funcionamento da API. Cada print mostra o método, a rota, o status
e o corpo da resposta. Nenhum print pode mostrar `ChaveCoordenacao`, senha do banco ou
dados de alunos. Os passos seguem o roteiro do Insomnia em [../README.md](../README.md#roteiro-do-insomnia).

Para repetir no Windows PowerShell 5.1, na raiz da worktree:
`powershell.exe -NoProfile -ExecutionPolicy Bypass -File api/evidencias/gerar-evidencias.ps1`.
Use `-Foto C:\caminho\sala-vazia.jpg` para fornecer um JPEG real sem pessoas;
sem esse argumento, a entrada é uma ilustração sintética de sala vazia, identificada na saída.
Use `-Autoverificar` para conferir HMAC, redação e chaves da coleção sem iniciar a API,
acessar o banco ou gerar saídas. Pedidos sem `metaSortKey` válido ou repetidos recusam a coleção antes da coleta.
O script usa apenas o cache NuGet instalado, desliga MQTT e mantém a configuração em variáveis
do processo. Aplica o perfil HTTP ao binário compilado para encerrá-lo diretamente pelo handle.
O EventLog é desabilitado somente durante a coleta; os logs do console ficam em `saidas/api-*.log`.
Leituras, ações, ocorrências e fotos aceitas permanecem no banco/pasta local; a revogação da sala 01
é restaurada ao estado ativo anterior. Nenhum banco é apagado; uma sala 05 preexistente interrompe a coleta.

Os relatos anteriores de falha de criptografia (08h51) e SSPI em 07/11 não descrevem as saídas atualmente vinculadas. B1, de 07/11 às 10h02, registra banco já atualizado; B2, do mesmo horário, lista as cinco migrações. `saidas/conferencia.tsv` contém 97 verificações OK e zero DIVERGE. São registros de coleta automática local, não testes unitários, prova de execução manual no Insomnia ou evidências da VM.

Resultados de 08/11/2026 (~04h40): API 85 aprovados/0 falhas; desktop 18/0; app 20/0; `tsc --noEmit` do app sem erros. A contagem atual da API está na tabela de validação acima.

IIS/HTTPS, ESP32 e broker público não são cobertos por estas saídas; ficam nas pastas `infra/` e `esp32/`. B3 e a resposta 24 registram cinco faixas, incluindo Frio; B2 registra também MapaTermico.

## Saídas geradas por comando

| Arquivo | Comando (a partir de `api/`) | O que mostra |
|---|---|---|
| [dotnet-test.txt](dotnet-test.txt) | `dotnet test api/TermometroDaSala.sln` e `dotnet test desktop/TermometroDaSala.Desktop.sln` | Registro histórico: 85 testes da API e 18 do desktop aprovados em 08/11/2026, zero falhas. A contagem atual da API (138) está na tabela de validação acima |
| [migracoes-idempotente.sql](migracoes-idempotente.sql) | `dotnet ef migrations script --idempotent --project TermometroDaSala.Api -o evidencias/migracoes-idempotente.sql` | Script idempotente único das 5 migrações (`Inicial`, `Enums`, `SegredosPorDispositivo`, `AlertaFrio` e `MapaTermico`). Em `CREATE TABLE [Leituras]`, só a FK para `Ambientes` tem `ON DELETE CASCADE`; a FK para `Dispositivos` fica sem ação |

`dotnet ef migrations has-pending-model-changes --project TermometroDaSala.Api` responde
"No changes have been made to the model since the last migration.": o modelo e as
migrações estão alinhados.

## Banco no SQL Server

| # | O que fazer | Resultado esperado | Print |
|---|---|---|---|
| B1 | `dotnet ef database update --project TermometroDaSala.Api` | As 5 migrações aplicadas, sem erro | [saida](saidas/B1.txt) - OK |
| B2 | `SELECT * FROM __EFMigrationsHistory` | `20261105025544_Inicial`, `20261105033613_Enums`, `20261105050515_SegredosPorDispositivo`, `20261107033211_AlertaFrio`, `20261107091447_MapaTermico` | [saida](saidas/B2.txt) - OK |
| B3 | `SELECT * FROM Ambientes`, `Dispositivos` (sem a coluna `Segredo`) e `NiveisAcao` | 4 salas, 4 dispositivos e 5 faixas após `AlertaFrio` | [saida](saidas/B3.txt) - OK |
| B4 | `dotnet run --project TermometroDaSala.Api --launch-profile http` | Log da API ligada ao SQL Server em `http://localhost:5080` | [saida](saidas/B4.txt) - OK |
| B5 | `SELECT TOP 5 * FROM Leituras ORDER BY Id DESC` depois do roteiro | As leituras gravadas pela API | [saida](saidas/B5.txt) - OK |

## Roteiro do Insomnia

A numeração é a dos nomes na coleção. A aba de testes de cada requisição mostra se o status bateu.

| # | Requisição | Status esperado | Print |
|---|---|---|---|
| 01 | `POST /leituras - sucesso` | 201 com `id`, `indiceCalor` e `nivel` | [saida](saidas/01-resposta.txt) - OK |
| 02 | `POST /leituras - reenvio` | 400 `{"motivo":"reenvio"}` | [saida](saidas/02-resposta.txt) - OK |
| 03 | `POST /leituras - assinatura-invalida` | 400 `{"motivo":"assinatura-invalida"}` | [saida](saidas/03-resposta.txt) - OK |
| 04 | `POST /leituras - fora-da-faixa` | 400 `{"motivo":"fora-da-faixa"}` | [saida](saidas/04-resposta.txt) - OK |
| 05 | `POST /dispositivos/{id}/revogar` (sala 01) | 200 com `ativo: false` | [saida](saidas/05-resposta.txt) - OK |
| 06 | `POST /leituras - dispositivo-revogado` | 400 `{"motivo":"dispositivo-revogado"}` | [saida](saidas/06-resposta.txt) - OK |
| 07 | `POST /ambientes` | 201 `{"id":"sala-05"}` e `Location: /ambientes/sala-05` | [saida](saidas/07-resposta.txt) - OK |
| 08 | `POST /ambientes - repetido` | 409 `{"motivo":"ambiente-existente"}` | [saida](saidas/08-resposta.txt) - OK |
| 09 | `POST /ambientes - sem chave` | 401 e nada gravado | [saida](saidas/09-resposta.txt) - OK |
| 10 | `GET /ambientes/{id}` (sala 05) | 200 com a sala | [saida](saidas/10-resposta.txt) - OK |
| 11 | `PUT /ambientes/{id}` (sala 05) | 200; o GET seguinte mostra o nome novo | [saida](saidas/11-resposta.txt) - OK |
| 12 | `DELETE /ambientes/{id} - com leituras` (sala 03) | 409 `{"motivo":"ambiente-com-leituras"}` | [saida](saidas/12-resposta.txt) - OK |
| 13 | `DELETE /ambientes/{id}` (sala 05) | 204; o GET seguinte responde 404 | [saida](saidas/13-resposta.txt) - OK |
| 14 | `POST /acoes` | 201 com `id` e `Location: /acoes?dia=` | [saida](saidas/14-resposta.txt) - OK |
| 15 | `GET /acoes?dia=AAAA-MM-DD` | 200 com a ação registrada | [saida](saidas/15-resposta.txt) - OK |
| 16 | `POST /ocorrencias` (JPEG de sala vazia) | 201 com `id` | [saida](saidas/16-resposta.txt) - OK |
| 17 | `GET /ambientes/{id}/ocorrencias` | 200 com a ocorrência no topo, sem foto | [saida](saidas/17-resposta.txt) - OK |
| 18 | `GET /ocorrencias/{id}/foto - sem chave` | 401 | [saida](saidas/18-resposta.txt) - OK |
| 19 | `GET /ocorrencias/{id}/foto` | 200 `image/jpeg` | [saida](saidas/19-resposta.txt) - OK |
| 20 | `GET /ambientes` | 200 com nível e índice de cada sala | [saida](saidas/20-resposta.txt) - OK |
| 21 | `GET /ambientes/{id}/leituras` | 200 em JSON | [saida](saidas/21-resposta.txt) - OK |
| 22 | `GET /ambientes/{id}/leituras?formato=csv` | 200 `text/csv` | [saida](saidas/22-resposta.txt) - OK |
| 23 | `GET /ambientes/{id}/previsao` | 200 com nível previsto. No Demo, `indiceRuaPrevisto` vem do modelo. Sem rua carregada (fora do Demo): `ruaDisponivel: false` e o índice da última leitura válida da sala; sem rua e sem leitura válida recente: 503 | [saida](saidas/23-resposta.txt) - OK |
| 24 | `GET /niveis` | 200 com as 5 faixas e a ação sugerida | [saida](saidas/24-resposta.txt) - OK |
| 25a | `POST /demo/semear` fora do ambiente Demo | 404 | [saida](saidas/25a-resposta.txt) - OK |
| 25b | `POST /demo/semear` em Demo, sem leitura ao vivo | 409 | [saida](saidas/25b-resposta.txt) - OK |
| 25c | `POST /demo/semear` em Demo, depois de uma leitura | 201 com `inseridas` | [saida](saidas/25c-resposta.txt) - OK |
| 26 | Development: `PUT /ambientes/sala-01/mapa` com chave, planta 6 × 4, sensor na janela e Cadeira 1 | 204; configura antes do GET | [saida](saidas/26-resposta.txt) - OK |
| 27 | Development: `GET /ambientes/sala-01/mapa` | 200 com dimensões, `grade`, `pontos` (janela e Cadeira 1) e `precaucoes`; sem leitura recente, valores podem ser nulos | [saida](saidas/27-resposta.txt) - OK |
| 28 | Demo: `POST /demo/mapa?periodo=tarde`, depois `GET /ambientes/sala-07/mapa` | Preparo 201; GET 200 com `grade`, `pontos`, `precaucoes`, janela a 35 °C/perigo, troca de lugar e ambiente desigual | [saida](saidas/28-resposta.txt) - OK |
| 29 | Demo: `POST /demo/mapa?periodo=madrugada`, depois `GET /ambientes/sala-07/mapa` | Preparo 201; GET 200 com `grade`, `pontos`, `precaucoes`, canto do fundo a 15 °C/frio, troca de lugar e ambiente desigual | [saida](saidas/29-resposta.txt) - OK |
| 30 | Mais de 30 escritas em um minuto | 429 | [saida](saidas/30-limite.txt) - OK |
| 31 | Cabeçalhos de qualquer resposta (aba Headers) | `X-Content-Type-Options: nosniff`; no IIS com HTTPS, também `Strict-Transport-Security` | [saida](saidas/31-resposta.txt) - OK |

Os itens 26 a 29 têm saídas com `Conferencia: OK`, confirmadas em `saidas/conferencia.tsv`;
restam a conferência visual e os ensaios de laboratório. O Demo prepara com POST (201), sem devolver o mapa; cada GET (200)
é registrado antes de preparar o período seguinte. O PUT substitui a configuração do mapa
da sala-01; o Demo substitui as leituras simuladas da Sala 7. Use o banco de demonstração.
Limite e cabeçalhos passaram de 26/27 para 30/31 para não sobrescrever as evidências do mapa.

## Telas

| # | Tela | O que mostra | Print |
|---|---|---|---|
| T1 | `/swagger` | As rotas documentadas | [print](saidas/T1-swagger.png) - OK |
| T2 | `/` (painel) | As salas com dados vindos do banco | [print](saidas/T2-painel.png) - OK |
