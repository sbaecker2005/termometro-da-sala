# Termômetro da Sala — Machine Learning

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
| `termometro_da_sala.ipynb` | O notebook, já executado (dados, índice de calor, modelos, métricas, exportação) |
| `dados/inmet_a701.csv` | Dataset: INMET, estação automática A701 (São Paulo, Mirante de Santana), dados horários de 2022 a 2025, hora em UTC |
| `extrair_a701.py` | Extrai a A701 dos zips anuais do INMET ([portal.inmet.gov.br/dadoshistoricos](https://portal.inmet.gov.br/dadoshistoricos)) |
| `gerar_notebook.py` | Monta o notebook (as células ficam legíveis no git) |
| `modelo.json` | O modelo exportado, que a API em C# carrega |
| `requirements.txt` | Versões usadas na execução local (Python 3.11) |

## Decisões

- **Dados reais e públicos.** O briefing aceita bases públicas. O INMET mede a rua; a sala entra pelo sensor ESP32, pela regra do delta.
- **Alvo:** índice de calor (fórmula da NOAA, a mesma do ESP32 e da API) daqui a 2 horas, regressão.
- **Atributos só de produção:** temperatura, umidade, hora local e mês (em seno e cosseno) e variação da última hora. Nada que a API não tenha no boletim horário.
- **Hora em UTC convertida para São Paulo** antes de tudo, com o vetor de teste do contrato (15/01/2026 às 1700 UTC = 14h).
- **Preparação e análise:** origem e colunas do INMET documentadas no notebook; calendário UTC com 0/212/19/47 horas ausentes ou descartadas em 2022/2023/2024/2025. São 92 buracos, o maior com 54 h. O CSV limpo não permite separar falta de registro de descarte. A montagem dos atributos/alvo remove 205 linhas sem vizinhos; outras 4 são excluídas nas fronteiras entre conjuntos. Há distribuição do alvo e dos níveis e matriz de correlação no treino.
- **Divisão por tempo:** treino 2022–2023 (17.192 exemplos), validação 2024 (8.740), teste 2025 (8.645, usado só na avaliação final). O alvo +2 h não cruza as fronteiras.
- **Busca:** `TimeSeriesSplit(n_splits=3, gap=2)` só no treino; seis combinações de profundidade (4, 6, 8) e folha mínima (30, 50), pelo MAE. Melhor árvore: profundidade 8 e folha 30 (MAE médio na CV 1,054 °C; linear 1,092 °C).
- **Modelo:** árvore ajustada, escolhida pelo menor MAE em 2024 (0,981 °C contra 1,098 °C da linear). Não houve retreino. Exportada no formato existente `tipo: arvore`, `atributos` e `raiz` com nós `atributo`, `limite`, `menor_ou_igual`, `maior` ou folha `valor`. A integração paralela precisa suportar esse tipo, além da linear. O notebook compara a leitura do JSON com as previsões já calculadas em 200 exemplos (diferença máxima < 0,00005 °C).
- **Interpretação:** coeficientes da linear multiplicados pelo desvio padrão dos atributos e importância por redução de impureza da árvore; temperatura responde por cerca de 88,6% da importância da árvore. Associação não implica causalidade.

## Resultado no teste (2025)

| Modelo | MAE (°C) | RMSE (°C) | R² | F1 macro (níveis presentes) |
|---|---:|---:|---:|---:|
| Baseline "daqui a 2 h = agora" | 1,442 | 2,021 | 0,834 | 0,667 |
| Regressão linear | 1,053 | 1,451 | 0,914 | 0,745 |
| **Árvore (profundidade 8, folha 30; escolhida)** | **0,958** | **1,364** | **0,924** | **0,660** |

O nível "perigo" (índice ≥ 41 °C) não aparece na rua da A701 entre 2022 e 2025, por isso o F1 macro é a média dos níveis presentes. Dentro de uma sala sem ventilação o índice passa da rua: é o que o delta mede.

**Limite crítico:** no treino, só 104 alvos são `cuidado` (0,605%) e nenhum é `perigo`. No teste, a árvore detecta 7 das 48 horas de `cuidado` (precisão 58,3%, recall 14,6%), enquanto o baseline detecta 18 (precisão 38,3%, recall 37,5%). A árvore classifica 39 dessas horas como atenção e 2 como normal. O menor MAE não garante melhor alerta; a escolha por MAE foi mantida sem ajustar o modelo ao teste. O notebook inclui `classification_report` e matriz de confusão; a macro do relatório inclui o nível sem suporte e difere da tabela acima.

O aviso com 2 h de antecedência apoia a coordenação ao hidratar, ventilar ou realocar a turma, ligado ao **ODS 3.d** (alerta precoce e redução de riscos à saúde). O indicador de aulas em Perigo sem ação ainda não foi medido; faltam dados reais da sala e validação do delta. A previsão não substitui a leitura atual nem os procedimentos da escola.

## Como rodar

Na pasta `ml`, com Python 3.11:

```powershell
python -m pip install -r requirements.txt
python gerar_notebook.py
python -c "from pathlib import Path; import nbformat; from nbclient import NotebookClient; p = Path('termometro_da_sala.ipynb'); nb = nbformat.read(p, as_version=4); NotebookClient(nb, timeout=600, kernel_name='python3').execute(); nbformat.write(nb, p)"
```

No **Colab**, envie um ZIP da pasta `ml` pelo painel Arquivos, extraia em `/content`, execute `%cd /content/ml` e `%pip install -r requirements.txt`. Abra o `.ipynb` por Arquivo → Fazer upload de notebook e execute todas as células. Se o ambiente solicitar, reinicie a sessão após instalar as versões. O upload manual dispensa um link de repositório ainda não publicado. Esse fluxo não foi executado no Colab.
