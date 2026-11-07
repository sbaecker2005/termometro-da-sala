# Evidências da API

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

Coleta de 07/11/2026, às 08:51: interrompida em B1 por falha de criptografia no cliente SQL.
Erro real: "The instance of SQL Server you attempted to connect to requires encryption but this machine does not support it."
O [registro de B1](saidas/B1.txt) contém o erro completo. Nenhuma migração foi comprovada nesta rodada.
B2, B3 e as chamadas HTTP não foram executadas; suas saídas e status abaixo são históricos.
O [resumo desta tentativa](saidas/conferencia.tsv) contém somente B1: 0 OK e 1 DIVERGE.
Na segunda tentativa de 07/11, o build passou (0 avisos, 0 erros) e a biblioteca nativa
`Microsoft.Data.SqlClient.SNI.dll` estava presente para Windows x64. A factory do EF usa
`ConnectionStrings__Sala` sobre o appsettings (teste `FactoryUsaConnectionStringDoAmbiente`).
Com `Encrypt=False` somente no coletor local, B1 passou a falhar com
"Failed to generate SSPI context." A conexão direta pelo cliente do Windows também falhou
na autenticação, inclusive por memória compartilhada. A coleta parou antes de B2 e das chamadas.
As saídas anteriores foram preservadas, incluindo B1 e o resumo: antes e depois,
0 OK e 1 DIVERGE. O B1 vinculado acima registra a primeira tentativa, não a segunda.
Falta autenticação Windows funcional no SQL Server nesta sessão para concluir a coleta;
os status históricos não comprovam o frio nem o mapa no banco local.
Telas, IIS/HTTPS, ESP32, MQTT e revisão visual da foto real continuam com o coordenador.

**Pendente: recapturar evidências antigas com SQL Server disponível**, após aplicar
`AlertaFrio`: B1 e B2 devem comprovar as cinco migrações desta versão (incluindo `MapaTermico`);
B3 e 24 devem comprovar as cinco faixas.
As saídas históricas abaixo não comprovam a migração de frio aplicada no banco.

## Saídas geradas por comando

| Arquivo | Comando (a partir de `api/`) | O que mostra |
|---|---|---|
| [dotnet-test.txt](dotnet-test.txt) | `dotnet test TermometroDaSala.Api.Tests/TermometroDaSala.Api.Tests.csproj --no-restore` | Resultado local com os testes de fronteira do frio e da migração |
| [migracoes-idempotente.sql](migracoes-idempotente.sql) | `dotnet ef migrations script --idempotent --project TermometroDaSala.Api -o evidencias/migracoes-idempotente.sql` | SQL das 4 migrações, incluindo `AlertaFrio`. Em `CREATE TABLE [Leituras]`, só a FK para `Ambientes` tem `ON DELETE CASCADE`; a FK para `Dispositivos` fica sem ação |

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

Os itens 26 a 29 aguardam nova coleta: o antigo `00-resposta.txt` sobrescreveu pedidos sem chave
e não comprova o mapa. O Demo prepara com POST (201), sem devolver o mapa; cada GET (200)
é registrado antes de preparar o período seguinte. O PUT substitui a configuração do mapa
da sala-01; o Demo substitui as leituras simuladas da Sala 7. Use o banco de demonstração.
Limite e cabeçalhos passaram de 26/27 para 30/31 para não sobrescrever as evidências do mapa.

## Telas

| # | Tela | O que mostra | Print |
|---|---|---|---|
| T1 | `/swagger` | As rotas documentadas | [print](saidas/T1-swagger.png) - OK |
| T2 | `/` (painel) | As salas com dados vindos do banco | [print](saidas/T2-painel.png) - OK |
