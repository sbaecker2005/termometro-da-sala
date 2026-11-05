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
| GET | `/ambientes` | Salas com nível e índice atuais |
| GET | `/ambientes/{id}/leituras` | Leituras em JSON. `?formato=csv` devolve CSV |
| GET | `/ambientes/{id}/previsao` | Rua daqui a 2 h, delta da sala e nível previsto |
| POST | `/leituras` | Mesma leitura assinada do MQTT |
| POST | `/ocorrencias` | Multipart: `ambienteId`, `descricao`, `foto` JPEG até 5 MB |
| GET | `/ocorrencias/{id}/foto` | A foto da ocorrência, só pelo id |
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
