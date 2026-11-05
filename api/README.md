# Termômetro da Sala — API

Global Solution 2026.2 (CitizenTech) — Engenharia de Software, 3º ano, FIAP.

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | [preencher] |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Integrantes

- Samuel Baecker - RM [preencher]

## Como rodar

Requisitos: .NET 8 e SQL Server (a conexão está em `TermometroDaSala.Api/appsettings.json`). Os testes não precisam de SQL Server: usam SQLite em memória.

```bash
cd api
dotnet tool restore
dotnet ef database update --project TermometroDaSala.Api
dotnet run --project TermometroDaSala.Api --launch-profile http
dotnet test
```

A API e o painel sobem em `http://localhost:5080`. O painel é a raiz (`/`). Swagger em `/swagger` fora de Production.

Modo demonstração (a rota `/demo/semear` só existe nesse ambiente; fora dele responde 404):

```bash
ASPNETCORE_ENVIRONMENT=Demo dotnet run --project TermometroDaSala.Api --launch-profile http
```

No PowerShell: `$env:ASPNETCORE_ENVIRONMENT="Demo"`.

O assinante MQTT conecta em `broker.hivemq.com:1883`, tópico `fiap-gs/termometro-da-sala/leituras` (configurável em `Mqtt`). Para desligar: `Mqtt__Desligado=true`.

Semente: salas `sala-01` a `sala-04`, dispositivo `esp32-sala-0N` em cada uma, segredo de demonstração `segredo-de-teste-sala-03`. Ele é revogável por `POST /dispositivos/{id}/revogar`. Não é segredo de produção.

## Rotas

| Método | Rota | O que faz |
|---|---|---|
| GET | `/ambientes` | Salas com nível e índice atuais, `temVentilador` e `dispositivoId` (null sem sensor) |
| POST | `/ambientes` | Cadastra sala sem criar ou vincular um sensor |
| GET | `/ambientes/{id}/leituras` | Leituras em JSON. `?formato=csv` devolve CSV |
| GET | `/ambientes/{id}/previsao` | Rua daqui a 2 h, delta da sala e nível previsto |
| POST | `/leituras` | Mesma leitura assinada do MQTT |
| POST | `/ocorrencias` | Multipart: `ambienteId`, `descricao`, `foto` JPEG até 5 MB |
| GET | `/ocorrencias/{id}/foto` | A foto da ocorrência, só pelo id |
| GET | `/acoes?dia=AAAA-MM-DD` | Ações do dia civil de São Paulo; data ausente/inválida retorna 400 |
| POST | `/acoes` | Ação tomada: `ambienteId`, `nivel`, `descricao` |
| POST | `/dispositivos/{id}/revogar` | Marca o dispositivo como inativo |
| POST | `/demo/semear?ambienteId=` | Só com `ASPNETCORE_ENVIRONMENT=Demo`. 409 sem leitura ao vivo |

`POST /leituras`:

```json
{ "ambienteId": "sala-03", "dispositivo": "esp32-sala-03",
  "temperaturaC": 33.4, "umidadePct": 58, "medidoEm": 1794331200,
  "assinatura": "2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0" }
```

O `medidoEm` de uma leitura real precisa estar a no máximo 10 min do relógio do servidor. Recusa com `{ "motivo": "..." }`: `assinatura-invalida`, `reenvio`, `dispositivo-revogado`, `atrasada`, `fora-da-faixa`. Salto maior que 5 °C, ou sala mais de 8 °C abaixo da rua num dia acima de 30 °C, grava com `suspeita: true` e fica fora da previsão.

A coleção do Insomnia está em [insomnia.json](insomnia.json).

## Previsão

O índice de calor é o da NOAA (Rothfusz), em °C com 1 casa. Faixas: abaixo de 27 normal, 27 a 32 atenção, 32 a 41 cuidado, 41 ou mais perigo.

O delta é a média de (índice da sala − índice da rua) nas últimas 3 h, com no mínimo 3 pares no mesmo instante UTC, sem as leituras suspeitas. A previsão da sala é o índice da rua daqui a 2 h mais o delta. Sem pares suficientes, `deltaSala` e `indiceSalaPrevisto` vêm `null` e `semLeiturasRecentes` vem `true`: mostra-se só a rua.

A rua daqui a 2 h vem do modelo linear em `TermometroDaSala.Api/modelo/modelo.json` (cópia de `ml/modelo.json`, regressão treinada no notebook com o horário do INMET, estação A701, 2022–2025). No teste de 2025 o MAE foi 1,05 °C, contra 1,44 °C do baseline "daqui a 2 h = agora". O caminho fica em `Modelo:Caminho`. Sem o arquivo, ou com JSON inválido, a API repete o índice da rua de agora e registra no log.

No ambiente Demo, a rua vem de `demo/rua-dia-quente.csv` (dia quente de exemplo) projetado na hora local de São Paulo. `POST /demo/semear` lê `demo/leituras-simuladas.csv` e desloca a curva para terminar na última leitura ao vivo (`origem = dispositivo`), gravando as 3 h anteriores com `origem = simulada`.

Limitação: o delta assume que a diferença das últimas 3 h se mantém nas próximas 2 h. No fim da tarde ele tende a superestimar.

## Dados

As leituras do sensor no Wokwi, as leituras do modo demonstração e as ocorrências são simuladas. Os dados meteorológicos de exemplo em `demo/` não substituem a série real do INMET. Nenhum dado de aluno é coletado. A foto é salva com nome GUID na pasta `Fotos:Pasta` e só é servida pelo id da ocorrência.

## Configuração do banco e migrações

A aplicação e `SalaDbFactory` carregam `appsettings.json`, a configuração do ambiente e
`ConnectionStrings__Sala`; a variável prevalece sobre o arquivo. A factory usa os arquivos
copiados para a saída do build: recompile depois de mudar um appsettings.
Exemplos em PowerShell, a partir de `api/`:

```powershell
# LocalDB (Windows, instalado separadamente)
$env:ConnectionStrings__Sala='Server=(localdb)\MSSQLLocalDB;Database=TermometroDaSala;Integrated Security=True;TrustServerCertificate=True'
dotnet build --no-restore
dotnet ef migrations list --project TermometroDaSala.Api --no-build
dotnet ef database update --project TermometroDaSala.Api --no-build
dotnet run --no-build --project TermometroDaSala.Api --launch-profile http
```

Alternativa: SQL Server 2022 em Docker. Defina `MSSQL_SA_PASSWORD` no ambiente local
com uma senha forte, sem registrá-la em arquivos, histórico ou capturas. O comando abaixo
usa essa variável existente, preserva o banco num volume e publica somente no localhost:

```powershell
docker run --name termometro-sql --env ACCEPT_EULA=Y --env MSSQL_SA_PASSWORD -p 127.0.0.1:14333:1433 -v termometro-sql:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
$env:ConnectionStrings__Sala="Server=localhost,14333;Database=TermometroDaSala;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True"
# Aguarde o SQL Server ficar pronto; depois execute build, migrations list e database update acima.
```

Histórico esperado: `20261105025544_Inicial` e `20261105033613_Enums`.
A segunda mantém os valores textuais e reduz o tamanho de Tipo (4), FaceSol (5),
Nivel/NivelPrevisto (7) e Origem (11) ao tamanho dos enums. Não remove tabelas ou linhas.
Em banco existente, confira antes se há valores fora do domínio ou maiores que esses
limites; corrija-os com revisão antes de aplicar. Faça backup antes de migrar dados que
precisem ser preservados. Os testes usam SQLite em memória e não comprovam a aplicação
no SQL Server.

## Cadastro e ações do desktop

`POST /ambientes` recebe:

```json
{"id":"sala-05","nome":"Sala 5","andar":1,"faceSol":"oeste","temVentilador":true}
```

Retorna 201 com `{"id":"sala-05"}`. ID no formato `sala-NN`, nome de 1 a 80 caracteres
(não só espaços), andar de 0 a 20 e face `leste`, `norte`, `oeste` ou `sul`.
Entrada inválida retorna 400; ID existente retorna 409. O tipo é `sala`.
O cadastro não provisiona credenciais de sensor: `dispositivoId` fica null até haver
um dispositivo vinculado. Os sensores da semente continuam disponíveis no GET.

`GET /acoes?dia=2026-11-05` devolve um array, do registro mais recente ao mais antigo,
com `id`, `ambienteId`, `nivel`, `descricao` e `registradoEm` (instante com offset).
O filtro usa a data civil de São Paulo, incluindo meia-noite e excluindo a meia-noite
do dia seguinte. Um dia sem ações retorna `[]`.

## Roteiro do Insomnia

Importe `insomnia.json` numa versão do Insomnia com scripts de requisição e resposta e
`require('crypto-js')`. Ajuste `base_url` e `dia` no ambiente Base. Use um banco de
**demonstração**, pois a revogação persiste; para repetir os testes de leitura, o
`esp32-sala-01` precisa estar ativo no início. Desligue MQTT (`Mqtt__Desligado=true`)
e o simulador durante o roteiro para não disputar os timestamps.

1. Execute `POST /leituras - sucesso`: o pre-request script gera `medidoEm` atual,
   assina o texto canônico com a chave pública de teste e guarda o corpo em
   `leitura_aceita`. Espere 201. Para outra leitura nova, aguarde ao menos um segundo.
2. Execute `POST /leituras - reenvio`: reutiliza exatamente o corpo anterior;
   espere 400 com `motivo: reenvio`.
3. Execute `POST /leituras - assinatura-invalida`: espere 400 com esse motivo.
4. Execute `POST /leituras - fora-da-faixa`: assina 70 °C para o sensor da sala 04;
   espere 400 com esse motivo.
5. Execute `POST /dispositivos/{id}/revogar` (sala 01): espere 200. Depois execute
   `POST /leituras - dispositivo-revogado`: espere 400 com esse motivo.
6. Execute `POST /ambientes`: espere 201 na primeira vez e 409 se repetir.
7. Execute `POST /acoes` e `GET /acoes?dia=AAAA-MM-DD`, ajustando `dia` para hoje em
   São Paulo: espere 201 e 200 com a ação registrada.
8. No multipart de `POST /ocorrencias`, selecione um JPEG real de uma sala vazia,
   sem alunos ou dados pessoais (até 5 MB; nome simples, por exemplo `sala.jpg`).
   Espere 201. O after-response script guarda `id` em `ocorrencia_id`;
   `GET /ocorrencias/{id}/foto` usa essa variável e deve retornar 200, `image/jpeg`.
9. Confira também GET ambientes, leituras, CSV e previsão. `/demo/semear` exige
   ambiente Demo e uma leitura ao vivo: 404 fora de Demo, 409 sem leitura, 201 após ela.

As fotos locais ficam em `TermometroDaSala.Api/fotos/`, ignorada pelo repositório.
Salve as capturas manuais em `api/evidencias/`, sem dados pessoais ou credenciais.
A aplicação de migrações no SQL Server e as capturas de cada resposta ainda precisam
ser executadas no ambiente de apresentação.

## Verificação local

```powershell
dotnet build --no-restore
dotnet test --no-restore
```

Nesta execução local, o MSBuild padrão da solução retornou código 1 sem diagnóstico
(zero erros reportados). A execução serial concluiu com zero avisos e 25 testes aprovados:

```powershell
dotnet build --no-restore -m:1
dotnet test --no-restore -m:1
```

Caso esta worktree não tenha `obj/project.assets.json`, gere os assets sem rede,
utilizando somente pacotes já presentes no cache (não baixa dependências):

```powershell
dotnet restore TermometroDaSala.Api/TermometroDaSala.Api.csproj --source "$env:USERPROFILE/.nuget/packages" -p:NuGetAudit=false
dotnet restore TermometroDaSala.Api.Tests/TermometroDaSala.Api.Tests.csproj --source "$env:USERPROFILE/.nuget/packages" -p:NuGetAudit=false --disable-parallel
```

### Limitação preexistente a conferir no SQL Server

A migração Inicial define exclusão em cascata pelos dois caminhos
`Ambientes -> Leituras` e `Ambientes -> Dispositivos -> Leituras`.
O SQL Server pode recusar a criação dessas FKs com erro 1785 (múltiplos caminhos
de cascata). A migração Enums não altera essas FKs; valide o histórico num banco
de apresentação antes de afirmar que o provisionamento completo funciona.
