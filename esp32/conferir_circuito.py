"""Confere o diagram.json contra o sketch.ino sem abrir o Wokwi.

Uso: python esp32/conferir_circuito.py
Verifica: JSON válido, peças e pinos existentes, cada GPIO do sketch ligado à
peça certa (direto ou por um resistor), retorno pelo GND, nenhum GPIO em curto
com outro e uma linha no libraries.txt para cada biblioteca incluída.
Não substitui a compilação, que fica com o Wokwi.
"""
import json
import re
from pathlib import Path

PASTA = Path(__file__).resolve().parent

# Pinos de cada peça usada (documentação das peças do Wokwi).
PINOS = {
    "wokwi-esp32-devkit-v1": {
        "EN", "VP", "VN", "D34", "D35", "D32", "D33", "D25", "D26", "D27", "D14",
        "D12", "D13", "GND.2", "VIN", "3V3", "GND.1", "D15", "D2", "D4", "RX2",
        "TX2", "D5", "D18", "D19", "D21", "RX0", "TX0", "D22", "D23",
    },
    "wokwi-dht22": {"VCC", "SDA", "NC", "GND"},
    "wokwi-led": {"A", "C"},
    "wokwi-resistor": {"1", "2"},
    "wokwi-lcd1602": {"GND", "VCC", "SDA", "SCL"},
    "wokwi-buzzer": {"1", "2"},
    "wokwi-pushbutton": {"1.l", "2.l", "1.r", "2.r"},
}

# Constante do sketch -> pino da peça que ela comanda.
ALVO = {
    "PIN_DHT": "dht:SDA",
    "VERDE": "verde:A",
    "AMARELO": "amarelo:A",
    "VERMELHO": "vermelho:A",
    "BUZZER": "buzzer:2",
    "BOTAO": "ciente:1.l",
}

# Pinos que precisam voltar pelo GND (cátodos, buzzer, botão e alimentação).
NO_GND = ["dht:GND", "verde:C", "amarelo:C", "vermelho:C", "lcd:GND", "buzzer:1", "ciente:2.l"]

# Cabeçalho incluído -> nome no libraries.txt (o resto vem do core ESP32).
BIBLIOTECA = {
    "DHT.h": "DHT sensor library",
    "PubSubClient.h": "PubSubClient",
    "LiquidCrystal_I2C.h": "LiquidCrystal I2C",
}
DO_CORE = {"WiFi.h", "time.h", "esp_sntp.h", "mbedtls/md.h", "assert.h", "math.h", "secrets.h"}


def redes(conexoes):
    """Agrupa os pinos ligados entre si (cada grupo é um nó elétrico)."""
    pai = {}

    def raiz(p):
        pai.setdefault(p, p)
        while pai[p] != p:
            pai[p] = pai[pai[p]]
            p = pai[p]
        return p

    for a, b, *_ in conexoes:
        pai[raiz(a)] = raiz(b)
    grupos = {}
    for p in list(pai):
        grupos.setdefault(raiz(p), set()).add(p)
    return {p: grupos[raiz(p)] for p in pai}


def conferir():
    # Com python -O os assert somem e o script aprovaria qualquer circuito.
    if not __debug__:
        raise SystemExit("Rode sem -O: as conferencias usam assert")
    diagrama = json.loads((PASTA / "diagram.json").read_text(encoding="utf-8"))
    sketch = (PASTA / "sketch.ino").read_text(encoding="utf-8")
    libs = {l.strip() for l in (PASTA / "libraries.txt").read_text(encoding="utf-8").splitlines() if l.strip()}
    feitas = 0

    tipos = {p["id"]: p["type"] for p in diagrama["parts"]}
    assert len(tipos) == len(diagrama["parts"]), "id de peca repetido"
    for a, b, *_ in diagrama["connections"]:
        for ponta in (a, b):
            peca, pino = ponta.split(":", 1)
            if peca == "$serialMonitor":
                continue
            assert peca in tipos, f"peca inexistente: {ponta}"
            assert pino in PINOS[tipos[peca]], f"pino inexistente: {ponta}"
            feitas += 1
    rede = redes(diagrama["connections"])

    def ligado(origem, destino):
        """Mesmo nó, ou um resistor com uma ponta em cada nó."""
        if destino in rede.get(origem, ()):
            return True
        for rid, tipo in tipos.items():
            if tipo == "wokwi-resistor":
                r1, r2 = f"{rid}:1", f"{rid}:2"
                for x, y in ((r1, r2), (r2, r1)):
                    if x in rede.get(origem, ()) and destino in rede.get(y, ()):
                        return True
        return False

    pinos = dict(re.findall(r"(\w+) = (\d+)", re.search(r"constexpr int ([^;]+);", sketch).group(1)))
    assert set(pinos) == set(ALVO), f"constantes de pino mudaram: {sorted(pinos)}"
    for nome, gpio in pinos.items():
        assert ligado(f"esp:D{gpio}", ALVO[nome]), f"{nome} (GPIO {gpio}) nao chega em {ALVO[nome]}"
        feitas += 1

    # LCD no I2C padrão do ESP32 (Wire sem pinos no sketch) e no endereço do sketch.
    assert ligado("esp:D21", "lcd:SDA") and ligado("esp:D22", "lcd:SCL"), "LCD fora do I2C padrao"
    endereco = re.search(r"LiquidCrystal_I2C lcd\((0x[0-9A-Fa-f]+), 16, 2\)", sketch).group(1)
    lcd = next(p for p in diagrama["parts"] if p["id"] == "lcd")
    assert lcd["attrs"].get("pins") == "i2c", "LCD sem o modulo I2C"
    assert int(lcd["attrs"].get("i2cAddress", "0x27"), 16) == int(endereco, 16), "endereco do LCD difere do sketch"
    feitas += 3

    for pino in NO_GND:
        assert any(ligado(pino, g) for g in ("esp:GND.1", "esp:GND.2")), f"{pino} sem GND"
        feitas += 1

    # Nenhum nó junta dois pinos do ESP32, salvo os dois GND.
    for no in {frozenset(n) for n in rede.values()}:
        esp = {p for p in no if p.startswith("esp:")} - {"esp:GND.1", "esp:GND.2"}
        assert len(esp) <= 1, f"curto entre {sorted(esp)}"
    feitas += 1

    for cab in re.findall(r"#include [<\"]([^>\"]+)[>\"]", sketch):
        if cab in DO_CORE or cab.startswith("freertos/"):
            continue
        assert BIBLIOTECA.get(cab) in libs, f"{cab} sem linha no libraries.txt"
        feitas += 1
    return feitas


if __name__ == "__main__":
    print(f"Circuito confere: {conferir()} verificacoes OK")
