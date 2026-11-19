"""Casos físicos e de dados ausentes do estimador offline, sem novas dependências."""
import math
import unittest

from ml.ocupantes import estimar_ocupantes, exemplo_terceirao


def grupo(**mudancas):
    return {'grupo': 'participantes', 'perfil_etario': 'não definido',
            'atividade': 'sentados', 'quantidade': 2, 'potencia_por_pessoa_w': 108,
            'referencia': 'hipótese explícita de teste, não medição', **mudancas}


class OcupantesTestes(unittest.TestCase):
    def test_demonstracao_20_alunos_e_professor(self):
        resultado = exemplo_terceirao()
        self.assertEqual(resultado['pessoas'], 21)
        self.assertEqual(resultado['potencia_total_w'], 2286)
        self.assertEqual([g['potencia_grupo_w'] for g in resultado['grupos']], [2160, 126])
        self.assertFalse(resultado['validado_em_campo'])
        self.assertEqual(resultado['origem'], 'estimativa')

    def test_presenca_e_atividade_alteram_potencia(self):
        # Dez lendo (99 W), dez sentados (108 W), um em pé (126 W).
        resultado = estimar_ocupantes([grupo(quantidade=10, potencia_por_pessoa_w=99),
                                      grupo(quantidade=10),
                                      grupo(quantidade=1, potencia_por_pessoa_w=126)])
        self.assertEqual(resultado['potencia_total_w'], 2196)

    def test_sala_vazia_conhecida(self):
        self.assertEqual(estimar_ocupantes([])['potencia_total_w'], 0)
        resultado = estimar_ocupantes([grupo(quantidade=0, potencia_por_pessoa_w=None)])
        self.assertEqual(resultado['potencia_total_w'], 0)
        self.assertTrue(resultado['completo'])

    def test_grupo_presente_sem_referencia_nao_vira_zero_nem_adulto(self):
        resultado = estimar_ocupantes([grupo(perfil_etario='berçário',
                                             potencia_por_pessoa_w=None, referencia=None),
                                      grupo(quantidade=1, potencia_por_pessoa_w=126)])
        self.assertEqual(resultado['pessoas'], 3)
        self.assertIsNone(resultado['potencia_total_w'])
        self.assertIsNone(resultado['grupos'][0]['potencia_grupo_w'])
        self.assertEqual(resultado['grupos'][1]['potencia_grupo_w'], 126)
        self.assertFalse(resultado['completo'])

    def test_idade_nao_escolhe_coeficiente(self):
        for perfil in ('berçário', '17–18 anos', 'adultos', 'não definido'):
            with self.subTest(perfil=perfil):
                resultado = estimar_ocupantes([grupo(perfil_etario=perfil,
                                                     potencia_por_pessoa_w=None)])
                self.assertIsNone(resultado['potencia_total_w'])

    def test_recusa_presenca_desconhecida_fracionaria_negativa_ou_booleana(self):
        for quantidade in (None, -1, 1.5, True, '20', float('nan'), float('inf')):
            with self.subTest(quantidade=quantidade), self.assertRaises(ValueError):
                estimar_ocupantes([grupo(quantidade=quantidade)])

    def test_recusa_potencia_invalida_e_overflow(self):
        for potencia in (-1, 0, True, '108', math.nan, math.inf, -math.inf, 10 ** 400):
            with self.subTest(potencia=potencia), self.assertRaises(ValueError):
                estimar_ocupantes([grupo(potencia_por_pessoa_w=potencia)])
        with self.assertRaises(ValueError):
            estimar_ocupantes([grupo(potencia_por_pessoa_w=1e308)])

    def test_recusa_entrada_sem_contexto_ou_origem(self):
        for entrada in (None, {}, [None], [grupo(referencia=' ')],
                        [grupo(perfil_etario='')], [grupo(atividade=None)],
                        [grupo(grupo='')]):
            with self.subTest(entrada=entrada), self.assertRaises(ValueError):
                estimar_ocupantes(entrada)


if __name__ == '__main__':
    unittest.main()
