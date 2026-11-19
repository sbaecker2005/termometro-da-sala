"""Mapa da Sala 7 e estudo com dados exclusivamente simulados."""
import json
import math
from pathlib import Path

import numpy as np
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.linear_model import LinearRegression
from sklearn.metrics import (f1_score, mean_absolute_error, mean_squared_error,
                             r2_score, silhouette_score)

SEED = 42
LARGURA, PROFUNDIDADE = 7, 9
SENSORES = [dict(nome=n, x=x, y=y) for n, x, y in
            [('janela', 0, 4), ('fundo', 7, 9), ('porta', 7, 0), ('centro', 3.5, 4.5)]]
LUGARES = [dict(nome=f'lugar {i + 1}', x=x, y=y, vulneravel=i < 2)
           for i, (x, y) in enumerate([(1, 4), (6, 8), (2, 2), (5, 2), (2, 6), (5, 6)])]
CORES = {'frio': 'blue', 'normal': 'green', 'atencao': 'orange',
         'cuidado': 'red', 'perigo': 'darkred'}


def leituras_recentes(leituras, agora):
    recentes = {}
    for leitura in leituras:
        instante = pd.Timestamp(leitura['instante'])
        if pd.Timedelta(0) <= agora - instante <= pd.Timedelta(minutes=15):
            nome = leitura['nome']
            if nome not in recentes or instante > pd.Timestamp(recentes[nome]['instante']):
                recentes[nome] = leitura
    return list(recentes.values())


def nivel(valor):
    if not math.isfinite(valor):
        raise ValueError('Índice deve ser finito')
    return ('frio' if valor < 18 else 'normal' if valor < 27 else
            'atencao' if valor < 32 else 'cuidado' if valor < 41 else 'perigo')


def idw(sensores, x, y, campo='temperatura_c'):
    """Potência 2; None/NaN não participa. Sem leituras, retorna None.

    O chamador fornece uma única leitura recente por sensor (até 15 minutos).
    """
    if not math.isfinite(x) or not math.isfinite(y):
        raise ValueError('Coordenadas devem ser finitas')
    soma, pesos = 0.0, 0.0
    for sensor in sensores:
        valor = sensor.get(campo)
        if valor is None or not math.isfinite(valor):
            continue
        sx, sy = sensor['x'], sensor['y']
        if not math.isfinite(sx) or not math.isfinite(sy):
            raise ValueError('Coordenadas devem ser finitas')
        distancia2 = (x - sx) ** 2 + (y - sy) ** 2
        if distancia2 == 0:
            return float(valor)
        peso = 1 / distancia2
        soma += peso * valor
        pesos += peso
    return soma / pesos if pesos else None


def grade(sensores):
    return pd.DataFrame([{'x': x, 'y': y,
                          'temperatura_c': idw(sensores, x, y),
                          'indice': idw(sensores, x, y, 'indice')}
                         for y in np.arange(0, PROFUNDIDADE + .5, .5)
                         for x in np.arange(0, LARGURA + .5, .5)])


def precaucoes(pontos):
    """Recebe sensores e lugares com índice; nenhum dado pessoal é necessário."""
    pontos = [p for p in pontos if p.get('indice') is not None
              and math.isfinite(p['indice'])]
    lugares = [p for p in pontos if p.get('tipo') != 'sensor']
    avisos = []

    def distancia_normal(p):
        return max(18 - p['indice'], 0, p['indice'] - 27)

    for ponto in lugares:
        if ponto.get('vulneravel') and nivel(ponto['indice']) in ('frio', 'cuidado', 'perigo'):
            candidatos = [p for p in lugares if p['nome'] != ponto['nome']]
            if candidatos:
                destino = min(candidatos, key=lambda p: (distancia_normal(p),
                              nivel(p['indice']) != 'normal', p['nome']))
                if distancia_normal(destino) < distancia_normal(ponto):
                    avisos.append(f"trocar {ponto['nome']} para {destino['nome']} "
                                  f"({ponto['indice']:.1f} → {destino['indice']:.1f} °C)")
                    continue
            avisos.append(f"{ponto['nome']}: sem lugar mais seguro; procurar outro ambiente")
    if pontos:
        frio, quente = min(pontos, key=lambda p: p['indice']), max(pontos, key=lambda p: p['indice'])
        diferenca = quente['indice'] - frio['indice']
        if diferenca >= 4:
            avisos.append(f"ambiente desigual: verificar janela, sol e corrente de ar perto de "
                          f"{quente['nome']} e {frio['nome']} (diferença {diferenca:.1f} °C)")
    return avisos


def simular(indice_calor):
    """14 dias horários por estação, hora local de São Paulo, seed fixo."""
    rng = np.random.default_rng(SEED)
    linhas = []
    for estacao, inicio, base in [('verao', '2026-01-01', 28), ('inverno', '2026-07-01', 19)]:
        dias = rng.normal(0, .8, 14)
        for instante in pd.date_range(inicio, periods=14 * 24, freq='h', tz='America/Sao_Paulo'):
            h = instante.hour
            temperatura = base + 3.5 * np.cos(2 * np.pi * (h - 15) / 24) + dias[instante.day - 1]
            umidade = np.clip(65 - 1.3 * (temperatura - base) + rng.normal(0, 2), 35, 90)
            sensores = []
            for sensor in SENSORES:
                desvio = {'janela': .8, 'fundo': -.8, 'porta': -.4, 'centro': .1}[sensor['nome']]
                if estacao == 'verao' and sensor['nome'] == 'janela':
                    desvio += 5 * np.exp(-((h - 15) / 3) ** 2)
                if estacao == 'inverno' and sensor['nome'] == 'fundo':
                    desvio -= 3.5 * np.exp(-((h - 4) / 3) ** 2)
                temp = temperatura + desvio + rng.normal(0, .2)
                sensores.append(dict(sensor, temperatura_c=temp, umidade_pct=float(umidade),
                                     indice=indice_calor(temp, umidade)))
            pontos = sensores + [dict(p, temperatura_c=idw(sensores, p['x'], p['y']),
                                      umidade_pct=float(umidade),
                                      indice=idw(sensores, p['x'], p['y'], 'indice')) for p in LUGARES]
            media = np.mean([p['indice'] for p in sensores])
            for ponto in pontos:
                linhas.append(dict(ponto, estacao=estacao, instante=instante,
                                   hora=h, dia=instante.day, origem='simulada', ambiente='Sala 7',
                                   tipo='sensor' if ponto['nome'] in [s['nome'] for s in SENSORES] else 'lugar',
                                   desvio_ponto=ponto['indice'] - media, nivel=nivel(ponto['indice'])))
    return pd.DataFrame(linhas)


def agrupar(dados):
    """Perfil médio de 24 h por ponto; estações analisadas separadamente."""
    resultados, avaliacoes = [], []
    for estacao, dados_estacao in dados.groupby('estacao', sort=True):
        perfil = dados_estacao.pivot_table(index='nome', columns='hora', values='indice')
        desvios = dados_estacao.groupby('nome').desvio_ponto
        if perfil.shape[1] != 24 or perfil.isna().any().any():
            raise ValueError('Cada ponto precisa de um perfil completo de 24 horas')
        candidatos = []
        for k in range(2, 5):
            grupos = KMeans(n_clusters=k, random_state=SEED, n_init=10).fit_predict(perfil)
            silhueta = silhouette_score(perfil, grupos)
            candidatos.append((silhueta, k, grupos))
            avaliacoes.append(dict(estacao=estacao, k=k, silhueta=silhueta))
        _, k, grupos = max(candidatos, key=lambda c: (c[0], -c[1]))
        for nome, grupo in zip(perfil.index, grupos):
            d = desvios.get_group(nome)
            cronico = 'quente' if (d > 0).all() else 'frio' if (d < 0).all() else 'variavel'
            resultados.append(dict(estacao=estacao, nome=nome, grupo=int(grupo), k=k,
                                   cronico=cronico, desvio_medio=float(d.mean())))
    return pd.DataFrame(resultados), pd.DataFrame(avaliacoes)


def prever_json(modelo, linha):
    if modelo['tipo'] == 'correcao_linear':
        return (prever_json(modelo['modelo_base'], linha) + modelo['intercepto'] +
                sum(modelo['coeficientes'][a] * linha[a] for a in modelo['atributos']))
    no = modelo['raiz']
    while 'valor' not in no:
        no = no['menor_ou_igual'] if linha[no['atributo']] <= no['limite'] else no['maior']
    return no['valor']


def preparar_previsao(dados):
    partes = []
    for (_, _), ponto in dados.groupby(['estacao', 'nome']):
        ponto = ponto.sort_values('instante').copy()
        ponto['alvo'] = ponto.indice.shift(-2)
        ponto['dia_alvo'] = ponto.dia.shift(-2)
        ponto['variacao_1h'] = ponto.temperatura_c.diff()
        for nome, valor, periodo in [('hora', ponto.hora, 24), ('mes', ponto.instante.dt.month, 12)]:
            ponto[nome + '_sen'] = np.sin(2 * np.pi * valor / periodo)
            ponto[nome + '_cos'] = np.cos(2 * np.pi * valor / periodo)
        ponto['indice_agora'] = ponto.indice
        partes.append(ponto)
    return pd.concat(partes).dropna(subset=['alvo', 'variacao_1h'])


def treinar_pontos(dados, modelo_base):
    dados = preparar_previsao(dados)
    treino = dados[(dados.dia <= 10) & (dados.dia_alvo <= 10)]
    teste = dados[dados.dia > 10]
    atributos = modelo_base['atributos'] + ['desvio_ponto']
    base_treino = np.array([prever_json(modelo_base, r) for r in treino.to_dict('records')])
    correcao = LinearRegression().fit(treino[atributos], treino.alvo - base_treino)
    modelo = dict(tipo='correcao_linear', versao=1, modelo_base=modelo_base,
                  alvo='indice_calor_c do ponto em 2 h', horizonte_horas=2,
                  fonte='Sala 7, dados simulados, seed 42, 14 dias por estação',
                  atributos=atributos, intercepto=float(correcao.intercept_),
                  coeficientes=dict(zip(atributos, map(float, correcao.coef_))))
    base_teste = np.array([prever_json(modelo_base, r) for r in teste.to_dict('records')])
    previstos = base_teste + correcao.predict(teste[atributos])
    metricas = []
    for nome, valores in [('persistencia', teste.indice), ('modelo_atual', base_teste),
                          ('modelo_atual_mais_desvio', base_teste + teste.desvio_ponto),
                          ('modelo_pontos', previstos)]:
        metricas.append(dict(modelo=nome, MAE=mean_absolute_error(teste.alvo, valores),
                             RMSE=float(np.sqrt(mean_squared_error(teste.alvo, valores))),
                             R2=r2_score(teste.alvo, valores),
                             F1_macro=f1_score(teste.alvo.map(nivel), [nivel(v) for v in valores], average='macro')))
    metricas = pd.DataFrame(metricas)
    modelo['metricas_teste'] = metricas.to_dict('records')
    modelo['divisao'] = dict(treino=len(treino), teste=len(teste),
                             regra='dias 1–10 treino, 11–14 teste por estação; alvo sem cruzar a fronteira')
    paridade = teste[atributos].copy()
    paridade['previsto'] = previstos
    return modelo, metricas, paridade


def exportar_modelo(modelo, caminho):
    caminho = Path(caminho)
    conteudo = json.dumps(modelo, ensure_ascii=False, indent=1, allow_nan=False)
    temporario = caminho.with_suffix(caminho.suffix + '.novo')
    temporario.write_text(conteudo, encoding='utf-8')
    temporario.replace(caminho)
