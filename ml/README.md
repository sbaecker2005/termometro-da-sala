# Termômetro da Sala — IA e Machine Learning

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Problema | Nas tardes quentes, a sala de aula passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização; quem opera é a coordenação |
| O que o modelo faz | Prevê o índice de calor da rua daqui a 2 horas; a API soma o delta medido da sala e mostra o nível previsto no app e no desktop |

Integrantes: Samuel Baecker — RM [preencher]

## Arquivos

| Arquivo | O que é |
|---|---|
| `termometro_da_sala.ipynb` | O notebook, já executado (dados, índice de calor, modelos, métricas, exportação) |
| `dados/inmet_a701.csv` | Dataset: INMET, estação automática A701 (São Paulo, Mirante de Santana), dados horários de 2022 a 2025, hora em UTC |
| `extrair_a701.py` | Extrai a A701 dos zips anuais do INMET ([portal.inmet.gov.br/dadoshistoricos](https://portal.inmet.gov.br/dadoshistoricos)) |
| `gerar_notebook.py` | Monta o notebook (as células ficam legíveis no git) |
| `modelo.json` | O modelo exportado, que a API em C# carrega |

## Decisões

- **Dados reais e públicos.** O briefing aceita bases públicas. O INMET mede a rua; a sala entra pelo sensor ESP32, pela regra do delta.
- **Alvo:** índice de calor (fórmula da NOAA, a mesma do ESP32 e da API) daqui a 2 horas, regressão.
- **Atributos só de produção:** temperatura, umidade, hora local e mês (em seno e cosseno) e variação da última hora. Nada que a API não tenha no boletim horário.
- **Hora em UTC convertida para São Paulo** antes de tudo, com o vetor de teste do contrato (15/01/2026 às 1700 UTC = 14h).
- **Divisão por tempo:** treino 2022–2023, validação 2024 (escolha do modelo), teste 2025 (medido uma vez).
- **Modelo:** regressão linear, que ganhou da árvore rasa na validação e é exportada como coeficientes num JSON, aplicado pela API sem servidor Python.

## Resultado no teste (2025)

| Modelo | MAE (°C) | F1 macro (níveis) |
|---|---:|---:|
| Baseline "daqui a 2 h = agora" | 1,442 | 0,667 |
| **Regressão linear (escolhido)** | **1,052** | **0,745** |
| Árvore (profundidade 6) | 1,087 | 0,689 |

O nível "perigo" (índice ≥ 41 °C) não aparece na rua da A701 entre 2022 e 2025, por isso o F1 macro é a média dos níveis presentes. Dentro de uma sala sem ventilação o índice passa da rua: é o que o delta mede.

## Como rodar

```
pip install pandas scikit-learn matplotlib nbformat nbclient ipykernel
python gerar_notebook.py
python -m jupyter nbconvert --to notebook --execute --inplace termometro_da_sala.ipynb
```
