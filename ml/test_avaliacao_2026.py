"""Recalcula a validação independente de 2026 e confere avaliacao-2026.json e o modelo congelado."""
import hashlib
import json
from pathlib import Path
import unittest

from avaliar_2026 import SHA256_MODELO, avaliar

PASTA = Path(__file__).resolve().parent


class Avaliacao2026Testes(unittest.TestCase):
    def test_modelo_congelado(self):
        self.assertEqual(SHA256_MODELO, hashlib.sha256((PASTA / 'modelo.json').read_bytes()).hexdigest())

    def test_numeros_gravados(self):
        gravado = json.loads((PASTA / 'avaliacao-2026.json').read_text(encoding='utf-8'))
        calculado = json.loads(json.dumps(avaliar(), ensure_ascii=False))
        self.assertEqual(gravado, calculado)
        self.assertEqual(SHA256_MODELO, gravado['sha256_modelo'])
        # Contas fechadas: calendário, exemplos e alerta.
        self.assertEqual(gravado['horas_calendario'], gravado['horas_validas'] + gravado['horas_ausentes_ou_descartadas'])
        self.assertEqual(gravado['horas_validas'], gravado['exemplos_avaliados'] + gravado['sem_vizinhos_ou_alvo'])
        criticas = gravado['niveis_reais']['cuidado'] + gravado['niveis_reais']['perigo']
        for linha in gravado['alerta']:
            with self.subTest(modelo=linha['modelo']):
                self.assertEqual(criticas, linha['TP'] + linha['FN'])


if __name__ == '__main__':
    unittest.main()
