#!/usr/bin/env python3
"""Corta os prints altos em partes que cabem numa pagina A4, sempre numa linha sem texto.

Uso: python fatiar_prints.py ../evidencias/prints prints
Gera prints/E06-1.png, prints/E06-2.png ... (prints que ja cabem saem com uma parte so).
Precisa do Pillow.
"""
import sys
from pathlib import Path

from PIL import Image

ORIGEM = Path(sys.argv[1])
DESTINO = Path(sys.argv[2])
MAXIMO = 1750   # 1400 px de largura na largura util do A4 deixam ~1750 px de altura por pagina


def linha_vazia(img: Image.Image, y: int) -> bool:
    """Linha de uma cor so (fundo do terminal): cortar ali nao divide texto."""
    faixa = img.crop((0, y, img.width, y + 1))
    return all(lo == hi for lo, hi in faixa.getextrema())


def cortes(img: Image.Image) -> list[int]:
    pontos = [0]
    while img.height - pontos[-1] > MAXIMO:
        inicio = pontos[-1]
        resto = img.height - inicio
        # Partes de altura parecida: evita sobrar uma tira de poucos pixels no fim.
        alvo = inicio + -(-resto // -(-resto // MAXIMO))
        teto = min(alvo + 150, inicio + MAXIMO)
        y = next((y for y in range(teto, (inicio + alvo) // 2, -1) if linha_vazia(img, y)), teto)
        pontos.append(y)
    pontos.append(img.height)
    return pontos


def main() -> None:
    DESTINO.mkdir(parents=True, exist_ok=True)
    for arquivo in sorted(ORIGEM.glob("E*.png")):
        if arquivo.stem == "E01":
            continue  # listagem de codigo com 12808 px; no PDF entram os trechos de codigo do anexo
        img = Image.open(arquivo).convert("RGB")
        pontos = cortes(img)
        for i, (a, b) in enumerate(zip(pontos, pontos[1:]), start=1):
            parte = img.crop((0, a, img.width, b))
            assert parte.height <= MAXIMO, (arquivo.name, i, parte.height)
            parte.save(DESTINO / f"{arquivo.stem}-{i}.png", optimize=True)
        print(f"{arquivo.name}: {img.height} px em {len(pontos) - 1} parte(s)")


if __name__ == "__main__":
    main()
