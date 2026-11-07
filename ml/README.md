# Termômetro da Sala — Machine Learning

[![Abrir no Colab](https://colab.research.google.com/assets/colab-badge.svg)](https://colab.research.google.com/github/sbaecker2005/termometro-da-sala/blob/main/ml/termometro_da_sala.ipynb)

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

Integrantes: Samuel Baecker — RM [preencher]

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
| `requirements.txt` | Versões usadas na execução local (Python 3.11) |

## Decisões

- **Dados reais e públicos.** O briefing aceita bases públicas. O INMET mede a rua; a sala entra pelo sensor ESP32, pela regra do delta.
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
- **Revisão motivada pela auditoria:** incluímos `indice_agora` para responder ao baixo recall. Os limites 27/32/41 °C permanecem os do contrato; nenhum limiar foi escolhido no teste. A revisão combina atributo adicional e grade ampliada, sem atribuir todo o ganho a um único fator. 2025 já havia sido visto na auditoria: o resultado abaixo é regressão temporal; confirmação independente requer dados novos.

## Resultado no teste (2025)

| Modelo | MAE (°C) | RMSE (°C) | R² | F1 macro (níveis presentes) |
|---|---:|---:|---:|---:|
| Baseline "daqui a 2 h = agora" | 1,442 | 2,021 | 0,834 | 0,667 |
| Regressão linear (com índice atual) | 1,050 | 1,448 | 0,915 | 0,757 |
| Floresta (somente comparação) | 0,822 | 1,220 | 0,939 | 0,728 |
| Árvore anterior (8, folha 30) | 0,958 | 1,364 | 0,924 | 0,660 |
| **Árvore exportada (12, folha 24)** | **0,909** | **1,324** | **0,929** | **0,730** |

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

## Previsão em uso

A seção 6 do notebook repete o vetor do teste da API e mostra a previsão do jeito que chega à coordenação:

| Passo | Valor |
|---|---|
| Agora | 15/01/2026 às 17:00 UTC = 14h em São Paulo; rua a 30 °C, 50% de umidade, +1,5 °C na última hora |
| Índice da rua agora | 31,0 °C (atenção) |
| Rua prevista para +2 h (`modelo.json`) | 27,16 °C, o mesmo valor que a API calcula no teste |
| Delta da sala 3 | +4 °C (valor de exemplo do modo demonstração, não medição) |
| Sala prevista para +2 h | 31,16 °C, nível **atenção** |
| Ação sugerida (`CONTRATO.md`) | liberar água, abrir janelas e ligar ventiladores |

Em produção, o delta é a média da diferença sala − rua nas últimas 3 h, medida pelo ESP32.

O aviso com 2 h de antecedência apoia a coordenação ao hidratar, ventilar ou realocar a turma, ligado ao **ODS 3.d** (alerta precoce e redução de riscos à saúde). O indicador de aulas em Perigo sem ação ainda não foi medido; faltam dados reais da sala e validação do delta. A previsão não substitui a leitura atual nem os procedimentos da escola.

## Como rodar

O estudo **Mapa térmico da sala** está na seção 8 do mesmo notebook. Usa somente
dados simulados da Sala 7 (14 dias por estação), IDW, agrupamento por perfil horário,
previsão por ponto e precauções. O código compartilhado está em `mapa.py`, o caso
calculado à mão em `caso-idw.json` e os testes em `test_mapa.py`.
Os resultados e limites estão em [MAPA-TERMICO-ML.md](../docs/MAPA-TERMICO-ML.md).
`modelo_pontos.json` guarda a árvore original congelada e uma correção linear;
seu formato separado não substitui o `modelo.json` consumido pela aplicação.
O notebook exporta `dados/sala7-simulada.csv`, `grupos-pontos.csv`,
`silhuetas-pontos.csv` e `metricas-pontos.csv` para reproduzir a análise.

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
Na API, seis casos pela rota HTTP conferem o nível servido e gravado após somar
o delta e arredondar a sala para uma casa, dos dois lados dos limites 27/32/41 °C.

**No Colab:** clique em "Abrir no Colab" no topo e use Ambiente de execução → Executar tudo. Não instale o `requirements.txt` no Colab: as versões fixadas (numpy 1.24.3, pandas 2.0.3, scikit-learn 1.3.0) são as da execução local em Python 3.11 e não têm pacote para o Python do Colab. As bibliotecas que já vêm no Colab bastam. Sem a pasta `ml` ao lado, o notebook lê o CSV de `https://raw.githubusercontent.com/sbaecker2005/termometro-da-sala/main/ml/dados/inmet_a701.csv`. O RMSE é calculado com `np.sqrt(mean_squared_error(...))`, que funciona nas versões antigas e novas do scikit-learn.

**Localmente**, na pasta `ml`, com Python 3.11. Execução desta revisão: **3.11.9**, numpy 1.24.3, pandas 2.0.3 e scikit-learn 1.3.0 (sem novos pacotes):

```powershell
python -m pip install -r requirements.txt
python gerar_notebook.py
python -c "from pathlib import Path; import nbformat; from nbclient import NotebookClient; p = Path('termometro_da_sala.ipynb'); nb = nbformat.read(p, as_version=4); NotebookClient(nb, timeout=600, kernel_name='python3').execute(); nbformat.write(nb, p)"
```

O `gerar_notebook.py` monta as células e o comando seguinte executa todas, gravando as saídas no `.ipynb`. A execução regrava `modelo.json`; com os mesmos dados e versões ele sai idêntico ao copiado em `api/TermometroDaSala.Api/modelo/modelo.json`.
