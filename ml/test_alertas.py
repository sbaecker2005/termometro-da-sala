"""Regressão offline do alerta da rua, com o CSV real e o modelo exportado."""
import ast
import json
import os
from pathlib import Path
import unittest

import numpy as np
import pandas as pd
from sklearn.tree import DecisionTreeRegressor


class AlertasTestes(unittest.TestCase):
    def test_previsao_fria_e_bordas(self):
        pasta = Path(__file__).resolve().parent
        notebook = json.loads((pasta / 'termometro_da_sala.ipynb').read_text(encoding='utf-8'))
        fonte, = [''.join(c['source']) for c in notebook['cells']
                  if c['cell_type'] == 'code' and ''.join(c['source']).startswith('def indice_calor_c')]
        # Executa só o classificador existente, sem preparar dados nem treinar.
        funcao, = [n for n in ast.parse(fonte).body if isinstance(n, ast.FunctionDef) and n.name == 'nivel']
        contexto = {}
        exec(compile(ast.Module(body=[funcao], type_ignores=[]), '<nivel>', 'exec'), contexto)
        for previsto, esperado in [(17.9, 'frio'), (18.0, 'normal'), (27.0, 'atencao')]:
            with self.subTest(previsto=previsto):
                self.assertEqual(esperado, contexto['nivel'](previsto))

    def test_precisao_recall_antes_e_depois(self):
        pasta = Path(__file__).resolve().parent
        self.assertTrue((pasta / 'dados/inmet_a701.csv').is_file())
        notebook = json.loads((pasta / 'termometro_da_sala.ipynb').read_text(encoding='utf-8'))
        contexto = {}
        # Reutiliza só preparação, fórmula e divisão temporal; não executa a grade nem exporta arquivos.
        inicios = ('import json, math', 'def indice_calor_c', 'uma_h, duas_h')
        anterior = Path.cwd()
        try:
            os.chdir(pasta)
            for inicio in inicios:
                fonte, = [''.join(c['source']) for c in notebook['cells']
                          if c['cell_type'] == 'code' and ''.join(c['source']).startswith(inicio)]
                exec(compile(fonte, str(pasta / 'termometro_da_sala.ipynb'), 'exec'), contexto)
        finally:
            os.chdir(anterior)

        atributos = [a for a in contexto['ATRIBUTOS'] if a != 'indice_agora']
        treino = contexto['treino']
        antes = DecisionTreeRegressor(max_depth=8, min_samples_leaf=30, random_state=42).fit(
            treino[atributos], treino.alvo)
        raiz = json.loads((pasta / 'modelo.json').read_text(encoding='utf-8'))['raiz']
        esperado = pd.read_csv(pasta / 'alertas.csv').set_index(['período', 'modelo'])
        self.assertEqual(6, len(esperado))
        for periodo, conjunto in [('validação 2024', contexto['valid']), ('teste 2025', contexto['teste'])]:
            depois = []
            for linha in conjunto.to_dict('records'):
                no = raiz
                while 'valor' not in no:
                    no = no['menor_ou_igual'] if linha[no['atributo']] <= no['limite'] else no['maior']
                depois.append(no['valor'])
            real = conjunto.alvo.to_numpy() >= 32
            for nome, valores in [('baseline (agora)', conjunto.indice_agora),
                                  ('árvore anterior', antes.predict(conjunto[atributos])),
                                  ('árvore ajustada', depois)]:
                with self.subTest(periodo=periodo, modelo=nome):
                    previsto = np.asarray(valores) >= 32
                    tp = int(np.sum(real & previsto))
                    fp = int(np.sum(~real & previsto))
                    fn = int(np.sum(real & ~previsto))
                    referencia = esperado.loc[(periodo, nome)]
                    self.assertEqual((tp, fp, fn), tuple(referencia[['TP', 'FP', 'FN']]))
                    self.assertAlmostEqual(tp / (tp + fp), referencia['precisão'])
                    self.assertAlmostEqual(tp / (tp + fn), referencia['recall'])


if __name__ == '__main__':
    unittest.main()
