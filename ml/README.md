# Termômetro da Sala — Machine Learning

[![Abrir no Colab](https://colab.research.google.com/assets/colab-badge.svg)](https://colab.research.google.com/github/sbaecker2005/termometro-da-sala/blob/main/ml/termometro_da_sala.ipynb)

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo; a entrevista foi no CRAS Santo Amaro. O piloto em escola ainda não tem unidade definida |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

Integrantes: Samuel David Baecker — RM 559269; Lucas de Almeida Villar — RM 560005; Guilherme Augusto Caseiro — RM 559765

## Problema e dados

O mesmo sensor pode avisar calor no verão (escolas) e frio no inverno (dormitórios e abrigos); o notebook e as métricas abaixo tratam do calor. O problema: nas tardes quentes a sala passa do limite seguro sem que ninguém meça. O modelo prevê o índice de calor da rua (fórmula da NOAA) daqui a 2 horas, e a API soma a diferença medida pelo sensor da sala.

Dados: INMET, estação automática A701 (São Paulo, Mirante de Santana), horários de 2022 a 2025, hora em UTC convertida para São Paulo. Arquivo `dados/inmet_a701.csv`, extraído dos zips anuais por `extrair_a701.py` ([portal.inmet.gov.br/dadoshistoricos](https://portal.inmet.gov.br/dadoshistoricos)). Divisão por tempo: treino 2022–2023, validação 2024, teste 2025.

## Resultados

### Teste de 2025

| Modelo | MAE (°C) | RMSE (°C) | R² | F1 macro (níveis presentes) |
|---|---:|---:|---:|---:|
| Baseline "daqui a 2 h = agora" | 1,442 | 2,021 | 0,834 | 0,693 |
| Regressão linear (com índice atual) | 1,050 | 1,448 | 0,915 | 0,778 |
| Floresta (somente comparação) | 0,822 | 1,220 | 0,939 | 0,767 |
| Árvore anterior (8, folha 30) | 0,958 | 1,364 | 0,924 | 0,710 |
| **Árvore exportada (12, folha 24)** | **0,909** | **1,324** | **0,929** | **0,764** |

O nível "perigo" (índice ≥ 41 °C) não aparece na rua da A701 entre 2022 e 2025, por isso o F1 macro é a média dos níveis presentes. Dentro de uma sala sem ventilação o índice passa da rua: é o que o delta mede.

**Alerta antes e depois:** índice previsto ≥ 32 °C, incluindo cuidado e perigo. A avaliação é da rua, sem delta da sala.

| Período | Regra | TP | FP | FN | Precisão | Recall |
|---|---|---:|---:|---:|---:|---:|
| 2024 | Persistência | 37 | 57 | 57 | 39,4% | 39,4% |
| 2024 | Árvore anterior | 8 | 8 | 86 | 50,0% | 8,5% |
| 2024 | Árvore revisada | 32 | 31 | 62 | 50,8% | 34,0% |
| 2025 | Persistência | 18 | 29 | 30 | 38,3% | 37,5% |
| 2025 | Árvore anterior | 7 | 5 | 41 | 58,3% | 14,6% |
| 2025 | Árvore revisada | 19 | 17 | 29 | 52,8% | 39,6% |

**Limite crítico:** o treino tem só 104 alvos cuidado (0,605%) e nenhum perigo. A revisão detecta 19 das 48 horas de cuidado em 2025, ante 7 da árvore anterior e 18 da persistência. A precisão cai de 58,3% para 52,8% e os falsos positivos sobem de 5 para 17; ainda perde 29 horas críticas (27 como atenção, 2 como normal). Em 2024, o recall continua abaixo da persistência. Não há garantia de segurança nem validação dentro da sala. O `classification_report` inclui perigo com suporte zero; sua macro difere da tabela de regressão, que usa só níveis presentes.

O notebook mostra previsto contra real na semana mais quente de 2025 e resíduos: média −0,032 °C, com 90% dos erros entre −2,11 e +2,08 °C.

### Previsão em uso

A seção 6 do notebook repete o vetor do teste da API e mostra a previsão do jeito que chega à coordenação:

| Passo | Valor |
|---|---|
| Agora | 15/01/2026 às 17:00 UTC = 14h em São Paulo; rua a 30 °C, 50% de umidade, +1,5 °C na última hora |
| Índice da rua agora | 31,0 °C (atenção) |
| Rua prevista para +2 h (`modelo.json`) | 27,16 °C, o mesmo valor que a API calcula no teste |
| Delta da sala 3 | +4 °C (valor de exemplo do modo demonstração, não medição) |
| Sala prevista para +2 h | 31,16 °C, nível **atenção** |
| Ação sugerida (`CONTRATO.md`) | liberar água, abrir janelas e ligar ventiladores |

Na execução integrada, o delta usa o histórico recente da sala e o pareamento
externo descritos em “Modelo geral com ajuste por sala”.

O aviso com 2 h de antecedência apoia a coordenação ao hidratar, ventilar ou realocar a turma, ligado ao **ODS 3.d** (alerta precoce e redução de riscos à saúde). O indicador de aulas em Perigo sem ação ainda não foi medido; faltam dados reais da sala e validação do delta. A previsão não substitui a leitura atual nem os procedimentos da escola.

## Decisões

- **Dados reais e públicos.** O briefing aceita bases públicas. O INMET mede a rua; a sala entra pelo sensor ESP32, pela regra do delta.
- **Níveis:** o classificador inclui Frio abaixo de 18 °C (mínimo interno recomendado pela OMS para moradias), além dos limites de calor 27/32/41 °C.
- **Alvo:** índice de calor (fórmula da NOAA, a mesma do ESP32 e da API) daqui a 2 horas, regressão.
- **Atributos só de produção:** temperatura, umidade, hora local e mês (em seno e cosseno) variação da última hora e `indice_agora`, calculado pela fórmula do contrato. Nada que a API não tenha no boletim horário.
- **Hora em UTC convertida para São Paulo** antes de tudo, com o vetor de teste do contrato (15/01/2026 às 1700 UTC = 14h).
- **Preparação e análise:** origem e colunas do INMET documentadas no notebook; calendário UTC com 0/212/19/47 horas ausentes ou descartadas em 2022/2023/2024/2025. São 92 buracos, o maior com 54 h. O CSV limpo não permite separar falta de registro de descarte. A montagem dos atributos/alvo remove 205 linhas sem vizinhos; outras 4 são excluídas nas fronteiras entre conjuntos. Há distribuição do alvo e dos níveis e matriz de correlação no treino.
- **Divisão por tempo:** treino 2022–2023 (17.192 exemplos), validação 2024 (8.740), teste 2025 (8.645, usado só na avaliação final). O alvo +2 h não cruza as fronteiras.
- **Busca:** 410 configurações, `max_depth` em `[4,5,6,7,8,9,10,11,12,None]` e `min_samples_leaf` em todos os inteiros de 10 a 50. Cada árvore treina até 2023 e é medida em 2024; MAE não arredondado decide, RMSE desempata. A tabela completa está no notebook e em `grade-parametros.csv`.
- **Modelo:** profundidade 12, folha mínima 24, MAE 0,914540 °C e RMSE 1,342734 °C em 2024. A árvore anterior tinha MAE 0,981 °C; a linear revisada 1,089 °C e a persistência 1,479 °C. Não houve retreino.
- **Ensemble só para comparação:** floresta de 100 árvores, profundidade 12, folha 10, semente 42, MAE 0,834 °C em 2024. Ela vence em regressão, mas a exportação continua uma árvore única, conforme o contrato da API, que percorre o JSON em C# sem Python em produção. A escolha é a melhor árvore da grade, não o melhor regressor entre todas as famílias.
- **Formato:** mantidos `tipo: arvore`, `atributos`, `raiz` e os nós existentes. O atributo `indice_agora` entra no ponto compartilhado `ModeloRua.Atributos` do Demo e do banco; a validação de nomes passa a aceitá-lo. As cópias do JSON em `ml/` e na API precisam ser publicadas junto com esse código. O notebook confere 200 exemplos contra scikit-learn (diferença máxima < 0,00005 °C), exporta suas entradas e previsões em `paridade-api.json` e calcula os três vetores de `PrevisorModeloTestes`. Os testes da API verificam igualdade byte a byte das cópias de `modelo.json` e paridade do avaliador C# com as 200 previsões do scikit-learn (tolerância de 0,00005 °C).
- **Interpretação:** coeficientes padronizados da linear e redução de impureza da árvore; `indice_agora` concentra 88,0% da importância. Ele deriva de temperatura e umidade, portanto essa importância não mede causalidade nem efeito isolado.
- **Índice atual e limite da avaliação:** incluímos `indice_agora` para responder ao baixo recall. Os limites 27/32/41 °C permanecem os do contrato; nenhum limiar foi escolhido no teste. A revisão combina atributo adicional e grade ampliada, sem atribuir todo o ganho a um único fator. 2025 já havia sido examinado quando o atributo foi escolhido: o resultado da seção Resultados é uma validação temporal, e a confirmação independente requer dados novos.

## Arquivos

| Arquivo | O que é |
|---|---|
| `termometro_da_sala.ipynb` | O notebook, já executado (dados, índice de calor, modelos, métricas, exportação, previsão em uso) |
| `dados/inmet_a701.csv` | Dataset: INMET, estação automática A701 (São Paulo, Mirante de Santana), dados horários de 2022 a 2025, hora em UTC |
| `extrair_a701.py` | Extrai a A701 dos zips anuais do INMET ([portal.inmet.gov.br/dadoshistoricos](https://portal.inmet.gov.br/dadoshistoricos)) |
| `gerar_notebook.py` | Monta o notebook (as células ficam legíveis no git) |
| `grade-parametros.csv` | MAE e RMSE de todas as 410 configurações na validação de 2024 |
| `alertas.csv` | Precisão, recall e TP/FP/FN antes e depois em 2024 e 2025 |
| `modelo.json` | O modelo exportado, que a API em C# carrega |
| `paridade-api.json` | 200 entradas de 2025 e previsões do scikit-learn, exportadas pelo notebook para o teste da API |
| `test_alertas.py` | Teste offline de precisão/recall antes e depois, com o CSV real e o JSON exportado |
| `ocupantes.py` | Estimador experimental em watts por grupos, com demonstração do 3º ano; separado da previsão atual |
| `test_ocupantes.py` | Cálculo de referência, presença variável, dados ausentes e validação das entradas do estimador |
| `salas.py` | Modelo de rua compartilhado com ajuste observacional isolado por sala e demonstração do terceirão |
| `test_salas.py` e `casos-ajuste-salas.json` | Casos temporais, isolamento, paridade com C# e cálculo sem retreinar o modelo |
| `requirements.txt` | Versões usadas na execução local (Python 3.11) |

## Como rodar

```powershell
python -m pytest ml -o cache_dir=ml/.pytest_cache
```

**Regressão offline**, a partir da raiz, no ambiente Python 3.11 já usado pelo notebook:

```powershell
python -m unittest discover -s ml -p test_alertas.py
dotnet test api/TermometroDaSala.Api.Tests/TermometroDaSala.Api.Tests.csproj --no-restore
```

O teste Python reutiliza as células de preparação e divisão temporal do notebook,
treina a árvore anterior e percorre o JSON revisado em todos os exemplos de 2024/2025.
Confere TP, FP, FN, precisão e recall das seis linhas de `alertas.csv`, sem executar
a grade nem regravar o notebook. Essas métricas usam o índice da rua sem delta.
Na API, oito casos pela rota HTTP conferem o nível servido e gravado após somar
o delta e arredondar a sala para uma casa, dos dois lados dos limites 18/27/32/41 °C.

**No Colab:** clique em "Abrir no Colab" no topo e use Ambiente de execução → Executar tudo. Não instale o `requirements.txt` no Colab: as versões fixadas (numpy 1.24.3, pandas 2.0.3, scikit-learn 1.3.0) são as da execução local em Python 3.11 e não têm pacote para o Python do Colab. As bibliotecas que já vêm no Colab bastam. Sem a pasta `ml` ao lado, o notebook lê o CSV de `https://raw.githubusercontent.com/sbaecker2005/termometro-da-sala/main/ml/dados/inmet_a701.csv`. O RMSE é calculado com `np.sqrt(mean_squared_error(...))`, que funciona nas versões antigas e novas do scikit-learn.

**Localmente**, na pasta `ml`, com Python 3.11. Execução desta revisão: **3.11.9**, numpy 1.24.3, pandas 2.0.3 e scikit-learn 1.3.0 (sem novos pacotes):

```powershell
python -m pip install -r requirements.txt
python gerar_notebook.py
python -c "from pathlib import Path; import nbformat; from nbclient import NotebookClient; p = Path('termometro_da_sala.ipynb'); nb = nbformat.read(p, as_version=4); NotebookClient(nb, timeout=600, kernel_name='python3').execute(); nbformat.write(nb, p)"
```

O `gerar_notebook.py` monta as células e o comando seguinte executa todas, gravando as saídas no `.ipynb`. A execução regrava `modelo.json`; mantenha a cópia em `api/TermometroDaSala.Api/modelo/modelo.json` idêntica, inclusive as métricas por nível.

Na execução de 07/11/2026, o Windows negou a configuração de permissões do arquivo
de conexão do Jupyter. Foi usado o kernel em processo já instalado
(`ipykernel.inprocess.manager.InProcessKernelManager`), com histórico em memória
e gráficos inline, preservando os eventos de saída das 19 células na ordem.

## Estudos complementares

Os estudos abaixo não fazem parte da previsão servida pela API; são extensões do trabalho.

### V1 de ocupação e referência de calor humano

A demonstração usa **somente uma turma do 3º ano do ensino médio, 20 alunos
aproximadamente de 17–18 anos e 1 professor**. Esse perfil é hipotético.
CRAS, escolas públicas e privadas, do berçário ao ensino médio, continuam como
público-alvo; o exemplo não valida todos esses públicos. O piloto em escola ainda não tem unidade confirmada. Cada sala precisa de seu próprio
perfil e histórico. Um modelo independente por sala só deve ser treinado quando
houver dados suficientes para avaliação temporal; hoje o modelo de rua é
compartilhado e a API calcula o delta de cada ambiente.

**Referência operacional:** EnergyPlus 26.2, *Input Output Reference*, tabela 1.30,
coluna W/pessoa: **108 W sentado em repouso e 126 W em pé relaxado**.
São valores de referência adulta, baseados em superfície corporal de 1,8 m².
O ganho é total, incluindo calor convectivo, radiante e latente.
[Fonte primária e valores](https://energyplus.readthedocs.io/en/v26.2.0/guides/input-output-reference/1.14-group-internal-gains-people-lights-other-internal-zone-equipment.html).
Usamos diretamente essa coluna; não recalculamos pelos valores arredondados de met.

O cálculo do cenário é `20 × 108 + 1 × 126 = 2286 W` (**2,286 kW**).
É uma hipótese adulta equivalente para demonstrar o método, não uma medição nem
uma calibração dos adolescentes. A idade não escolhe watts automaticamente.
O berçário e outros perfis não recebem esses coeficientes por padrão.

`estimar_ocupantes(grupos)` exige rótulo do grupo, perfil etário agregado, atividade,
quantidade presente e, quando disponível, potência por pessoa com sua referência.
Uma lista vazia representa sala explicitamente vazia. Presença desconhecida é
recusada. Grupo presente sem potência deixa o total como `None`, nunca zero.
Não são necessários nomes, temperatura corporal, peso ou altura individuais.

```powershell
python -B -m ml.ocupantes
python -B -m unittest discover -s ml -p test_ocupantes.py -v
```

Execute da raiz do projeto; o módulo usa só a biblioteca padrão. A saída JSON
identifica `origem: estimativa`, `validado_em_campo: false`, os grupos e a fonte.
O coeficiente se refere à atividade registrada; falar ou caminhar não é a mesma
atividade do professor em pé relaxado adotada no exemplo.

**Integração técnica:** este primeiro módulo funciona offline. Ele ainda não é
atributo de `modelo.json`, não altera a API e não soma watts a graus Celsius.
O INMET não contém ocupação das salas: treinar com os novos fatores exige coleta
interna, com horário da observação e do registro, ambiente, grupos presentes,
atividade e referência utilizada. Atualizar presença/atividade por evento, conforme
o plano V1. Coeficientes, registros ausentes e mudanças de perfil devem continuar
identificados na preparação do dataset. Só promover o fator ao modelo após comparar
o resultado com a previsão atual em períodos posteriores e separados do treino.
Não foram atribuídas frações arbitrárias de calor sensível/latente, intervalos de
confiança, nem efeitos em °C. Computadores e eletrônicos continuam fora da V1.

### Modelo geral com ajuste por sala

`salas.py` reutiliza a árvore de `modelo.json`, o avaliador de `mapa.py` e o cálculo
do índice de calor de `ferramentas/indice_calor.py`. A API aplica a mesma regra de
ajuste; nenhuma árvore por idade, escola ou turma foi treinada nesta etapa.

- Histórico de até três horas, isolado por `ambiente_id`, sem futuros ou suspeitas.
- Última leitura por sensor/minuto UTC, com desempate pelo maior `id`.
- Observação externa anterior ou igual com no máximo 60 minutos, nunca a posterior.
- Média das diferenças por minuto entre sensores; depois, média entre minutos.
  Três minutos distintos e último par com até 15 minutos são o mínimo operacional.
- Sem rua disponível, persistência da média dos sensores recentes (15 minutos),
  com igual peso por sensor; sem dados suficientes, a ausência é explícita.

Os limites acima são escolhas do projeto, não uma calibração comprovada. A média
não representa o ponto mais quente da sala; o mapa continua mostrando diferenças
espaciais. Reutilizar a rua horária não cria observações independentes.

O alvo é **duas horas após a observação externa usada**. Se a consulta ocorre
30 minutos depois, restam 90 minutos até o alvo. `base_rua_em` e `previsto_para`
explicam isso na saída Python, e `previstoPara` informa o alvo no contrato da API.
O mapa +1 h interpola proporcionalmente ao horizonte restante, sem extrapolá-lo.

```powershell
python -B -m ml.salas
python -B -m unittest discover -s ml -p test_salas.py -v
```

Executar da raiz com o ambiente de `requirements.txt`, já utilizado pelo estudo.
A demonstração tem 4 sensores em 3 minutos, dados internos **simulados**, perfil
do 3º ano e 20 alunos +1 professor. Ela informa 2286 W separadamente e usa o modelo
exportado sem alterá-lo. Mudar a ocupação não muda essa previsão enquanto não
houver dados internos para treinar e validar esse efeito.

Entrada de `ajuste_por_sala`: lista de leituras com `ambiente_id`, `sensor`,
`instante` com fuso, `indice`, `suspeita` opcional e `id` para desempates;
lista externa com `instante`, `indice` e `id`. `prever_sala` também exige
`temperatura_c` e `umidade_pct` externos. Sem `id`, a ordem recebida desempata.
Campos opcionais de temperatura/umidade inválidos são descartados. Para exportar
leituras da API, prefira JSON: o CSV atual não contém a identificação do sensor.

Os vetores compartilhados são sintéticos, não métricas de acurácia. A conferência
temporal considera o instante medido; ainda não existe histórico de recebimento
para auditar atrasos de transmissão. A próxima coleta deve registrar ambos e
buscar o alvo pelo timestamp exato, sem confundir duas linhas com duas horas.
As métricas de 2025 da seção Resultados continuam referentes apenas ao modelo externo original.

### Mapa térmico da sala

Está na seção 8 do mesmo notebook. Usa somente
dados simulados da Sala 7 (14 dias por estação), IDW, agrupamento por perfil horário,
previsão por ponto e precauções. O código compartilhado está em `mapa.py`, o caso
calculado à mão em `caso-idw.json` e os testes em `test_mapa.py`.
Os resultados e limites estão em [MAPA-TERMICO-ML.md](../docs/MAPA-TERMICO-ML.md).
`modelo_pontos.json` guarda a árvore original congelada e uma correção linear;
seu formato separado não substitui o `modelo.json` consumido pela aplicação.
O estudo por ponto estima +2 h em dados simulados e não é servido pela API.
A API serve previsão do ambiente em +2 h e mapa em +1 h por metade da variação
do índice do ambiente, somada à temperatura e ao índice dos sensores recentes
(ver `docs/MAPA-TERMICO.md`); não carrega `modelo_pontos.json`.
O notebook exporta `dados/sala7-simulada.csv`, `grupos-pontos.csv`,
`silhuetas-pontos.csv` e `metricas-pontos.csv` para reproduzir a análise.
