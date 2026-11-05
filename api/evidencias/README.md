# Evidências da API

Índice das provas de funcionamento da API. Cada print mostra o método, a rota, o status
e o corpo da resposta. Nenhum print pode mostrar `ChaveCoordenacao`, senha do banco ou
dados de alunos. Os passos seguem o roteiro do Insomnia em [../README.md](../README.md#roteiro-do-insomnia).

## Saídas geradas por comando

| Arquivo | Comando (a partir de `api/`) | O que mostra |
|---|---|---|
| [dotnet-test.txt](dotnet-test.txt) | `dotnet test --logger "console;verbosity=normal"` (só as linhas de resultado) | 47 testes aprovados, incluindo `EsquemaTestes` e `DemoAplicaOModeloAoDiaDoCsvEDaOMesmoValor` |
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

A numeração é a dos nomes na coleção. A aba de testes de cada requisição mostra se o status bateu.

| # | Requisição | Status esperado | Print |
|---|---|---|---|
| 01 | `POST /leituras - sucesso` | 201 com `id`, `indiceCalor` e `nivel` | [preencher] |
| 02 | `POST /leituras - reenvio` | 400 `{"motivo":"reenvio"}` | [preencher] |
| 03 | `POST /leituras - assinatura-invalida` | 400 `{"motivo":"assinatura-invalida"}` | [preencher] |
| 04 | `POST /leituras - fora-da-faixa` | 400 `{"motivo":"fora-da-faixa"}` | [preencher] |
| 05 | `POST /dispositivos/{id}/revogar` (sala 01) | 200 com `ativo: false` | [preencher] |
| 06 | `POST /leituras - dispositivo-revogado` | 400 `{"motivo":"dispositivo-revogado"}` | [preencher] |
| 07 | `POST /ambientes` | 201 `{"id":"sala-05"}` e `Location: /ambientes/sala-05` | [preencher] |
| 08 | `POST /ambientes - repetido` | 409 `{"motivo":"ambiente-existente"}` | [preencher] |
| 09 | `POST /ambientes - sem chave` | 401 e nada gravado | [preencher] |
| 10 | `GET /ambientes/{id}` (sala 05) | 200 com a sala | [preencher] |
| 11 | `PUT /ambientes/{id}` (sala 05) | 200; o GET seguinte mostra o nome novo | [preencher] |
| 12 | `DELETE /ambientes/{id} - com leituras` (sala 03) | 409 `{"motivo":"ambiente-com-leituras"}` | [preencher] |
| 13 | `DELETE /ambientes/{id}` (sala 05) | 204; o GET seguinte responde 404 | [preencher] |
| 14 | `POST /acoes` | 201 com `id` e `Location: /acoes?dia=` | [preencher] |
| 15 | `GET /acoes?dia=AAAA-MM-DD` | 200 com a ação registrada | [preencher] |
| 16 | `POST /ocorrencias` (JPEG de sala vazia) | 201 com `id` | [preencher] |
| 17 | `GET /ambientes/{id}/ocorrencias` | 200 com a ocorrência no topo, sem foto | [preencher] |
| 18 | `GET /ocorrencias/{id}/foto - sem chave` | 401 | [preencher] |
| 19 | `GET /ocorrencias/{id}/foto` | 200 `image/jpeg` | [preencher] |
| 20 | `GET /ambientes` | 200 com nível e índice de cada sala | [preencher] |
| 21 | `GET /ambientes/{id}/leituras` | 200 em JSON | [preencher] |
| 22 | `GET /ambientes/{id}/leituras?formato=csv` | 200 `text/csv` | [preencher] |
| 23 | `GET /ambientes/{id}/previsao` | 200 com nível previsto. No Demo, `indiceRuaPrevisto` vem do modelo. Sem rua carregada (fora do Demo): `ruaDisponivel: false` e o índice da última leitura válida da sala; sem rua e sem leitura válida recente: 503 | [preencher] |
| 24 | `GET /niveis` | 200 com as 4 faixas e a ação sugerida | [preencher] |
| 25a | `POST /demo/semear` fora do ambiente Demo | 404 | [preencher] |
| 25b | `POST /demo/semear` em Demo, sem leitura ao vivo | 409 | [preencher] |
| 25c | `POST /demo/semear` em Demo, depois de uma leitura | 201 com `inseridas` | [preencher] |
| 26 | Mais de 30 escritas em um minuto | 429 | [preencher] |
| 27 | Cabeçalhos de qualquer resposta (aba Headers) | `X-Content-Type-Options: nosniff`; no IIS com HTTPS, também `Strict-Transport-Security` | [preencher] |

## Telas

| # | Tela | O que mostra | Print |
|---|---|---|---|
| T1 | `/swagger` | As rotas documentadas | [preencher] |
| T2 | `/` (painel) | As salas com dados vindos do banco | [preencher] |
