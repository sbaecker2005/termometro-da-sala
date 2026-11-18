"""Paridade do ajuste operacional e limites temporais do modelo compartilhado."""
from copy import deepcopy
from datetime import datetime, timedelta, timezone
import json
from pathlib import Path
import unittest

from ml.salas import ajuste_por_sala, demonstracao, prever_sala

PASTA = Path(__file__).parent
AGORA = datetime(2026, 11, 17, 15, 30, tzinfo=timezone.utc)
MODELO = {'tipo': 'arvore', 'atributos': [], 'raiz': {'valor': 30}}


def interna(segundos=0, **campos):
    return {'ambiente_id': 'sala-01', 'sensor': 'S1', 'indice': 32,
            'instante': AGORA + timedelta(seconds=segundos), **campos}


def externa(segundos=0, **campos):
    return {'indice': 28, 'temperatura_c': 28, 'umidade_pct': 50,
            'instante': AGORA + timedelta(seconds=segundos), **campos}


class SalasTestes(unittest.TestCase):
    def test_vetores_compartilhados_com_api(self):
        dados = json.loads((PASTA / 'casos-ajuste-salas.json').read_text(encoding='utf-8'))
        agora = datetime.fromisoformat(dados['agora'].replace('Z', '+00:00'))
        for caso in dados['casos']:
            with self.subTest(caso=caso['nome']):
                leituras = [dict(r, instante=agora + timedelta(seconds=r['segundos']), id=i)
                            for i, r in enumerate(caso['leituras'])]
                rua = [dict(r, instante=agora + timedelta(seconds=r['segundos']), id=i)
                       for i, r in enumerate(caso['rua'])]
                p = ajuste_por_sala(leituras, rua, caso['ambiente_id'], agora)
                self.assertEqual(p['delta_sala'], caso['delta_esperado'])
                self.assertEqual(p['persistencia_sala'], caso['persistencia_esperada'])
                self.assertEqual(p['minutos_pareados'], caso['minutos_pareados'])

    def test_mesmo_modelo_produz_ajustes_distintos_por_sala(self):
        leituras = [interna(s, ambiente_id=sala, indice=indice)
                    for s in (-120, -60, 0) for sala, indice in [('sala-01', 32), ('sala-02', 26)]]
        rua = [externa(-1800)]
        original = deepcopy(MODELO)
        a = prever_sala(MODELO, leituras, rua, 'sala-01', AGORA)
        b = prever_sala(MODELO, leituras, rua, 'sala-02', AGORA)
        self.assertEqual(a['indice_rua_previsto'], b['indice_rua_previsto'])
        self.assertEqual((a['indice_sala_previsto'], b['indice_sala_previsto']), (34, 28))
        self.assertEqual(MODELO, original)

    def test_horizonte_ancorado_na_medicao_externa(self):
        p = prever_sala(MODELO, [interna(s) for s in (-120, -60, 0)],
                        [externa(-1800)], 'sala-01', AGORA)
        self.assertEqual(p['previsto_para'], (AGORA + timedelta(minutes=90)).isoformat())
        self.assertEqual(p['base_rua_em'], (AGORA - timedelta(minutes=30)).isoformat())

    def test_rua_expira_e_persistencia_nao_pega_so_ultimo_sensor(self):
        leituras = [interna(-30, indice=30), interna(0, sensor='S2', indice=20)]
        p = prever_sala(MODELO, leituras, [externa(-3601)], 'sala-01', AGORA)
        self.assertEqual(p['metodo'], 'persistencia_sala')
        self.assertEqual(p['indice_sala_previsto'], 25)
        self.assertIsNone(p['indice_rua_previsto'])
        self.assertEqual(p['previsto_para'], (AGORA + timedelta(hours=2)).isoformat())

    def test_faltando_historico_nao_inventa_delta(self):
        p = prever_sala(MODELO, [interna()], [externa()], 'sala-01', AGORA)
        self.assertEqual(p['metodo'], 'modelo_rua_sem_ajuste')
        self.assertIsNone(p['indice_sala_previsto'])
        self.assertIsNone(p['delta_sala'])

    def test_sem_dados_nao_vira_sala_normal(self):
        p = prever_sala(MODELO, [], [], 'sala-01', AGORA)
        self.assertEqual(p['metodo'], 'sem_dados')
        self.assertIsNone(p['indice_sala_previsto'])
        self.assertIsNone(p['nivel_previsto'])
        self.assertIsNone(p['previsto_para'])

    def test_futuro_nao_altera_previsao_passada(self):
        leituras, rua = [interna(s) for s in (-120, -60, 0)], [externa(-1800)]
        antes = prever_sala(MODELO, leituras, rua, 'sala-01', AGORA)
        depois = prever_sala(MODELO, leituras + [interna(1, indice=55, origem='simulada')],
                            rua + [externa(1, indice=50)], 'sala-01', AGORA)
        self.assertEqual(antes, depois)

    def test_empate_de_horario_usa_id_e_nao_ordem_da_lista(self):
        leituras = [interna(s, id=i, indice=32) for i, s in enumerate((-120, -60, 0))]
        leituras.append(interna(0, id=10, indice=38))
        rua = [externa(-1800, id=1, indice=10), externa(-1800, id=2, indice=28)]
        a = ajuste_por_sala(leituras, rua, 'sala-01', AGORA)
        b = ajuste_por_sala(list(reversed(leituras)), list(reversed(rua)), 'sala-01', AGORA)
        self.assertEqual(a, b)
        self.assertEqual(a['delta_sala'], 6)

    def test_rejeita_fuso_ausente_e_entradas_invalidas(self):
        for agora in (AGORA.replace(tzinfo=None), '2026-11-17T15:30:00', None):
            with self.subTest(agora=agora), self.assertRaises(ValueError):
                ajuste_por_sala([], [], 'sala-01', agora)
        with self.assertRaises(ValueError):
            ajuste_por_sala([interna(suspeita='false')], [], 'sala-01', AGORA)
        self.assertEqual(prever_sala(MODELO, [], [externa(umidade_pct=101)],
                                    'sala-01', AGORA)['metodo'], 'sem_dados')
        self.assertIsNone(ajuste_por_sala([interna(indice=float('nan'))], [],
                                        'sala-01', AGORA)['persistencia_sala'])

    def test_overflow_em_indices_corrompidos_nao_quebra_nem_produz_infinito(self):
        leituras = [interna(s, sensor=sensor, indice=1e308)
                    for s in (-120, -60, 0) for sensor in ('S1', 'S2')]
        p = ajuste_por_sala(leituras, [externa(-1800, indice=0)], 'sala-01', AGORA)
        self.assertIsNone(p['delta_sala'])
        self.assertIsNone(p['persistencia_sala'])
        self.assertEqual(p['minutos_pareados'], 3)
        p = ajuste_por_sala(leituras, [externa(-1800, indice=-1e308)], 'sala-01', AGORA)
        self.assertIsNone(p['delta_sala'])
        self.assertEqual(p['minutos_pareados'], 0)
        self.assertIsNone(p['ultima_pareada_em'])
        json.dumps(p, allow_nan=False)
        self.assertEqual(prever_sala(MODELO, leituras, [], 'sala-01', AGORA)['metodo'], 'sem_dados')

    def test_ultima_rua_invalida_nao_substitui_observacao_valida(self):
        leituras = [interna(s) for s in (-120, -60, 0)]
        base = prever_sala(MODELO, leituras, [externa(-1800)], 'sala-01', AGORA)
        for mudanca in ({'temperatura_c': float('nan')}, {'indice': float('inf')},
                        {'umidade_pct': 101}, {'temperatura_c': 61}):
            with self.subTest(mudanca=mudanca):
                self.assertEqual(base, prever_sala(MODELO, leituras,
                                  [externa(-1800), externa(0, **mudanca)], 'sala-01', AGORA))

    def test_observacao_anterior_usa_mesmo_desempate_e_descarte_da_api(self):
        modelo = {'tipo': 'arvore', 'raiz': {'atributo': 'variacao_1h', 'limite': 2,
                  'menor_ou_igual': {'valor': 30}, 'maior': {'valor': 40}}}
        rua = [externa(-5400, id=1, temperatura_c=10),
               externa(-5400, id=2, temperatura_c=26.5),
               externa(-5400, id=3, temperatura_c=float('nan')),
               externa(-1800, temperatura_c=28)]
        p = prever_sala(modelo, [], rua, 'sala-01', AGORA)
        self.assertEqual(p['indice_rua_previsto'], 30)

    def test_previsao_final_arredonda_como_csharp(self):
        modelo = {'tipo': 'arvore', 'raiz': {'valor': 2.45}}
        leituras = [interna(s, indice=0) for s in (-120, -60, 0)]
        p = prever_sala(modelo, leituras, [externa(-1800, indice=0)], 'sala-01', AGORA)
        self.assertEqual((p['indice_rua_previsto'], p['indice_sala_previsto']), (2.4, 2.4))

    def test_horario_em_outro_fuso_e_o_mesmo_instante(self):
        leituras = [interna(s) for s in (-120, -60, 0)]
        rua = [externa(-1800)]
        utc = ajuste_por_sala(leituras, rua, 'sala-01', AGORA)
        for r in leituras + rua:
            r['instante'] = r['instante'].astimezone(timezone(timedelta(hours=-3))).isoformat()
        self.assertEqual(utc, ajuste_por_sala(leituras, rua, 'sala-01', AGORA))

    def test_demonstracao_com_modelo_real_e_ocupacao_sem_coeficiente_inventado(self):
        p = demonstracao()
        self.assertEqual(p['ocupantes']['potencia_total_w'], 2286)
        self.assertEqual(p['minutos_pareados'], 3)
        self.assertEqual(p['delta_sala'], 4)
        self.assertEqual(p['indice_rua_previsto'], 27.2)
        self.assertEqual(p['indice_sala_previsto'], 31.2)
        self.assertTrue(p['demonstracao'])
        self.assertFalse(p['validado_em_campo'])
        leituras, rua = [interna(s) for s in (-120, -60, 0)], [externa(-1800)]
        sem = prever_sala(MODELO, leituras, rua, 'sala-01', AGORA)
        com = prever_sala(MODELO, leituras, rua, 'sala-01', AGORA, p['ocupantes']['grupos'])
        self.assertEqual(sem['indice_sala_previsto'], com['indice_sala_previsto'])


if __name__ == '__main__':
    unittest.main()
