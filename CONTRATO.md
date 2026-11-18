# Contrato — Termômetro da Sala

Referência única de integração. Alterações passam por PR e aviso à equipe.
A entidade é `Ambiente`, com `tipo = "sala"`; na tela, “Sala”.

## 1. Relógio

Mensagens e banco usam UTC: `medidoEm` é inteiro em segundos Unix;
datas na API usam `DateTimeOffset.FromUnixTimeSeconds` e `DateTimeOffset`.
Hora local de São Paulo (UTC−3) só entra na tela, nos atributos hora/mês do
modelo e na projeção do dia da demonstração.
INMET entrega UTC: no notebook, `tz_localize("UTC").dt.tz_convert("America/Sao_Paulo")`;
no Windows, `TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")`.

Vetor: **15/01/2026, “1700 UTC” = 2026-01-15T17:00:00Z =
15/01/2026 14:00:00−03:00 = Unix 1768496400**.
Notebook e API devem testar esse mesmo caso.
ESP32 usa `time(nullptr)` após sincronização NTP; chama
`sntp_set_sync_interval(60000)` antes de `configTime(0, 0, ...)`.
Sem NTP, mantém os LEDs e não publica data inventada.

## 2. Leitura do sensor

MQTT a cada 10 s em `broker.hivemq.com:1883`, tópico
`fiap-gs/termometro-da-sala/leituras`, sem mensagem retida.
`POST /leituras` deve aplicar as mesmas validações do assinante MQTT.

```json
{"ambienteId":"sala-03","dispositivo":"esp32-sala-03","temperaturaC":33.4,"umidadePct":58,"medidoEm":1794331200,"assinatura":"2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0"}
```

Texto canônico: `ambienteId|dispositivo|temperaturaC|umidadePct|medidoEm`.
UTF-8, sem espaços, sem quebra de linha final; os IDs cadastrados não contêm
`|`, aspas ou caracteres de controle e o dispositivo pertence ao ambiente informado.

```text
sala-03|esp32-sala-03|33.4|58|1794331200
```

| Campo | ESP32 (C++) | API (C#) |
|---|---|---|
| Temperatura (número, 1 casa) | `snprintf(t, sizeof t, "%.1f", temp)`; o mesmo `t` no JSON e HMAC | `TemperaturaC.ToString("0.0", CultureInfo.InvariantCulture)` |
| Umidade (inteiro arredondado) | `lround(umidade)`, `%d` | `UmidadePct.ToString(CultureInfo.InvariantCulture)`, propriedade `int` |
| Instante (inteiro) | `%lld`, conversão de `time(nullptr)` para `long long` | `MedidoEm.ToString(CultureInfo.InvariantCulture)`, propriedade `long` |
| Separador | caractere `|` | igual |
| Assinatura | `mbedtls_md_hmac`, SHA256, 64 caracteres hex minúsculos | `HMACSHA256.HashData`, hex minúsculo; comparar bytes com `CryptographicOperations.FixedTimeEquals` |

Temperatura usa ponto decimal, nunca vírgula. A API valida JSON, tipos,
valores finitos e cadastro/vínculo antes de aceitar. Não arredonda silenciosamente
uma temperatura recebida com precisão diferente do contrato.

**Vetor público de teste:** chave `segredo-de-teste-sala-03`, linha acima,
HMAC-SHA256 **`2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0`**.
Reproduzir com `python ferramentas/vetor_hmac.py`. O ESP32 imprime o vetor ao
ligar; a API deve conferir o mesmo caso em teste unitário.

| Situação | Tratamento da API | Motivo no log |
|---|---|---|
| JSON/tipos/precisão inválidos, campos ausentes, dispositivo desconhecido ou vínculo errado | recusar | `leitura-invalida` |
| HMAC não confere ou hex inválido | recusar | `assinatura-invalida` |
| `medidoEm` ≤ último aceito do dispositivo | recusar | `reenvio` |
| Dispositivo com `ativo = false` | recusar | `dispositivo-revogado` |
| Mais de 600 s atrás ou 60 s à frente do servidor | recusar | `atrasada` |
| Fora de −10 a 60 °C ou 0 a 100%, inclusive não finitos | recusar | `fora-da-faixa` |
| Salto >5 °C ou sala >8 °C abaixo da rua quando rua >30 °C (a regra da rua só vale com leitura da rua medida no mesmo instante; hoje não se aplica: no Demo a rua é um dia reproduzido e fora dele nada grava a rua) | gravar suspeita, excluir da previsão e destacar no desktop | `suspeita` |

Plausibilidade é para equipes de 4–5; nas de 2–3 fica para depois. A ordem
estrita por dispositivo impede replay; a janela de tempo sozinha não impede.
A checagem de ordem e a gravação devem ser atômicas entre HTTP e MQTT.
Não registrar chave nos logs. A API recalcula o índice e é a fonte oficial.

O segredo nunca vai no JSON. `secrets.h` é ignorado; o exemplo contém só a chave
pública de teste. O Wokwi expõe os arquivos a quem tem o link: usar dispositivo
de demonstração, com chave própria, revogável (`Dispositivo.ativo/segredo`), e
revogá-lo após a avaliação. Na API, chave em variável de ambiente do pool IIS,
nunca em configuração versionada. NVS no aparelho real é evolução futura.
O broker público não dá confidencialidade nem impede inundação de mensagens.
Exceção didática: os quatro IDs da Sala 7 compartilham o valor público de
demonstração; o preparo do pitch também replica o valor da porta nos quatro IDs.
Revogação é individual, mas não há isolamento dos segredos nesses cenários.

## 3. Níveis de calor

Fórmula NOAA/Rothfusz, idêntica a `ferramentas/indice_calor.py`.
Com temperatura Celsius `C` e umidade relativa `rh` em porcentagem:

```text
t = C * 9 / 5 + 32
simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094)
se (simples + t) / 2 < 80: hi = simples
senão:
  hi = -42.379 + 2.04901523*t + 10.14333127*rh - 0.22475541*t*rh
       - 0.00683783*t*t - 0.05481717*rh*rh + 0.00122874*t*t*rh
       + 0.00085282*t*rh*rh - 0.00000199*t*t*rh*rh
  se rh < 13 e 80 <= t <= 112:
    hi -= ((13-rh)/4) * sqrt((17-abs(t-95))/17)
  senão se rh > 85 e 80 <= t <= 87:
    hi += ((rh-85)/10) * ((87-t)/5)
indiceC = round((hi-32)*5/9, 1)
```

Calcular com precisão dupla e arredondar a uma casa antes de classificar.
No ESP32, usar a temperatura e umidade já formatadas para a mensagem, para
que o cálculo local tenha as mesmas entradas da API. Conferir com tolerância
**0,1 °C**, sem mudar fórmula nem coeficientes.
Referência: https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml

| Temperatura °C | Umidade % | Índice °C | Código do nível |
|---|---|---|---|
| 24.0 | 40 | 23.5 | `normal` |
| 30.0 | 50 | 31.0 | `atencao` |
| 33.4 | 58 | 39.8 | `cuidado` |
| 38.0 | 70 | 62.5 | `perigo` |

| Índice °C (limites sem sobreposição) | Nível / código | LED | Ação sugerida para sala |
|---|---|---|---|
| <18 | Frio / `frio` | amarelo piscando (500 ms) | fechar janelas, agasalhar e oferecer cobertor; à noite, avisar o plantão e levar quem estiver no cômodo mais frio para outro |
| ≥18 e <27 | Normal / `normal` | verde | nenhuma |
| ≥27 e <32 | Atenção / `atencao` | amarelo | liberar água, abrir janelas e ligar ventiladores |
| ≥32 e <41 | Cuidado extremo / `cuidado` | vermelho fixo | pausa, troca de sala ou adiar Educação Física |
| ≥41 | Perigo / `perigo` | vermelho piscando (500 ms ligado/500 ms apagado) | tirar a turma da sala |

Ações sugeridas ficam na tabela `NivelAcao` do banco. O LED é calculado no
ESP32 mesmo sem internet. Sensor inválido apaga LEDs e não publica; apagado
não significa “Normal”. O índice é uma aproximação didática, não diagnóstico
médico nem medição IBUTG.

## 4. Previsão da sala

Rua atual: dia quente do CSV INMET reproduzido. Rua daqui a 2 h: o modelo
treinado com dados reais (`ml/modelo.json`, árvore de regressão), aplicado pela
API com os atributos do notebook: temperatura, umidade, hora local, mês, variação
da última hora e índice de calor atual (`indice_agora`). Fora do Demo, a entrada é a rua gravada em `LeiturasRua`. No Demo,
a entrada é a hora atual do CSV (temperatura, umidade e variação desde a hora
anterior do CSV), com o mês de hoje; o `indiceRuaPrevisto` que app, desktop e
painel mostram é a saída do modelo. Sem `modelo.json` válido, o Demo usa a hora do
CSV duas horas à frente e fora dele a rua de agora se repete. O teste
`PrevisorModeloTestes.DemoAplicaOModeloAoDiaDoCsvEDaOMesmoValor` confere que o Demo
e o previsor do banco devolvem o mesmo valor para o vetor da seção 6 do notebook
(15/01/2026 14h, 30 °C, 50%, +1,5 °C na última hora: 27,16 °C). Busca ao vivo A701
e reserva Open-Meteo são evolução opcional.

O modelo de rua é compartilhado. O ajuste é calculado separadamente por ambiente,
sem coeficientes escolhidos pelo tipo de escola, idade ou ocupação:

1. Nas últimas 3 h, excluir leituras futuras, suspeitas e valores não finitos.
2. Selecionar a última leitura de cada sensor em cada minuto UTC; em empate,
   usar o maior ID. Mais envios do mesmo sensor não aumentam seu peso.
3. Parear cada leitura com a última observação externa anterior ou igual,
   com no máximo 60 minutos de idade. Empates externos também usam o maior ID.
4. Calcular a média das diferenças sala − rua por minuto e depois a média
   dos minutos. Exigir três minutos distintos e último par com até 15 minutos.

Exemplo: diferenças +4 em três minutos dão delta +4; rua prevista 34 + delta 4
= sala 38 °C de índice. Os limites 3 h/60 min/15 min e o mínimo de minutos são
regras operacionais iniciais, não uma garantia estatística ou de segurança.
Quatro sensores simultâneos não formam três minutos de histórico. A mesma
observação externa pode servir a vários minutos, sem virar novas medições.
Sem histórico suficiente, `deltaSala` e `indiceSalaPrevisto` são `null`;
mostrar só a rua e “Histórico insuficiente para ajustar a sala”. A hipótese
do ajuste é que a diferença recente se mantenha até o alvo; precisa de validação.

Fora do Demo, a observação externa usada na previsão também expira em 60 minutos.
`previstoPara` é o horário dessa observação **mais 2 h**, preservando o horizonte
do treino: consultando às 12h35 uma observação de 12h, o alvo é 14h, não 14h35.
As interfaces exibem o horário recebido. No Demo e na persistência sem rua,
o alvo continua sendo agora +2 h. Não há mudança nos campos do JSON.

`GET /ambientes/{id}/previsao` responde:

- 200 com a previsão (exemplo abaixo);
- 404 quando o ambiente não existe;
- 503 com `{"motivo":"sem-dados-para-prever"}` quando não há previsão da rua
  nem leitura válida da sala nos últimos 15 minutos. Não é queda da API: app e desktop
  mostram “sem dados para prever” no lugar da previsão, sem tela de erro.

```json
{"ambienteId":"sala-03","previstoPara":"2026-11-06T19:10:00+00:00","indiceRuaPrevisto":34,"deltaSala":4,"indiceSalaPrevisto":38,"nivelPrevisto":"cuidado","semLeiturasRecentes":false,"demonstracao":false,"ruaDisponivel":true}
```

Sem previsão da rua (fora do Demo nada grava a rua), `ruaDisponivel` vem
`false`: a sala persiste a média da última leitura válida de cada sensor nos
últimos 15 minutos, com peso igual por sensor. `indiceSalaPrevisto` é essa média,
`deltaSala` é `null` e `indiceRuaPrevisto`
é `0`, que não vai para a tela. App, desktop e painel avisam que a previsão
usa a média dos sensores recentes porque a previsão da rua não está disponível.
Uma leitura suspeita aparece em “agora”, mas não entra na previsão.
Cada previsão devolvida com 200 é gravada na tabela `Previsoes` (com `CalculadaEm`),
para comparar depois o previsto com o que a sala mediu. A rota é anônima e painel,
app e desktop a consultam a cada 30 s, então grava no máximo uma por sala a cada
10 min; as consultas seguintes só leem (teste `PrevisaoServidaEntraNoHistorico`).

`ml/salas.py` reproduz o modelo compartilhado e a regra de ajuste. Os vetores
`ml/casos-ajuste-salas.json` são executados por Python e C#, incluindo dados
ausentes, isolamento, frequências, limites temporais e arredondamento.
Ocupação em watts permanece um metadado experimental; não é somada ao índice.
O cadastro e a coleta dos fatores adicionais continuam planejados. O sistema
ainda não guarda o instante de recebimento de cada leitura: uma análise histórica
pode excluir medições futuras, mas não provar disponibilidade à época quando
houve transmissão atrasada.

## 5. Demonstração

Só em `ASPNETCORE_ENVIRONMENT=Demo`; em Production, `/demo/semear` deve retornar
404. O dia quente do INMET é projetado no dia atual pela hora local: 17 UTC
do CSV → hoje às 14h em São Paulo. Gravar entre 12h e 17h.

No pitch, ajustar os quatro DHT22 a 33 °C/55% antes de ligar o Wokwi (o diagrama inicia janela a 35 °C/60% e demais a 24 °C/60%). Seguir o banco descartável e o cadastro da Sala 3 em `arquitetura/PITCH.md`. Então chamar
`POST /demo/semear?ambienteId=sala-03`: sem leitura ao vivo, 409 “ligue o sensor
primeiro”. A rota não é mensagem de dispositivo e não usa HMAC. Grava histórico
com `origem = "simulada"` antes da primeira leitura ao vivo; horários e valores usam referências distintas, descritas abaixo.

`python demo/gerador.py` gera `demo/leituras-simuladas.csv`, com colunas
`minutos_antes,temperaturaC,umidadePct`, intervalos de 10 min, de 180 até 0.
O código descarta o ponto 0 e conserva somente `0 < minutos_antes <= 180`,
ordenados do maior para o menor. A âncora dos valores é o último dessa lista
(o menor minuto positivo: 10 no CSV atual), comparado à última leitura ao vivo
do ambiente. Insere os 18 pontos com esse deslocamento, limitando temperatura
a −10–60 °C e umidade a 0–100%. O horário usa a primeira leitura ao vivo:
instante do ponto = primeira leitura ao vivo − minutos_antes × 60.
Limite conhecido: valor e horário usam leituras ao vivo distintas quando há mais
de uma; a curva não é ancorada no ponto zero nem prevê individualmente cada sensor.
As simuladas são atribuídas ao primeiro dispositivo encontrado no ambiente.
A curva usa face oeste/andar 2, atraso de 1 h e ruído com semente 2026; não usa
delta. `gerar(face, andar)` pode ser reutilizado no notebook; não serve para
treinar o modelo da rua. São hipóteses sintéticas, sem calibração real.

No pitch, subir os quatro de 33 para 37 °C/55% e parar: já é Perigo. No Physical,
seguir 24 → 29 → 33 → 37 °C e as umidades de `esp32/ROTEIRO-VIDEO.md`.
Esperar uma publicação (10 s) entre passos e conferir a aceitação pela API.
Para evidenciar plausibilidade, salto de 12 °C vira “suspeita” quando essa regra
estiver habilitada. Enquanto houver leitura simulada na janela, app e desktop
exibem “DEMONSTRAÇÃO”. Wokwi, histórico semeado e ocorrências da gravação são
simulados; a rua vem de dado real histórico reproduzido.

## 6. Salas, ocorrências e níveis

O ID de ambiente é exatamente `sala-` seguido por dois dígitos ASCII (`0`–`9`),
sem espaços ou quebras de linha. A descrição de `POST /acoes` deve ter de 5 a
300 caracteres após remover espaços nas pontas; é esse texto aparado que fica gravado.

`GET /ambientes` e `GET /ambientes/{id}` incluem `dispositivoId` e
`sensorRevogado`. Ambos descrevem o primeiro dispositivo da sala em ordem
crescente de ID: `sensorRevogado` é `true` quando esse dispositivo está inativo.
Sem dispositivo, os valores são `null` e `false`, respectivamente. A revogação
de outro sensor da mesma sala não muda esse indicador.

Escritas da coordenação levam o cabeçalho `X-Chave-Coordenacao`; sem ele, ou com
valor errado, 401 `{"motivo":"chave-invalida"}` e nada é gravado. Recusas 400 da
API levam `{"motivo":"..."}`; as de validação do corpo vêm no formato padrão do
ASP.NET Core (`errors`). Toda resposta leva `X-Content-Type-Options: nosniff`; em
Production, HTTPS com `Strict-Transport-Security`.

| Rota | Chave | Respostas |
|---|---|---|
| `POST /ambientes` | sim | 201 com `Location: /ambientes/{id}`; 400; 409 `ambiente-existente` |
| `GET /ambientes/{id}` | não | 200 com o mesmo objeto de cada item de `GET /ambientes`; 404 |
| `PUT /ambientes/{id}` | sim | Corpo igual ao do POST, com o mesmo `id` da rota. 200 `{"id":...}`; 400 `id-divergente`; 404 |
| `DELETE /ambientes/{id}` | sim | 204, levando junto dispositivos, ocorrências (e as fotos delas no disco), ações e previsões da sala; 409 `ambiente-com-leituras` (o histórico não se apaga); 404 |
| `GET /ambientes/{id}/ocorrencias` | não | 200 com `[{"id","ambienteId","descricao","criadaEm"}]`, da mais recente à mais antiga, sem a foto; 404 sala inexistente |
| `GET /ocorrencias/{id}/foto` | sim | 200 `image/jpeg`; 401 sem chave; 404. A foto pode mostrar pessoas, então só a coordenação abre |
| `GET /niveis` | não | 200 com as 5 faixas (Frio, Normal, Atenção, Cuidado extremo e Perigo) da tabela `NiveisAcao`: `nivel`, `nome`, `indiceMin`, `acaoSugerida` (seção 3) |
| `POST /acoes` | sim | 201 com `Location: /acoes?dia=AAAA-MM-DD` (dia de São Paulo); 400 `nivel` ou `descricao`; 404 |

O app do professor lista as ocorrências da sala e não abre fotos; o desktop da
coordenação não exibe fotos.
