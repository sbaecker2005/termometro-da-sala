# Evidências da API

Índice das provas de funcionamento da API. Cada print mostra o método, a rota, o status
e o corpo da resposta. Nenhum print pode mostrar `ChaveCoordenacao`, senha do banco ou
dados de alunos. Os passos seguem o roteiro do Insomnia em [../README.md](../README.md#roteiro-do-insomnia).

## Saídas geradas por comando

| Arquivo | Comando (a partir de `api/`) | O que mostra |
|---|---|---|
| [dotnet-test.txt](dotnet-test.txt) | `dotnet test` | 36 testes aprovados, incluindo `EsquemaTestes` |
| [migracoes-idempotente.sql](migracoes-idempotente.sql) | `dotnet ef migrations script --idempotent --project TermometroDaSala.Api -o evidencias/migracoes-idempotente.sql` | SQL das 3 migrações. Em `CREATE TABLE [Leituras]`, só a FK para `Ambientes` tem `ON DELETE CASCADE`; a FK para `Dispositivos` fica sem ação |

`dotnet ef migrations has-pending-model-changes --project TermometroDaSala.Api` responde
"No changes have been made to the model since the last migration.": o modelo e as
migrações estão alinhados.

## Banco no SQL Server

| # | O que fazer | Resultado esperado | Print |
|---|---|---|---|
| B1 | `dotnet ef database update --project TermometroDaSala.Api` | As 3 migrações aplicadas, sem erro | [preencher] |
| B2 | `SELECT * FROM __EFMigrationsHistory` | `20261105025544_Inicial`, `20261105033613_Enums`, `20261105050515_SegredosPorDispositivo` | [preencher] |
| B3 | `SELECT * FROM Ambientes`, `Dispositivos` (sem a coluna `Segredo`) e `NiveisAcao` | 4 salas, 4 dispositivos e 4 faixas da semente | [preencher] |
| B4 | `dotnet run --project TermometroDaSala.Api --launch-profile http` | Log da API ligada ao SQL Server em `http://localhost:5080` | [preencher] |
| B5 | `SELECT TOP 5 * FROM Leituras ORDER BY Id DESC` depois do roteiro | As leituras gravadas pela API | [preencher] |

## Roteiro do Insomnia

| # | Requisição | Status esperado | Print |
|---|---|---|---|
| 1 | `POST /leituras - sucesso` | 201 com `id`, `indiceCalor` e `nivel` | [preencher] |
| 2 | `POST /leituras - reenvio` | 400 `{"motivo":"reenvio"}` | [preencher] |
| 3 | `POST /leituras - assinatura-invalida` | 400 `{"motivo":"assinatura-invalida"}` | [preencher] |
| 4 | `POST /leituras - fora-da-faixa` | 400 `{"motivo":"fora-da-faixa"}` | [preencher] |
| 5a | `POST /dispositivos/{id}/revogar` (sala 01) | 200 com `ativo: false` | [preencher] |
| 5b | `POST /leituras - dispositivo-revogado` | 400 `{"motivo":"dispositivo-revogado"}` | [preencher] |
| 6a | `POST /ambientes` (primeira vez) | 201 `{"id":"sala-05"}` | [preencher] |
| 6b | `POST /ambientes` (repetido) | 409 | [preencher] |
| 6c | `POST /ambientes` sem `X-Chave-Coordenacao` | 401 e nada gravado | [preencher] |
| 7a | `POST /acoes` | 201 com `id` | [preencher] |
| 7b | `GET /acoes?dia=AAAA-MM-DD` | 200 com a ação registrada | [preencher] |
| 8a | `POST /ocorrencias` (JPEG de sala vazia) | 201 com `id` | [preencher] |
| 8b | `GET /ocorrencias/{id}/foto` | 200 `image/jpeg` | [preencher] |
| 9a | `GET /ambientes` | 200 com nível e índice de cada sala | [preencher] |
| 9b | `GET /ambientes/{id}/leituras` | 200 em JSON | [preencher] |
| 9c | `GET /ambientes/{id}/leituras?formato=csv` | 200 `text/csv` | [preencher] |
| 9d | `GET /ambientes/{id}/previsao` | 200 com rua daqui a 2 h, delta e nível previsto | [preencher] |
| 9e | `POST /demo/semear` fora do ambiente Demo | 404 | [preencher] |
| 9f | `POST /demo/semear` em Demo, sem leitura ao vivo | 409 | [preencher] |
| 9g | `POST /demo/semear` em Demo, depois de uma leitura | 201 com `inseridas` | [preencher] |
| 10 | Mais de 30 escritas em um minuto | 429 | [preencher] |

## Telas

| # | Tela | O que mostra | Print |
|---|---|---|---|
| T1 | `/swagger` | As rotas documentadas | [preencher] |
| T2 | `/` (painel) | As salas com dados vindos do banco | [preencher] |
