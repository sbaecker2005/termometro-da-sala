"""Monta ml/termometro_da_sala.ipynb; execute com nbclient conforme README.md."""
from pathlib import Path

import nbformat as nbf

md, code = nbf.v4.new_markdown_cell, nbf.v4.new_code_cell
celulas = [
    md("""# Termômetro da Sala — previsão do índice de calor

[![Abrir no Colab](https://colab.research.google.com/assets/colab-badge.svg)](https://colab.research.google.com/github/sbaecker2005/termometro-da-sala/blob/main/ml/termometro_da_sala.ipynb)

**Global Solution 2026.2 (CitizenTech) — Machine Learning**

| Campo | Conteúdo |
|---|---|
| Problema | Nas tardes quentes, a sala de aula passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização; quem opera é a coordenação |
| O que o modelo faz | Prevê o **índice de calor da rua daqui a 2 horas**. A API soma a diferença medida entre a sala e a rua (o *delta* da sala, vindo do sensor ESP32) e mostra o nível previsto da sala no app e no desktop |
| Dados | Reais e públicos: INMET, estação automática A701 (São Paulo, Mirante de Santana), dados horários de 2022 a 2025 |
| Métricas | MAE do índice (°C) e F1 macro dos níveis, comparados com o baseline "daqui a 2 h = agora" |

Integrantes: Samuel Baecker — RM [preencher]"""),
    md("""## 1. Dados e origem
Fonte: [dados históricos públicos do INMET](https://portal.inmet.gov.br/dadoshistoricos),
arquivos anuais `2022.zip`, `2023.zip`, `2024.zip` e `2025.zip`; estação automática
**A701, São Paulo — Mirante de Santana**, de 01/01/2022 a 31/12/2025 (hora em UTC).
Consulta às condições de uso e atribuição: [portal do INMET](https://portal.inmet.gov.br/).
Não se presume uma licença específica que não esteja documentada na fonte.

`extrair_a701.py` seleciona o único CSV `_A701_` de cada ZIP, decodifica Latin-1,
pula as oito linhas de metadados e lê o separador `;` e a vírgula decimal.
Usa `Data` (ou `DATA (YYYY-MM-DD)`), `Hora UTC` (ou `HORA (UTC)`),
`TEMPERATURA DO AR - BULBO SECO, HORARIA (°C)` e
`UMIDADE RELATIVA DO AR, HORARIA (%)`. Exporta `data_hora_utc`, `temperatura_c`
e `umidade_pct` em `dados/inmet_a701.csv`, descartando registros sem data/hora,
temperatura ou umidade e valores vazios ou `-9999` nessas medições.
O CSV já está limpo: as ausências abaixo incluem horas não registradas ou descartadas,
sem permitir separar as causas. São Paulo é UTC−3 neste período.

**Reprodução / Colab:** abra pelo botão "Abrir no Colab" acima e use Ambiente de
execução → Executar tudo. No Colab não instale o `requirements.txt`: as versões
fixadas ali são as da execução local em Python 3.11 e não têm pacote para o Python
do Colab; as bibliotecas que já vêm no Colab bastam. Sem a pasta `ml` ao lado, a
célula abaixo lê o CSV direto do repositório público no GitHub.
Localmente, execute a partir de `ml` ou da raiz do projeto."""),
    code("""import json, math
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from sklearn.linear_model import LinearRegression
from sklearn.tree import DecisionTreeRegressor
from sklearn.ensemble import RandomForestRegressor
from pathlib import Path
from IPython.display import display
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score, f1_score, classification_report
from sklearn.model_selection import ParameterGrid
import platform
print('Python', platform.python_version())

PASTA = Path('ml') if Path('ml/dados/inmet_a701.csv').exists() else Path('.')
CSV_GITHUB = 'https://raw.githubusercontent.com/sbaecker2005/termometro-da-sala/main/ml/dados/inmet_a701.csv'
origem = PASTA / 'dados/inmet_a701.csv'
if not origem.exists():  # Colab aberto pelo link: o CSV vem do repositório
    origem = CSV_GITHUB
print('lendo', origem)
df = pd.read_csv(origem, parse_dates=['data_hora_utc'])
df['data_hora_utc'] = pd.to_datetime(df['data_hora_utc'], utc=True)
assert df[['temperatura_c', 'umidade_pct']].notna().all().all()
assert df.temperatura_c.between(-90, 60).all() and df.umidade_pct.between(0, 100).all()
print('timestamps duplicados removidos:', df.data_hora_utc.duplicated().sum())
df = df.drop_duplicates('data_hora_utc').set_index('data_hora_utc').sort_index()
df['local'] = df.index.tz_convert('America/Sao_Paulo')
# vetor de teste do relógio (CONTRATO.md): 15/01/2026 às 1700 UTC = 14h em São Paulo
assert pd.Timestamp('2026-01-15 17:00', tz='UTC').tz_convert('America/Sao_Paulo').hour == 14
print(len(df), 'horas válidas de', df.index.min().date(), 'a', df.index.max().date())
df.describe()[['temperatura_c', 'umidade_pct']]"""),
    code("""calendario = pd.date_range('2022-01-01', '2026-01-01', freq='h', inclusive='left', tz='UTC')
assert df.index.isin(calendario).all()
faltantes = calendario.difference(df.index)
por_ano = pd.DataFrame({'esperadas': pd.Series(1, index=calendario).groupby(calendario.year).sum(),
                       'validas': df.groupby(df.index.year).size()})
por_ano['ausentes_ou_descartadas'] = por_ano.esperadas - por_ano.validas
por_ano['ausentes_pct'] = (100 * por_ano.ausentes_ou_descartadas / por_ano.esperadas).round(2)
display(por_ano)
grupos = pd.Series(faltantes, index=faltantes).diff().ne(pd.Timedelta(hours=1)).cumsum()
buracos = pd.DataFrame({'hora': faltantes, 'grupo': grupos.to_numpy()}).groupby('grupo').hora.agg(['min', 'max', 'size'])
print('Buracos contínuos (incluindo bordas do período):', len(buracos))
display(buracos.sort_values('size', ascending=False).head(10))
assert por_ano.ausentes_ou_descartadas.sum() == len(faltantes)"""),
    md("## 2. Índice de calor\nFórmula da NOAA (Rothfusz, com os ajustes de umidade), a mesma do ESP32 e da API. Os 4 casos do contrato conferem a implementação."),
    code("""def indice_calor_c(temp_c, umidade):
    t = temp_c * 9 / 5 + 32
    rh = umidade
    simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094)
    if (simples + t) / 2 < 80:
        hi = simples
    else:
        hi = (-42.379 + 2.04901523 * t + 10.14333127 * rh - 0.22475541 * t * rh - 0.00683783 * t * t
              - 0.05481717 * rh * rh + 0.00122874 * t * t * rh + 0.00085282 * t * rh * rh - 0.00000199 * t * t * rh * rh)
        if rh < 13 and 80 <= t <= 112:
            hi -= ((13 - rh) / 4) * math.sqrt((17 - abs(t - 95)) / 17)
        elif rh > 85 and 80 <= t <= 87:
            hi += ((rh - 85) / 10) * ((87 - t) / 5)
    return round((hi - 32) * 5 / 9, 1)

NIVEIS = ['normal', 'atencao', 'cuidado', 'perigo']
def nivel(i):
    return 'normal' if i < 27 else 'atencao' if i < 32 else 'cuidado' if i < 41 else 'perigo'

for t, u, esperado, n in [(24.0, 40, 23.5, 'normal'), (30.0, 50, 31.0, 'atencao'), (33.4, 58, 39.8, 'cuidado'), (38.0, 70, 62.5, 'perigo')]:
    assert indice_calor_c(t, u) == esperado and nivel(esperado) == n

df['indice'] = [indice_calor_c(t, u) for t, u in zip(df.temperatura_c, df.umidade_pct)]
df['nivel'] = df['indice'].map(nivel)
df['nivel'].value_counts()"""),
    md("Quando o calor aparece: índice médio por hora local nos meses de verão (dezembro a março)."),
    code("""verao = df[df.local.dt.month.isin([12, 1, 2, 3])]
verao.groupby(verao.local.dt.hour)['indice'].mean().plot(marker='o', figsize=(8, 3), title='Índice de calor médio por hora (verão, A701)')
plt.xlabel('hora local'); plt.ylabel('°C'); plt.grid(alpha=.3); plt.show()"""),
    md("""## 3. Atributos e alvo
Só atributos que **existem em produção** (o boletim horário do INMET): temperatura, umidade, hora local (em seno e cosseno, para o modelo entender que 23h fica perto de 0h), mês, variação da temperatura na última hora e índice de calor atual, calculado pela mesma fórmula da API. O alvo é o índice de calor **2 horas depois**. Só entram linhas com as horas vizinhas presentes (sem buracos). O índice atual responde ao baixo recall encontrado na versão anterior, sem mudar os limites dos níveis do contrato."""),
    code("""uma_h, duas_h = pd.Timedelta(hours=1), pd.Timedelta(hours=2)
anterior = df['temperatura_c'].reindex(df.index - uma_h).to_numpy()
futuro = df['indice'].reindex(df.index + duas_h).to_numpy()
X = pd.DataFrame({
    'temperatura_c': df.temperatura_c,
    'umidade_pct': df.umidade_pct,
    'hora_sen': np.sin(2 * np.pi * df.local.dt.hour / 24),
    'hora_cos': np.cos(2 * np.pi * df.local.dt.hour / 24),
    'mes_sen': np.sin(2 * np.pi * df.local.dt.month / 12),
    'mes_cos': np.cos(2 * np.pi * df.local.dt.month / 12),
    'variacao_1h': df.temperatura_c.to_numpy() - anterior,
    'indice_agora': df['indice'],
}, index=df.index)
dados = X.assign(alvo=futuro).dropna()
ano = dados.index.year
treino = dados[(ano <= 2023) & (dados.index + duas_h < pd.Timestamp('2024-01-01', tz='UTC'))]
valid = dados[(ano == 2024) & (dados.index + duas_h < pd.Timestamp('2025-01-01', tz='UTC'))]
teste = dados[ano == 2025]
ATRIBUTOS = list(X.columns)
print('dropna (sem hora anterior ou alvo +2 h):', len(df) - len(dados))
print('exemplos excluídos nas fronteiras:', len(dados) - len(treino) - len(valid) - len(teste))
print('treino', len(treino), '| validação', len(valid), '| teste', len(teste))
assert treino.index.max() + duas_h < valid.index.min()
assert valid.index.max() + duas_h < teste.index.min()"""),
    md("""### Distribuições e correlação (treino)
A análise do alvo usa apenas o treino, preservando 2025 para a avaliação final.
As frequências evidenciam o desbalanceamento: MAE e acurácia global podem esconder
erros em `cuidado`. Correlação mede associação linear, sem demonstrar causalidade;
seno e cosseno devem ser interpretados em conjunto."""),
    code("""treino.alvo.plot.hist(bins=40, figsize=(8, 3), title='Alvo: índice de calor +2 h (treino)')
plt.xlabel('°C'); plt.ylabel('horas'); plt.show()
contagens = treino.alvo.map(nivel).value_counts().reindex(NIVEIS, fill_value=0)
display(pd.DataFrame({'horas': contagens, 'percentual': (100 * contagens / len(treino)).round(3)}))
contagens.plot.bar(title='Níveis do alvo (treino)', figsize=(6, 3))
plt.ylabel('horas'); plt.show()
correlacao = treino[ATRIBUTOS + ['alvo']].corr()
fig, ax = plt.subplots(figsize=(9, 7))
imagem = ax.imshow(correlacao, vmin=-1, vmax=1, cmap='coolwarm')
ax.set_xticks(range(len(correlacao)), correlacao.columns, rotation=90)
ax.set_yticks(range(len(correlacao)), correlacao.columns)
for i in range(len(correlacao)):
    for j in range(len(correlacao)):
        ax.text(j, i, f'{correlacao.iloc[i, j]:.2f}', ha='center', va='center', fontsize=8)
fig.colorbar(imagem, ax=ax); plt.tight_layout(); plt.show()"""),
    md("""## 4. Modelos
Divisão **por tempo**, sem embaralhar: cada configuração treina só em 2022–2023
 e é medida em 2024. Grade completa: profundidades 4 a 12 e None, folhas de 10 a 50
 (todos os inteiros), 410 configurações. Escolha da árvore pelo menor MAE não arredondado
 de validação; em empate, menor RMSE. Comparamos linear, persistência e uma floresta
 de 100 árvores (profundidade 12, folha 10), sem buscar parâmetros do ensemble.
 Exportamos sempre uma árvore única: a API percorre seus nós em C#, sem Python.
 Nenhum retreino após a escolha. O ano de 2025 fica para a avaliação final.
 Como 2025 já apareceu na auditoria anterior, este é um teste de regressão temporal,
 não uma confirmação independente da revisão; será necessário um período novo."""),
    code("""def avalia(nome, previsto, real):
    reais = [nivel(v) for v in real]
    presentes = [n for n in NIVEIS if n in reais]  # 'perigo' não aparece na A701 em 2022-2025: fica fora da média
    return {'modelo': nome, 'MAE (°C)': round(mean_absolute_error(real, previsto), 3),
            'RMSE (°C)': round(float(np.sqrt(mean_squared_error(real, previsto))), 3),
            'R²': round(r2_score(real, previsto), 3),
            'F1 macro (níveis)': round(f1_score(reais, [nivel(v) for v in previsto], average='macro', labels=presentes, zero_division=0), 3)}

resultados_grade = []
configuracoes = list(ParameterGrid({'max_depth': list(range(4, 13)) + [None],
                                    'min_samples_leaf': list(range(10, 51))}))
arvore, melhor_erro = None, (float('inf'), float('inf'))
for parametros in configuracoes:
    candidata = DecisionTreeRegressor(random_state=42, **parametros).fit(treino[ATRIBUTOS], treino.alvo)
    p = candidata.predict(valid[ATRIBUTOS])
    erros = (mean_absolute_error(valid.alvo, p), float(np.sqrt(mean_squared_error(valid.alvo, p))))
    resultados_grade.append({**parametros, 'MAE (°C)': erros[0], 'RMSE (°C)': erros[1]})
    if erros < melhor_erro:
        arvore, melhor_erro, melhores_parametros = candidata, erros, parametros
assert len(resultados_grade) == 410
grade = pd.DataFrame(resultados_grade)
grade['max_depth'] = grade.max_depth.map(lambda v: 'None' if pd.isna(v) else str(int(v)))
grade = grade.sort_values(['MAE (°C)', 'RMSE (°C)'], kind='stable')
grade.to_csv(PASTA / 'grade-parametros.csv', index=False)
with pd.option_context('display.max_rows', 410):
    display(grade)
print('Melhor árvore:', melhores_parametros, '| MAE e RMSE 2024:', melhor_erro)
linear = LinearRegression().fit(treino[ATRIBUTOS], treino.alvo)
floresta = RandomForestRegressor(n_estimators=100, max_depth=12, min_samples_leaf=10,
                                 random_state=42, n_jobs=1).fit(treino[ATRIBUTOS], treino.alvo)
# Versão anterior: mesma divisão, sete atributos, profundidade 8 e folha 30.
atributos_antes = [a for a in ATRIBUTOS if a != 'indice_agora']
arvore_antes = DecisionTreeRegressor(max_depth=8, min_samples_leaf=30, random_state=42).fit(
    treino[atributos_antes], treino.alvo)
candidatos = {'baseline (agora)': lambda d: d.indice_agora.to_numpy(),
              'regressão linear': lambda d: linear.predict(d[ATRIBUTOS]),
              'floresta (comparação)': lambda d: floresta.predict(d[ATRIBUTOS]),
              'árvore anterior': lambda d: arvore_antes.predict(d[atributos_antes]),
              'árvore ajustada': lambda d: arvore.predict(d[ATRIBUTOS])}
validacao = pd.DataFrame([avalia(n, f(valid), valid.alvo) for n, f in candidatos.items()])
display(validacao)
print('Exportação: árvore única com menor MAE em 2024; ensemble somente para comparação.')"""),
    md("""### Importância dos atributos
Coeficientes da linear multiplicados pelo desvio padrão de cada atributo no treino:
variação prevista em °C para +1 desvio padrão, mantendo os demais atributos constantes.
A árvore usa redução de impureza, que pode distribuir importância entre atributos
correlacionados; as duas escalas não são diretamente comparáveis."""),
    code("""importancias = pd.DataFrame({'coeficiente_padronizado': linear.coef_ * treino[ATRIBUTOS].std(ddof=0),
                              'importancia_arvore': arvore.feature_importances_}, index=ATRIBUTOS)
display(importancias)
importancias.plot.bar(subplots=True, figsize=(9, 6), legend=False)
plt.tight_layout(); plt.show()"""),
    code("""escolhido = 'árvore ajustada'
print('modelo escolhido na validação:', escolhido)
previsoes_teste = {n: f(teste) for n, f in candidatos.items()}
resultado_teste = pd.DataFrame([avalia(n, p, teste.alvo) for n, p in previsoes_teste.items()])
resultado_teste"""),
    md("### Onde o modelo acerta e onde erra (teste, 2025)"),
    md("O `classification_report` inclui os quatro níveis: sua média macro inclui `perigo` com suporte zero. A tabela de regressão usa F1 macro apenas dos níveis presentes, por isso as duas médias diferem."),
    code("""prev = previsoes_teste[escolhido]
relatorios = {}
for nome in ['baseline (agora)', escolhido]:
    reais, previstos = teste.alvo.map(nivel), pd.Series(previsoes_teste[nome]).map(nivel)
    print(nome)
    print(classification_report(reais, previstos, labels=NIVEIS, zero_division=0, digits=3))
    relatorios[nome] = classification_report(reais, previstos, labels=NIVEIS, zero_division=0, output_dict=True)
display(pd.DataFrame({nome: rel['cuidado'] for nome, rel in relatorios.items()}).T)
print('Cuidado no teste:', relatorios[escolhido]['cuidado']['support'],
      'horas; recall do modelo:', round(relatorios[escolhido]['cuidado']['recall'], 3),
      '| baseline:', round(relatorios['baseline (agora)']['cuidado']['recall'], 3))
erro = pd.Series(np.abs(prev - teste.alvo.to_numpy()), index=teste.index)
erro.groupby(teste.index.tz_convert('America/Sao_Paulo').hour).mean().plot(kind='bar', figsize=(8, 3), title='Erro médio por hora local (teste)')
plt.ylabel('°C'); plt.show()
pd.crosstab(pd.Series([nivel(v) for v in teste.alvo], name='real'), pd.Series([nivel(v) for v in prev], name='previsto'))"""),
    md("""### Alerta antes e depois
Alerta significa índice previsto **≥ 32 °C** (cuidado ou perigo), sem alterar a tabela
 do contrato. Comparamos a árvore anterior (sete atributos), a revisão (índice atual
 incluído e grade ampliada) e a persistência. Precisão, recall e contagens são medidos
 em 2024 e depois em 2025; não calibramos um novo limiar no teste.
 São alertas da rua: o delta da sala ainda precisa de validação com medições reais."""),
    code("""from sklearn.metrics import precision_score, recall_score
alertas = []
for periodo, conjunto in [('validação 2024', valid), ('teste 2025', teste)]:
    real = conjunto.alvo.to_numpy() >= 32
    for nome in ['baseline (agora)', 'árvore anterior', escolhido]:
        previsto = candidatos[nome](conjunto) >= 32
        tp = int(np.sum(real & previsto))
        fp = int(np.sum(~real & previsto))
        fn = int(np.sum(real & ~previsto))
        assert tp + fn == int(real.sum())
        alertas.append({'período': periodo, 'modelo': nome, 'TP': tp, 'FP': fp, 'FN': fn,
                        'precisão': precision_score(real, previsto, zero_division=0),
                        'recall': recall_score(real, previsto, zero_division=0)})
alertas = pd.DataFrame(alertas)
display(alertas)
alertas.to_csv(PASTA / 'alertas.csv', index=False)"""),
    md("""## 5. Exportar `modelo.json`
A API (C#) carrega este arquivo e aplica o modelo: não há servidor Python em produção. O JSON leva os atributos na ordem, os coeficientes (linear) ou os nós da árvore, e as métricas de teste."""),
    code("""def arvore_json(m):
    t = m.tree_
    def no(i):
        if t.children_left[i] == -1:
            return {'valor': round(float(t.value[i][0][0]), 4)}
        return {'atributo': ATRIBUTOS[t.feature[i]], 'limite': round(float(t.threshold[i]), 6),
                'menor_ou_igual': no(t.children_left[i]), 'maior': no(t.children_right[i])}
    return no(0)

metricas = resultado_teste.set_index('modelo').loc[escolhido].to_dict()
base = resultado_teste.set_index('modelo').loc['baseline (agora)'].to_dict()
modelo = {'versao': 1, 'alvo': 'indice_calor_c em 2 h', 'horizonte_horas': 2, 'atributos': ATRIBUTOS,
          'fonte': 'INMET A701, horário, 2022-2025 (treino 2022-2023, validação 2024, teste 2025)',
          'metricas_teste': metricas, 'baseline_teste': base}
modelo |= {'tipo': 'arvore', 'raiz': arvore_json(arvore)}
with open(PASTA / 'modelo.json', 'w', encoding='utf-8') as f:
    json.dump(modelo, f, ensure_ascii=False, indent=1)
print('modelo.json:', modelo['tipo'], metricas)"""),
    md("Conferência: a mesma previsão feita lendo o JSON (como a API vai fazer) bate com o modelo do scikit-learn."),
    code("""def prever_json(m, linha):
    if m['tipo'] == 'linear':
        return m['intercepto'] + sum(m['coeficientes'][a] * linha[a] for a in m['atributos'])
    n = m['raiz']
    while 'valor' not in n:
        n = n['menor_ou_igual'] if linha[n['atributo']] <= n['limite'] else n['maior']
    return n['valor']

with open(PASTA / 'modelo.json', encoding='utf-8') as f:
    exportado = json.load(f)
amostra = teste.sample(200, random_state=1)
referencia = pd.Series(prev, index=teste.index)
diferenca = max(abs(prever_json(exportado, r) - referencia.loc[i]) for i, r in amostra.iterrows())
assert diferenca < 0.01, diferenca
print('diferença máxima JSON x scikit-learn:', round(diferenca, 5))
paridade = amostra[ATRIBUTOS].copy()
paridade['previsto'] = arvore.predict(amostra[ATRIBUTOS])
paridade.to_json(PASTA / 'paridade-api.json', orient='records', indent=1, double_precision=15)
print('paridade-api.json:', len(paridade), 'previsões do scikit-learn para o teste da API')"""),
    md("""## 6. Previsão em uso
O mesmo vetor do teste da API (`PrevisorModeloTestes.cs`): 15/01/2026 às 17:00 UTC
(14h em São Paulo), rua a 30 °C e 50% de umidade, 1,5 °C mais quente que uma hora antes.
A previsão sai do `modelo.json` percorrido como a API faz. Depois soma-se o *delta*
da sala: a diferença média sala − rua das últimas 3 h, medida pelo ESP32. Aqui o delta
é o valor de exemplo da sala 3 no modo demonstração (+4 °C), não uma medição real.
O nível e a ação sugerida seguem a tabela do `CONTRATO.md`."""),
    code("""ACOES = {'normal': 'nenhuma',
         'atencao': 'liberar água, abrir janelas e ligar ventiladores',
         'cuidado': 'pausa, troca de sala ou adiar Educação Física',
         'perigo': 'tirar a turma da sala'}
agora_utc = pd.Timestamp('2026-01-15 17:00', tz='UTC')
local = agora_utc.tz_convert('America/Sao_Paulo')
entrada = {'temperatura_c': 30.0, 'umidade_pct': 50.0,
           'hora_sen': np.sin(2 * np.pi * local.hour / 24), 'hora_cos': np.cos(2 * np.pi * local.hour / 24),
           'mes_sen': np.sin(2 * np.pi * local.month / 12), 'mes_cos': np.cos(2 * np.pi * local.month / 12),
           'variacao_1h': 1.5, 'indice_agora': indice_calor_c(30.0, 50)}
rua_prevista = prever_json(exportado, entrada)
assert abs(rua_prevista - arvore.predict(pd.DataFrame([entrada])[ATRIBUTOS])[0]) < 0.01
vetores = []
for temp, umidade, hora, mes, variacao in [(30, 50, 14, 1, 1.5), (18, 80, 0, 7, -0.4), (27.2, 62, 12, 12, 0)]:
    x = {'temperatura_c': temp, 'umidade_pct': umidade, 'hora_sen': np.sin(2*np.pi*hora/24),
         'hora_cos': np.cos(2*np.pi*hora/24), 'mes_sen': np.sin(2*np.pi*mes/12),
         'mes_cos': np.cos(2*np.pi*mes/12), 'variacao_1h': variacao, 'indice_agora': indice_calor_c(temp, umidade)}
    vetores.append({**x, 'previsto': prever_json(exportado, x)})
print('Vetores para PrevisorModeloTestes:')
display(pd.DataFrame(vetores))
DELTA_SALA_3 = 4.0  # exemplo do modo demonstração, não medição
sala_prevista = rua_prevista + DELTA_SALA_3
display(pd.DataFrame({'valor': [f"{local:%d/%m/%Y %Hh} (São Paulo)", f"{indice_calor_c(30.0, 50)} °C ({nivel(indice_calor_c(30.0, 50))})",
                                f"{rua_prevista:.2f} °C", f"+{DELTA_SALA_3:.0f} °C", f"{sala_prevista:.2f} °C",
                                nivel(sala_prevista), ACOES[nivel(sala_prevista)]]},
                     index=['agora', 'índice da rua agora', 'rua prevista para +2 h', 'delta da sala 3',
                            'sala prevista para +2 h', 'nível previsto', 'ação sugerida']))"""),
    md("""### Previsto contra real e resíduos (teste, 2025)
A semana em torno da hora mais quente de 2025 mostra o modelo escolhido e o baseline
contra o índice que de fato ocorreu 2 h depois. O histograma mostra a distribuição
do erro (previsto − real) em todo o teste."""),
    code("""pico = teste.alvo.idxmax()
semana = teste.loc[pico - pd.Timedelta(days=3.5): pico + pd.Timedelta(days=3.5)]
quando = (semana.index + duas_h).tz_convert('America/Sao_Paulo')
fig, ax = plt.subplots(figsize=(11, 4))
ax.plot(quando, semana.alvo, label='real', color='black', linewidth=1.5)
ax.plot(quando, pd.Series(prev, index=teste.index).loc[semana.index], label=escolhido, linewidth=1.2)
ax.plot(quando, semana.indice_agora, label='baseline (agora)', linestyle='--', linewidth=1, alpha=.7)
for limite in (27, 32):
    ax.axhline(limite, color='grey', linestyle=':', linewidth=.8)
ax.set_title('Índice de calor da rua: previsto para +2 h contra real (semana mais quente do teste)')
ax.set_ylabel('°C'); ax.set_xlabel('hora local prevista'); ax.legend(); ax.grid(alpha=.3)
plt.tight_layout(); plt.show()
residuos = pd.Series(prev - teste.alvo.to_numpy(), index=teste.index)
residuos.plot.hist(bins=60, figsize=(8, 3), title=f'Resíduos no teste: previsto − real ({escolhido})')
plt.axvline(0, color='black', linewidth=.8); plt.xlabel('°C'); plt.ylabel('horas'); plt.show()
print('resíduo médio:', round(residuos.mean(), 3), '°C | 90% dos erros entre',
      round(residuos.quantile(.05), 2), 'e', round(residuos.quantile(.95), 2), '°C')"""),
    md("""## 7. Conclusões e limitações
- O modelo é comparado com o baseline "daqui a 2 h = agora", que é o que uma pessoa faria olhando o termômetro. A tabela de teste mostra quanto ele ganha.
- `Cuidado` é raro: o relatório mostra seu recall e a comparação com o baseline, pois o F1 macro não basta para avaliar a segurança. `Perigo` não aparece nesta base; suporte zero não comprova capacidade de previsão desse nível. Não há garantia de detectar todos os episódios críticos, nem validação da previsão dentro da sala.
- A tabela de regressão e a comparação de precisão/recall mostram separadamente erro em graus e alerta. O índice atual responde ao limite da versão anterior; os limites continuam 27, 32 e 41 °C. Nenhum limiar foi escolhido em 2025. O atributo adicional não garante detectar todos os episódios; falsos negativos e falsos positivos devem acompanhar a demonstração.
- O aviso com 2 h de antecedência apoia a decisão da coordenação de hidratar, ventilar ou realocar a turma, ligado ao **ODS 3.d** (alerta precoce e redução de riscos à saúde). O impacto deve ser medido por aulas em Perigo sem ação, ainda sem evidência medida neste estudo; a previsão não substitui a leitura atual nem os protocolos da escola.
- A previsão é da **rua**. A sala entra pela regra do delta (média das últimas 3 h da diferença sala − rua, medida pelo ESP32), que é simples e auditável. Ela supõe que a diferença das últimas 3 h se mantém nas próximas 2 h; no fim da tarde tende a superestimar.
- Os dados são públicos e reais (INMET). As leituras da sala no Wokwi e no modo demonstração são simuladas, e o README diz isso.
- Próximo passo: com uma semana de leituras reais da sala (termo-higrômetro anotado pelo professor), avaliar a previsão completa da sala contra o que a sala realmente teve 2 h depois."""),
]
celulas += [
    md("""## 8. Mapa térmico da sala
**Tudo nesta seção é simulado**, não é medição da escola. Sala 7 de 7 × 9 m,
quatro sensores e seis lugares (dois vulneráveis, sem cadastro de pessoas).
São 14 dias por estação, de hora em hora, seed 42: janela quente à tarde no verão
e parede externa fria de madrugada no inverno, com variação diária e ruído de 0,2 °C.
O índice usa a mesma fórmula acima; interpolamos temperatura e índice separadamente.
Os lugares são estimativas derivadas dos quatro sensores, não novas observações.

IDW usa potência 2 e grade de 0,5 m, preserva o valor sobre o sensor e ignora leituras
ausentes. Em uso, selecionar só a última leitura por sensor com até 15 minutos.
A média da sala é a média dos quatro sensores. As funções abaixo também estão em `mapa.py`;
ficam incluídas para executar este notebook sozinho."""),
    code(Path(__file__).with_name('mapa.py').read_text(encoding='utf-8')),
    code("""simulados = simular(indice_calor_c)
(PASTA / 'dados').mkdir(parents=True, exist_ok=True)
simulados.to_csv(PASTA / 'dados/sala7-simulada.csv', index=False)
print(len(simulados), 'registros simulados; sensores e lugares estimados')
display(simulados.groupby(['estacao', 'tipo']).size().rename('registros'))
grupos, silhuetas = agrupar(simulados)
display(silhuetas)
display(grupos)
grupos.to_csv(PASTA / 'grupos-pontos.csv', index=False)
silhuetas.to_csv(PASTA / 'silhuetas-pontos.csv', index=False)"""),
    md("""### Agrupamento e pontos crônicos
K-means compara perfis médios de 24 horas em °C, sem padronizar para preservar
diferenças de temperatura. Escolhemos k de 2 a 4 pela maior silhueta, por estação.
Crônico quente/frio significa desvio positivo/negativo em **todas as leituras individuais**
da estação; qualquer inversão de sinal ou igualdade torna o ponto variável. Os lugares interpolados
compartilham informação dos sensores e não são amostras independentes."""),
    code("""for estacao, hora in [('verao', 15), ('inverno', 4)]:
    retrato = simulados[(simulados.estacao == estacao) & (simulados.dia == 12) & (simulados.hora == hora)]
    sensores = retrato[retrato.tipo == 'sensor'].to_dict('records')
    superficie = grade(sensores)
    pontos = retrato.merge(grupos, on=['estacao', 'nome'])
    fig, ax = plt.subplots(figsize=(7, 7))
    imagem = ax.pcolormesh(superficie.x.unique(), superficie.y.unique(),
                           superficie.indice.to_numpy().reshape(19, 15), cmap='coolwarm', shading='auto')
    fig.colorbar(imagem, ax=ax, label='índice estimado (°C)')
    for grupo, parte in pontos.groupby('grupo'):
        ax.scatter(parte.x, parte.y, c=parte.nivel.map(CORES), marker=['o', 's', '^', 'D'][grupo],
                   s=110, edgecolors='black', label=f'grupo {grupo + 1}')
    for p in pontos.to_dict('records'):
        ax.annotate(p['nome'], (p['x'], p['y']), xytext=(4, 5), textcoords='offset points', fontsize=8)
    for faixa, cor in CORES.items():
        ax.scatter([], [], color=cor, label=faixa)
    ax.set(xlim=(-.4, 7.8), ylim=(-.4, 9.7), xlabel='x (m)', ylabel='y (m)',
           title=f'Sala 7 — SIMULADO — {estacao}, dia 12, {hora}h')
    ax.set_aspect('equal'); ax.legend(loc='upper left', fontsize=8)
    plt.tight_layout(); plt.show()
    print(estacao, precaucoes(retrato.to_dict('records')))"""),
    md("""### Previsão por ponto (+2 h)
Reutilizamos a árvore de `modelo.json`, congelada. Uma regressão linear aprende
o resíduo da árvore com os oito atributos originais e `desvio_ponto` (índice do
ponto menos média dos sensores no mesmo instante). A correção fica em
`modelo_pontos.json`, que contém a árvore base e seus coeficientes, sem substituir
o arquivo original. Este formato separado ainda precisa de um consumidor próprio.
Treino nos dias 1–10 de cada estação; teste nos dias 11–14, sem alvo atravessando
a fronteira. Parâmetros fixos, sem escolher modelo no teste.
Comparamos MAE, RMSE, R² e F1 macro com persistência e com a árvore original,
também somando apenas o desvio atual. O teste é simulado e não valida uso real."""),
    code("""modelo_pontos, metricas_pontos, paridade_pontos = treinar_pontos(simulados, exportado)
exportar_modelo(modelo_pontos, PASTA / 'modelo_pontos.json')
metricas_pontos.to_csv(PASTA / 'metricas-pontos.csv', index=False)
print(modelo_pontos['divisao'])
display(metricas_pontos)
recarregado = json.loads((PASTA / 'modelo_pontos.json').read_text(encoding='utf-8'))
erro_json = max(abs(prever_json(recarregado, r) - r['previsto'])
                for r in paridade_pontos.to_dict('records'))
assert erro_json < 1e-9, erro_json
print('Diferença máxima JSON x regressão:', erro_json)
ganho = (metricas_pontos.set_index('modelo').loc['persistencia', 'MAE'] -
         metricas_pontos.set_index('modelo').loc['modelo_pontos', 'MAE'])
print('Redução de MAE contra persistência (negativo = pior):', ganho)"""),
    md("""### Precauções e limites
Lugar vulnerável abaixo de 18 °C ou a partir de 32 °C recebe sugestão de troca
para o lugar com índice mais próximo de [18, 27) °C, somente se houver melhora.
Sem alternativa melhor, procurar outro ambiente. Diferença de pelo menos 4 °C
entre pontos (sensores e lugares) gera aviso de ambiente desigual, indicando extremos calculados.
Não estimamos ocupação nem identificamos moradores/alunos. A interpolação não
modela paredes, obstáculos, radiação solar ou circulação de ar. Quatro sensores
não validam todos os lugares: precisamos de mais sensores reais, calibrados e
posicionados em diferentes alturas, e dados reais em outras épocas para avaliar
os grupos, a previsão e as precauções. Nenhum resultado simulado comprova segurança."""),
]
nb = nbf.v4.new_notebook(cells=celulas, metadata={'kernelspec': {'name': 'python3', 'display_name': 'Python 3', 'language': 'python'}})
nbf.write(nb, Path(__file__).with_name('termometro_da_sala.ipynb'))
print('notebook gerado')
