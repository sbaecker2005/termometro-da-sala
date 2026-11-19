"""Conferências offline do mapa e da previsão com dados simulados."""
import json
import math
from pathlib import Path

import numpy as np
import pandas as pd
import pytest

from ml.mapa import (LUGARES, SENSORES, agrupar, exportar_modelo, grade, idw,
                     leituras_recentes, nivel, precaucoes, preparar_previsao,
                     prever_json, simular, treinar_pontos)

PASTA = Path(__file__).resolve().parent


@pytest.fixture(scope='module')
def dados():
    notebook = json.loads((PASTA / 'termometro_da_sala.ipynb').read_text(encoding='utf-8'))
    fonte, = [''.join(c['source']) for c in notebook['cells']
              if c['cell_type'] == 'code' and ''.join(c['source']).startswith('def indice_calor_c')]
    contexto = {'math': math}
    exec(fonte.split('NIVEIS =')[0], contexto)
    return simular(contexto['indice_calor_c'])


def test_idw_caso_compartilhado():
    caso = json.loads((PASTA / 'caso-idw.json').read_text(encoding='utf-8'))
    for ponto in caso['casos']:
        assert idw(caso['sensores'], ponto['x'], ponto['y']) == pytest.approx(ponto['esperado'])
    assert idw([], 0, 0) is None
    assert idw([{'temperatura_c': np.nan}], 0, 0) is None
    assert idw(caso['sensores'][:1], 7, 9) == 20
    with pytest.raises(ValueError):
        idw(caso['sensores'], np.nan, 0)


def test_grade_e_sensor_exato():
    sensores = [dict(s, temperatura_c=20 + i, indice=19 + i) for i, s in enumerate(SENSORES)]
    mapa = grade(sensores)
    assert len(mapa) == 15 * 19
    assert mapa.x.min() == 0 and mapa.x.max() == 7
    assert mapa.y.min() == 0 and mapa.y.max() == 9
    for sensor in sensores:
        celula = mapa[(mapa.x == sensor['x']) & (mapa.y == sensor['y'])].iloc[0]
        assert celula.temperatura_c == sensor['temperatura_c']
        assert celula.indice == sensor['indice']


def test_leituras_recentes():
    agora = pd.Timestamp('2026-01-01 12:00', tz='UTC')
    leituras = [dict(nome=nome, instante=agora - pd.Timedelta(minutes=idade),
                    x=x, y=0, temperatura_c=valor)
                for nome, idade, x, valor in [('A', 15, 0, 20), ('B', 16, 2, 50),
                                              ('A', 1, 0, 22), ('C', -1, 3, 60),
                                              ('D', 15, 2, 26)]]
    recentes = leituras_recentes(leituras, agora)
    assert {p['nome'] for p in recentes} == {'A', 'D'}
    assert idw(recentes, 1, 0) == 24


@pytest.mark.parametrize('valor,esperado', [(17.999, 'frio'), (18, 'normal'), (26.999, 'normal'),
                                          (27, 'atencao'), (32, 'cuidado'), (41, 'perigo')])
def test_faixas(valor, esperado):
    assert nivel(valor) == esperado


@pytest.mark.parametrize('indice', [17.9, 32, 41])
def test_troca_vulneravel(indice):
    avisos = precaucoes([dict(nome='A', indice=indice, vulneravel=True),
                        dict(nome='B', indice=24), dict(nome='C', indice=29)])
    assert any('trocar A para B' in a for a in avisos)
    assert not any('trocar' in a for a in precaucoes([dict(nome='A', indice=indice)]))


def test_precaucoes_bordas_e_sem_alternativa():
    assert not precaucoes([dict(nome='A', indice=23), dict(nome='B', indice=26.999)])
    assert 'ambiente desigual' in precaucoes([dict(nome='A', indice=23), dict(nome='B', indice=27)])[0]
    assert not precaucoes([dict(nome='A', indice=18, vulneravel=True)])
    assert 'sem lugar mais seguro' in precaucoes([dict(nome='A', indice=16, vulneravel=True)])[0]
    assert 'sem lugar mais seguro' in precaucoes([dict(nome='A', indice=16, vulneravel=True),
                                               dict(nome='B', indice=35)])[0]
    assert not precaucoes([dict(nome='A', indice=None, vulneravel=True)])
    assert 'para B' in precaucoes([dict(nome='A', indice=32, vulneravel=True),
                                   dict(nome='C', indice=27), dict(nome='B', indice=26)])[0]


def test_extremos_incluem_sensores_sem_oferecer_sensor_como_destino():
    avisos = precaucoes([dict(nome='janela', indice=36, tipo='sensor'),
                        dict(nome='centro', indice=24, tipo='sensor'),
                        dict(nome='A', indice=32, vulneravel=True, tipo='lugar'),
                        dict(nome='B', indice=30, tipo='lugar')])
    assert 'trocar A para B' in avisos[0]
    assert 'janela' in avisos[1] and 'centro' in avisos[1] and '12.0' in avisos[1]


def test_simulacao_e_efeitos(dados):
    assert len(dados) == 2 * 14 * 24 * (len(SENSORES) + len(LUGARES))
    assert dados.origem.eq('simulada').all()
    assert not dados.duplicated(['estacao', 'nome', 'instante']).any()
    assert dados[dados.tipo == 'sensor'].groupby(['estacao', 'instante']).size().eq(4).all()
    assert dados[dados.tipo == 'lugar'].groupby(['estacao', 'instante']).vulneravel.sum().eq(2).all()
    assert dados.groupby(['estacao', 'nome']).instante.diff().dropna().eq(pd.Timedelta(hours=1)).all()
    sensores = dados[dados.tipo == 'sensor']
    verao = sensores[(sensores.estacao == 'verao') & (sensores.hora == 15)].groupby('nome').temperatura_c.mean()
    inverno = sensores[(sensores.estacao == 'inverno') & (sensores.hora == 4)].groupby('nome').temperatura_c.mean()
    assert verao['janela'] - verao['centro'] > 4
    assert inverno['centro'] - inverno['fundo'] > 3


def test_agrupamento_deterministico(dados):
    grupos, silhuetas = agrupar(dados)
    pd.testing.assert_frame_equal(grupos, pd.read_csv(PASTA / 'grupos-pontos.csv'))
    outros, outras = agrupar(dados.sample(frac=1, random_state=7))
    pd.testing.assert_frame_equal(grupos, outros)
    np.testing.assert_allclose(silhuetas.silhueta, outras.silhueta, atol=1e-12)
    for estacao, parte in silhuetas.groupby('estacao'):
        escolhido = parte.loc[parte.silhueta.idxmax(), 'k']
        assert grupos[grupos.estacao == estacao].k.eq(escolhido).all()
    janela = grupos[grupos.nome == 'janela']
    fundo = grupos[grupos.nome == 'fundo']
    assert janela.cronico.eq('quente').all()
    assert fundo.cronico.eq('frio').all()


@pytest.mark.parametrize('sinal,excecao', [(1, -.175), (-1, .175), (1, 0)])
def test_cronico_exige_todas_as_leituras(dados, sinal, excecao):
    amostra = dados[dados.estacao == 'inverno'].copy()
    centro = amostra.nome == 'centro'
    amostra.loc[centro, 'desvio_ponto'] = sinal
    primeiro = amostra[centro & (amostra.hora == 12)].index[0]
    amostra.loc[primeiro, 'desvio_ponto'] = excecao
    medias = amostra[centro].groupby('hora').desvio_ponto.mean()
    assert (medias * sinal > 0).all()
    grupos, _ = agrupar(amostra)
    assert grupos[grupos.nome == 'centro'].cronico.item() == 'variavel'


def test_celula_exporta_sem_pasta_dados(dados, tmp_path, monkeypatch):
    notebook = json.loads((PASTA / 'termometro_da_sala.ipynb').read_text(encoding='utf-8'))
    fonte, = [''.join(c['source']) for c in notebook['cells']
              if c['cell_type'] == 'code' and ''.join(c['source']).startswith('simulados =')]
    monkeypatch.chdir(tmp_path)
    contexto = dict(PASTA=Path('.'), simular=lambda _: dados.copy(), indice_calor_c=None,
                    agrupar=agrupar, display=lambda _: None)
    assert not Path('dados').exists()
    for _ in range(2):
        exec(compile(fonte, 'celula_simulados', 'exec'), contexto)
        assert len(pd.read_csv('dados/sala7-simulada.csv')) == len(dados)
        grupos = pd.read_csv('grupos-pontos.csv')
        centro = grupos[(grupos.estacao == 'inverno') & (grupos.nome == 'centro')]
        assert centro.cronico.item() == 'variavel'


def test_exportacao_e_divisao_temporal(dados, tmp_path):
    original = (PASTA / 'modelo.json').read_bytes()
    modelo, metricas, paridade = treinar_pontos(dados, json.loads(original))
    caminho = tmp_path / 'modelo_pontos.json'
    exportar_modelo(modelo, caminho)
    exportado = json.loads(caminho.read_text(encoding='utf-8'))
    assert exportado['atributos'][-1] == 'desvio_ponto'
    assert exportado['modelo_base'] == json.loads(original)
    assert original == (PASTA / 'modelo.json').read_bytes()
    valores = [prever_json(exportado, r) for r in paridade.to_dict('records')]
    np.testing.assert_allclose(valores, paridade.previsto, atol=1e-9, rtol=0)
    salvo = json.loads((PASTA / 'modelo_pontos.json').read_text(encoding='utf-8'))
    np.testing.assert_allclose([prever_json(salvo, r) for r in paridade.to_dict('records')],
                               paridade.previsto, atol=1e-9, rtol=0)
    pd.testing.assert_frame_equal(metricas, pd.read_csv(PASTA / 'metricas-pontos.csv'))
    preparado = preparar_previsao(dados)
    assert modelo['divisao']['treino'] == len(preparado[(preparado.dia <= 10) & (preparado.dia_alvo <= 10)])
    assert modelo['divisao']['teste'] == len(preparado[preparado.dia > 10])
