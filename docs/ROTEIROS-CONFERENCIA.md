# Conferência dos roteiros

Conferência dos cinco pontos no conteúdo desta pasta, em 08/11/2026. As referências abaixo usam as linhas após os ajustes. Não houve consulta ao histórico de versões, acesso à rede, instalação nem execução de banco ou aplicação.

## 1. Ambiente Demo e perfil de inicialização

**Valia no ESP32.** `esp32/ROTEIRO-VIDEO.md:7` exigia Demo, mas mandava usar o perfil `http`. Esse perfil fixa `ASPNETCORE_ENVIRONMENT=Development` em `api/TermometroDaSala.Api/Properties/launchSettings.json:19`. As duas semeaduras exigem Demo: `api/TermometroDaSala.Api/Controllers/DemoController.cs:20` e `api/TermometroDaSala.Api/Controllers/DemoController.cs:58`.

Corrigido em `esp32/ROTEIRO-VIDEO.md:11` e `esp32/ROTEIRO-VIDEO.md:15`: ambiente explícito, `--no-launch-profile` e URL explícita para manter a porta 5080. `arquitetura/PITCH.md:36` já exigia Demo, mas não ensinava a inicialização; os comandos de `arquitetura/PITCH.md:40` e `arquitetura/PITCH.md:44` agora evitam o mesmo perfil. Demo também seleciona o previsor próprio em `api/TermometroDaSala.Api/Program.cs:35`.

**Sem mudança necessária:** `app/ROTEIRO-VIDEO.md:5` usa o modo simulado por padrão e não manda semear nem usar o perfil `http` (implementação em `app/services/api.ts:115`). `infra/ROTEIRO-VIDEO.md:9` trata de IIS, sem comando de inicialização por perfil ou rota Demo.

## 2. Preparação reproduzível

**Valia no ESP32 e no pitch.** A preparação de `esp32/ROTEIRO-VIDEO.md:7` não zerava o banco. A semeadura do mapa recusa uma Sala 7 com leitura de dispositivo em `api/TermometroDaSala.Api/Controllers/DemoController.cs:24`; só substitui leituras simuladas em `api/TermometroDaSala.Api/Controllers/DemoController.cs:40`. Portanto repetir a preparação depois de um ensaio podia resultar em 409.

`arquitetura/PITCH.md:36` também não zerava o histórico. A semeadura exige leitura de dispositivo (`api/TermometroDaSala.Api/Controllers/DemoController.cs:60`), usa a primeira e a última leituras antigas como referência (`api/TermometroDaSala.Api/Controllers/DemoController.cs:79`) e remove apenas as simuladas (`api/TermometroDaSala.Api/Controllers/DemoController.cs:84`). Além disso, a leitura anterior por dispositivo influencia a suspeita (`api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs:48`).

Corrigido com bancos LocalDB exclusivos e descartáveis, recriados antes de cada ensaio ou tomada: `esp32/ROTEIRO-VIDEO.md:10` e `arquitetura/PITCH.md:39`. A conexão vem da variável de ambiente, que a factory de migrações lê em `api/TermometroDaSala.Api/Dados/SalaDbFactory.cs:10`, e a aplicação lê em `api/TermometroDaSala.Api/Program.cs:26`. A preparação para API e Wokwi antes do reset, aplica as migrações e mantém MQTT ligado. No ESP32, o mapa é semeado antes do sensor (`esp32/ROTEIRO-VIDEO.md:18`). No pitch, o reset sozinho era insuficiente: o circuito atual usa quatro IDs da Sala 7 (`esp32/README.md:36`), não a Sala 3.

O pitch agora documenta o cadastro após cada reset (`arquitetura/PITCH.md:49`): a migração inicial recria a Sala 3 (`api/TermometroDaSala.Api/Dados/Migrations/20261105025544_Inicial.cs:192`) e seus dispositivos iniciais (`api/TermometroDaSala.Api/Dados/Migrations/20261105025544_Inicial.cs:207`); o SQL acrescenta três IDs, mantendo `esp32-sala-03` como porta. A configuração local do Wokwi alinha ambiente, quatro IDs e valor público de demonstração ao cadastro (`arquitetura/PITCH.md:78`). Não basta usar o cadastro de sala, que não cria sensores (`api/TermometroDaSala.Api/Controllers/AmbientesController.cs:20`), nem o PUT do mapa, que exige sensores existentes (`api/TermometroDaSala.Api/Controllers/MapaController.cs:44`). Antes de semear, exige-se conferir na API as leituras dos quatro dispositivos, não apenas as publicações do Serial (`arquitetura/PITCH.md:80`). A limpeza e esse cadastro compõem a preparação documentada; a reprodutibilidade operacional ainda depende de repetir o ensaio, sem aproveitar estado anterior.

**Correção da preparação espacial:** cadastrar os sensores não bastava para a fala de `arquitetura/PITCH.md:34`. A migração `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:13` apenas cria os campos e a tabela de lugares; `/demo/semear` só acrescenta leituras (`api/TermometroDaSala.Api/Controllers/DemoController.cs:92`). Sem posições e dimensões, os sensores caem em (0,0), sem lugares (`api/TermometroDaSala.Api/Servicos/MapaTermico.cs:60`). O SQL de `arquitetura/PITCH.md:58` agora configura a Sala 3 com planta fictícia de 6 × 4 m, quatro sensores rotulados e seis cadeiras, reproduzindo a geometria do exemplo existente em `api/TermometroDaSala.Api/Controllers/DemoController.cs:29`. A conferência em `arquitetura/PITCH.md:76` exige dimensões, coordenadas e dez pontos no GET do mapa, implementado em `api/TermometroDaSala.Api/Controllers/MapaController.cs:14`. Com todos os sensores na mesma temperatura, não se deve esperar contraste térmico nem sugestão de um lugar melhor; as posições continuam distintas. Sem mudança de código, fixtures, configuração ou tempos de gravação.

**Limite explícito:** os comandos apagam integralmente apenas os bancos locais nomeados nos roteiros. Não devem apontar para dados a preservar. LocalDB, ferramenta EF e, no pitch, um editor SQL precisam estar disponíveis; não foram instalados nem executados nesta conferência. Os INSERTs são exclusivos do banco descartável do pitch e devem ser executados uma vez após cada reset. A configuração do pitch fica em projeto Wokwi separado do ensaio ESP32 da Sala 7.

**Sem mudança necessária:** `app/ROTEIRO-VIDEO.md:5` já pede identificação não salva e usa dados simulados; `infra/ROTEIRO-VIDEO.md:13` precisa justamente demonstrar persistência depois da reciclagem do pool, sem usar semeadura Demo.

## 3. Saltos de temperatura

**A regra vale; o ESP32 já a respeitava.** `api/TermometroDaSala.Api/Dominio/Contrato.cs:111` marca como suspeita uma diferença absoluta maior que 5 °C, tanto ao subir quanto ao descer. A comparação é por dispositivo em `api/TermometroDaSala.Api/Servicos/ServicoLeituras.cs:48`.

Preservados os passos 24 → 29 → 33 → 37 °C (`esp32/ROTEIRO-VIDEO.md:29`), a volta 37 → 33 °C (`esp32/ROTEIRO-VIDEO.md:46`) e a passagem anterior à tomada (`esp32/ROTEIRO-VIDEO.md:20`). Essa passagem continua necessária: a semente do mapa põe a janela em 35 °C (`api/TermometroDaSala.Api/Controllers/DemoController.cs:44`), e sua primeira leitura em 24 °C pode ser suspeita; a próxima em 24 °C já não tem esse salto. O firmware mede a cada 10 s (`esp32/sketch.ino:315`).

O pitch já pedia passos de até 4 °C, espera de 10 s e parada em 37 °C com 55 % de umidade. A preparação agora põe os quatro DHT22 em 33 °C / 55 % antes da primeira publicação (`arquitetura/PITCH.md:80`); na tomada, todos sobem uma vez para 37 °C / 55 %, esperando as quatro publicações (`arquitetura/PITCH.md:87`). Isso também evita que um ponto mais frio seja a última leitura da sala, usada pela lista (`api/TermometroDaSala.Api/Controllers/AmbientesController.cs:105`). As faixas e o cálculo estão em `api/TermometroDaSala.Api/Dominio/Contrato.cs:11` e `api/TermometroDaSala.Api/Dominio/Contrato.cs:22`. App e infra não prescrevem saltos de temperatura; nada a corrigir neles neste ponto.

## 4. Painel web e registro da ação

**Valia como ambiguidade no pitch e na infra.** O mural se declara só de leitura em `api/TermometroDaSala.Api/wwwroot/index.html:21`; consulta as salas e previsões por GET em `api/TermometroDaSala.Api/wwwroot/index.html:49` e `api/TermometroDaSala.Api/wwwroot/index.html:64`, e atualiza a cada 30 s em `api/TermometroDaSala.Api/wwwroot/index.html:96`. O registro é feito no desktop em `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:147`. O botão Atualizar existe em `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.Designer.cs:141`.

Em `arquitetura/PITCH.md:87` e `arquitetura/PITCH.md:88`, a tela chamada genericamente de terminal agora é identificada como desktop; usa Atualizar após a publicação e registra a ação ali. `arquitetura/PITCH.md:89` esclarece que os 2 s do painel são um corte com o mural já atualizado, sem esperar sua atualização dentro desse trecho. `infra/ROTEIRO-VIDEO.md:13` agora separa mural, ocorrência pelo app e ação pelo desktop, citando os 30 s.

**Sem mudança necessária:** `app/ROTEIRO-VIDEO.md:15` já diz que a ação fica registrada no desktop; `esp32/ROTEIRO-VIDEO.md:46` já atribui a decisão e o registro à coordenação, sem prometer um formulário no painel.

## 5. Cinco migrações

**Valia na infra.** `infra/ROTEIRO-VIDEO.md:9` dizia três; agora cita as cinco migrações existentes. Evidências: as classes e seus métodos `Up`, sem contar os arquivos Designer nem o snapshot:

| Migração | Código |
|---|---|
| Inicial | `api/TermometroDaSala.Api/Dados/Migrations/20261105025544_Inicial.cs:11` |
| Enums | `api/TermometroDaSala.Api/Dados/Migrations/20261105033613_Enums.cs:8` |
| SegredosPorDispositivo | `api/TermometroDaSala.Api/Dados/Migrations/20261105050515_SegredosPorDispositivo.cs:8` |
| AlertaFrio | `api/TermometroDaSala.Api/Dados/Migrations/20261107033211_AlertaFrio.cs:8` |
| MapaTermico | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |

Os outros três roteiros não afirmavam uma contagem de migrações; nada a corrigir neles neste ponto.

## Escopo e tempos

Alterados somente os roteiros de ESP32, pitch e infra e este documento. O roteiro do app foi conferido e permaneceu intacto. Não há alteração de código, chamadas, testes, fixtures, configuração nem exports; os comandos acrescentados são instruções de preparação, não foram executados. Entrevista, crédito autorizado, nomes, marcadores de identificação e números de impacto foram preservados.

Nenhum intervalo de gravação mudou. A preparação e os cortes prévios ficam fora da tomada. Somas dos blocos, em segundos:

| Roteiro | Soma | Meta e limite |
|---|---|---|
| ESP32 | 20 + 25 + 25 + 25 + 25 + 50 + 25 + 15 = 210 | 3min30s; limite 4min |
| Pitch | 20 + 25 + 90 + 15 + 40 + 20 = 210 | 3min30s; limite 4min |
| App | 15 + 20 + 20 + 25 + 30 = 110 | 1min50s; limite 2min |
| Infra | 20 + 70 + 50 + 45 + 45 + 50 + 70 + 40 = 390 | 6min30s; limite 7min |

No pitch, o passo 33 → 37 °C dos quatro sensores mantém o bloco planejado de 15 s, com espera de cerca de 10 s pelas publicações. A soma é documental; ajustar os quatro controles, as falas e a latência precisam caber no ensaio cronometrado.

## Validação para o coordenador

Conferência estática executada: as quatro tabelas principais têm intervalos contínuos e somam 210, 210, 110 e 390 s; as referências de arquivo:linha acima existem; há cinco arquivos de migração, excluindo Designer e snapshot. A correção do pitch foi confrontada com o cadastro das migrações, os quatro IDs exigidos pelo firmware e as rotas de leitura e semeadura. Na correção espacial, uma checagem PowerShell confirmou os quatro sensores e as seis cadeiras do SQL contra as coordenadas e os marcadores de vulnerabilidade do exemplo existente; as referências documentais também foram verificadas. SQL e aplicação não foram executados. Os ensaios abaixo seguem pendentes.

- Repetir duas vezes a preparação de cada roteiro, com API e Wokwi parados antes de recriar seu banco descartável; cada comando deve terminar sem erro. O POST do ESP32 deve responder 201 antes de ligar o sensor. No pitch, repetir também o cadastro SQL: três inserções, quatro IDs ativos na consulta, sem exibir valores de autenticação. Depois de configurar o Wokwi, `Invoke-RestMethod 'http://localhost:5080/ambientes/sala-03/leituras'` deve mostrar os quatro IDs com origem dispositivo, 33 °C / 55 % e suspeita falsa; só então `Invoke-RestMethod -Method Post 'http://localhost:5080/demo/semear?ambienteId=sala-03'` deve responder 201. Sem publicação aceita nessa sala, esperar 409.
- No ensaio ESP32, consultar `Invoke-RestMethod 'http://localhost:5080/ambientes/sala-07/leituras'`: os passos da tomada devem entrar sem suspeita por salto. Não confundir com a primeira leitura da passagem de preparação.
- No pitch, antes de ligar o Wokwi, executar `Invoke-RestMethod 'http://localhost:5080/ambientes/sala-03/mapa' | ConvertTo-Json -Depth 6`: esperar dimensões 6 × 4 m, quatro sensores com rótulos e posições do SQL, seis cadeiras com 1 e 6 vulneráveis, todos sem leitura. Repetir a conferência após o segundo reset e preparo completo. Depois das publicações, repetir o GET: os dez pontos devem ter leitura e manter suas posições. Mostrar Perigo no desktop após Atualizar, registrar a ação ali e só mostrar o mural depois que estiver atualizado.
- Com a conexão do banco de ensaio definida, executar dentro de `api/`: `dotnet ef migrations list --project TermometroDaSala.Api`. Esperado: as cinco migrações da tabela aplicadas, sem pendências.
- Cronometrar os quatro vídeos: 3min30s, 3min30s, 1min50s e 6min30s planejados, todos abaixo dos respectivos limites.
