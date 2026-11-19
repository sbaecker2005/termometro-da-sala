"""Validação independente em 2026 (jan-out): aplica o modelo.json congelado, sem retreino.

Uso (da raiz ou de ml/): python -B ml/avaliar_2026.py
Lê dados/inmet_a701_2026.csv (gerado por extrair_a701.py a partir do CSV oficial
do INMET) e grava avaliacao-2026.json. Reaproveita as células do notebook:
leitura e checagens do CSV, calendário de buracos, índice de calor, montagem dos
atributos/alvo, avalia(), prever_json() e a árvore anterior. O histórico de 2025
entra só como defasagem: a hora anterior a 01/01/2026 00h UTC (variacao_1h).
"""
import ast
import contextlib
import hashlib
import io
import json
import os
from pathlib import Path

PASTA = Path(__file__).resolve().parent
SHA256_MODELO = 'c5c4f1fc38e33088fa12ed4e7ed5bde2a5ac40de1f1927fb8604e03521aaf7bd'
INICIO, FIM = '2026-01-01', '2026-11-01'  # calendário UTC avaliado: 01/01 a 31/10/2026
LIMIAR_ALERTA = 32  # cuidado ou perigo, como em alertas.csv


def sha256_modelo():
    return hashlib.sha256((PASTA / 'modelo.json').read_bytes()).hexdigest()


def _celula(notebook, inicio):
    fonte, = [''.join(c['source']) for c in notebook['cells']
              if c['cell_type'] == 'code' and ''.join(c['source']).startswith(inicio)]
    return fonte


def _trechos(fonte, nomes):
    """Só as funções/atribuições nomeadas de uma célula (sem rodar a grade nem exportar)."""
    def nome(n):
        if isinstance(n, ast.FunctionDef):
            return n.name
        if isinstance(n, ast.Assign) and isinstance(n.targets[0], ast.Name):
            return n.targets[0].id
    corpo = [n for n in ast.parse(fonte).body if nome(n) in nomes]
    assert sorted(nome(n) for n in corpo) == sorted(nomes), nomes
    return compile(ast.Module(body=corpo, type_ignores=[]), '<notebook>', 'exec')


def avaliar():
    assert sha256_modelo() == SHA256_MODELO, 'modelo.json mudou'
    notebook = json.loads((PASTA / 'termometro_da_sala.ipynb').read_text(encoding='utf-8'))
    leitura = _celula(notebook, 'import json, math')
    calendario = _celula(notebook, 'calendario = pd.date_range')
    original = "PASTA / 'dados/inmet_a701.csv'"
    assert leitura.count(original) == 1 and calendario.count("'2022-01-01', '2026-01-01'") == 1
    c = {}
    anterior = Path.cwd()
    try:
        os.chdir(PASTA)
        with contextlib.redirect_stdout(io.StringIO()):
            exec(compile(leitura, 'leitura', 'exec'), c)  # 2022-2025, com as checagens do notebook
            historico = c['df']
            exec(compile(leitura.replace(original, "PASTA / 'dados/inmet_a701_2026.csv'"), 'leitura 2026', 'exec'), c)
            novo = c['df']
            c['display'] = lambda *a, **k: None
            exec(compile(calendario.replace("'2022-01-01', '2026-01-01'", f"'{INICIO}', '{FIM}'"),
                         'calendário 2026', 'exec'), c)
            assert historico.index.max() < novo.index.min()
            c['df'] = c['pd'].concat([historico, novo])
            exec(compile(_celula(notebook, 'def indice_calor_c'), 'índice', 'exec'), c)
            exec(compile(_celula(notebook, 'uma_h, duas_h'), 'atributos', 'exec'), c)
            fonte_modelos = _celula(notebook, 'def avalia')
            exec(_trechos(fonte_modelos, ['avalia', 'atributos_antes', 'arvore_antes']), c)
            exec(_trechos(_celula(notebook, 'def prever_json'), ['prever_json']), c)
    finally:
        os.chdir(anterior)

    np, dados = c['np'], c['dados']
    # O treino da árvore anterior não muda com 2026 (mesmos exemplos do notebook).
    assert (len(c['treino']), len(c['valid'])) == (17192, 8740)
    modelo = json.loads((PASTA / 'modelo.json').read_text(encoding='utf-8'))
    assert modelo['atributos'] == c['ATRIBUTOS']
    teste = dados[dados.index.year == 2026]
    previsoes = {
        'baseline (agora)': teste.indice_agora.to_numpy(),
        'árvore anterior': c['arvore_antes'].predict(teste[c['atributos_antes']]),
        'árvore exportada': np.array([c['prever_json'](modelo, r) for r in teste.to_dict('records')]),
    }
    real = teste.alvo.to_numpy()
    alerta_real = real >= LIMIAR_ALERTA
    alertas = []
    for nome, p in previsoes.items():
        previsto = p >= LIMIAR_ALERTA
        tp, fp, fn = (int(np.sum(alerta_real & previsto)), int(np.sum(~alerta_real & previsto)),
                      int(np.sum(alerta_real & ~previsto)))
        alertas.append({'modelo': nome, 'TP': tp, 'FP': fp, 'FN': fn,
                        'precisão': tp / (tp + fp) if tp + fp else 0.0,
                        'recall': tp / (tp + fn) if tp + fn else 0.0})
    niveis_reais = teste.alvo.map(c['nivel']).value_counts().reindex(c['NIVEIS'], fill_value=0)
    buracos = c['buracos']
    resultado = {
        'fonte': 'INMET, estação automática A701 (São Paulo, Mirante de Santana), CSV horário oficial de 2026',
        'periodo_utc': f'{INICIO}T00:00Z a 2026-10-31T23:00Z',
        'sha256_modelo': SHA256_MODELO,
        'horas_calendario': int(c['por_ano'].esperadas.sum()),
        'horas_validas': len(novo),
        'horas_ausentes_ou_descartadas': int(c['por_ano'].ausentes_ou_descartadas.sum()),
        'buracos': len(buracos),
        'maior_buraco_h': int(buracos['size'].max()) if len(buracos) else 0,
        'sem_vizinhos_ou_alvo': len(novo) - len(teste),
        'exemplos_avaliados': len(teste),
        'exemplos_com_defasagem_de_2025': int((teste.index == c['pd'].Timestamp(INICIO, tz='UTC')).sum()),
        'niveis_reais': {k: int(v) for k, v in niveis_reais.items()},
        'regressao': [c['avalia'](n, p, teste.alvo) for n, p in previsoes.items()],
        'alerta': alertas,
    }
    assert sha256_modelo() == SHA256_MODELO
    return resultado


def main():
    resultado = avaliar()
    (PASTA / 'avaliacao-2026.json').write_text(json.dumps(resultado, ensure_ascii=False, indent=1), encoding='utf-8')
    print(json.dumps(resultado, ensure_ascii=False, indent=1))


if __name__ == '__main__':
    main()
