"""Indice de calor da NOAA (Rothfusz), entrada em graus Celsius e umidade relativa em %.

Referencia: https://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml
E a mesma conta da API (C#) e do ESP32 (C++); os casos do final conferem as tres.
"""
import math


def indice_calor_c(temp_c: float, umidade: float) -> float:
    t = temp_c * 9 / 5 + 32
    rh = umidade
    simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094)
    if (simples + t) / 2 < 80:
        hi = simples
    else:
        hi = (-42.379 + 2.04901523 * t + 10.14333127 * rh - 0.22475541 * t * rh
              - 0.00683783 * t * t - 0.05481717 * rh * rh + 0.00122874 * t * t * rh
              + 0.00085282 * t * rh * rh - 0.00000199 * t * t * rh * rh)
        if rh < 13 and 80 <= t <= 112:
            hi -= ((13 - rh) / 4) * math.sqrt((17 - abs(t - 95)) / 17)
        elif rh > 85 and 80 <= t <= 87:
            hi += ((rh - 85) / 10) * ((87 - t) / 5)
    return round((hi - 32) * 5 / 9, 1)


def nivel(indice_c: float) -> str:
    if indice_c < 18:
        return "frio"
    if indice_c < 27:
        return "normal"
    if indice_c < 32:
        return "atencao"
    if indice_c < 41:
        return "cuidado"
    return "perigo"


CASOS = [  # (temperatura C, umidade %, indice C esperado, nivel); C# e C++ conferem com tolerancia de 0.1
    (24.0, 40, 23.5, "normal"),
    (30.0, 50, 31.0, "atencao"),
    (33.4, 58, 39.8, "cuidado"),
    (38.0, 70, 62.5, "perigo"),
]

if __name__ == "__main__":
    assert nivel(17.9) == "frio" and nivel(18.0) == "normal" and nivel(27.0) == "atencao"
    for t, u, esperado, n in CASOS:
        i = indice_calor_c(t, u)
        assert i == esperado and nivel(i) == n, (t, u, i)
        print(f"{t:.1f} C, {u}% -> indice {i:.1f} C, nivel {nivel(i)}")
