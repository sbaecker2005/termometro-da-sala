"""Curva sintética de uma sala, independente da regra de previsão por delta."""
import csv
import math
from pathlib import Path
import random


def gerar(face="oeste", andar=2):
    """Janela local 12h–15h; minutos_antes=0 é a referência para ancoragem.

    Face leste recebe sol pela manhã; oeste, à tarde. Cada andar aumenta
    o ganho solar em 15%. Temperatura e ganho respondem à curva de uma hora
    antes (atraso térmico). São hipóteses didáticas, não calibração física.
    """
    if face not in ("leste", "oeste") or not isinstance(andar, int) or andar < 0:
        raise ValueError("Use face leste/oeste e andar inteiro não negativo")
    ruido = random.Random(2026)
    leituras = []
    for minutos_antes in range(180, -1, -10):
        hora = 15 - minutos_antes / 60 - 1
        rua = 28 + 4 * math.cos((hora - 14) * math.pi / 12)
        pico_sol = 9 if face == "leste" else 15
        sol = max(0, math.cos((hora - pico_sol) * math.pi / 12))
        ganho = 2.5 * (1 + 0.15 * andar) * sol
        temperatura = round(rua + ganho + ruido.uniform(-0.2, 0.2), 1)
        umidade = round(62 - (temperatura - 28) * 1.5 + ruido.uniform(-1, 1))
        leituras.append((minutos_antes, temperatura, umidade))
    return leituras


if __name__ == "__main__":
    linhas = gerar()
    assert linhas == gerar()
    assert [m for m, _, _ in linhas] == list(range(180, -1, -10))
    assert all(-10 <= t <= 60 and 0 <= u <= 100 for _, t, u in linhas)
    assert linhas[-1][1] > gerar("leste", 2)[-1][1]
    assert linhas[-1][1] > gerar("oeste", 0)[-1][1]
    destino = Path(__file__).with_name("leituras-simuladas.csv")
    with destino.open("w", newline="", encoding="utf-8") as arquivo:
        escritor = csv.writer(arquivo)
        escritor.writerow(("minutos_antes", "temperaturaC", "umidadePct"))
        escritor.writerows((m, f"{t:.1f}", u) for m, t, u in linhas)
    print(f"{destino.name}: {len(linhas)} leituras, 180 a 0 minutos")
