"""Extrai a estação A701 (São Paulo, Mirante de Santana) dos zips anuais do INMET.

Uso: python extrair_a701.py <pasta com 2022.zip ... 2025.zip>
Fonte: https://portal.inmet.gov.br/dadoshistoricos (dados horários, hora em UTC).
Gera dados/inmet_a701.csv com data_hora_utc, temperatura_c e umidade_pct.
"""
import csv
import io
import sys
import zipfile
from pathlib import Path

COL_TEMP = "TEMPERATURA DO AR - BULBO SECO, HORARIA (°C)"
COL_UMID = "UMIDADE RELATIVA DO AR, HORARIA (%)"


def numero(texto):
    texto = (texto or "").strip().replace(",", ".")
    if not texto or texto == "-9999":
        return None
    return float(texto)


def linhas_do_zip(caminho):
    with zipfile.ZipFile(caminho) as z:
        nomes = [n for n in z.namelist() if "_A701_" in n.upper() and n.upper().endswith(".CSV")]
        if len(nomes) != 1:
            raise SystemExit(f"{caminho}: esperava 1 arquivo da A701, achei {nomes}")
        texto = z.read(nomes[0]).decode("latin-1")
    linhas = texto.splitlines()
    leitor = csv.DictReader(io.StringIO("\n".join(linhas[8:])), delimiter=";")  # 8 linhas de cabeçalho da estação
    for r in leitor:
        data = (r.get("Data") or r.get("DATA (YYYY-MM-DD)") or "").replace("/", "-")
        hora = (r.get("Hora UTC") or r.get("HORA (UTC)") or "").replace(" UTC", "").replace(":", "")[:4]
        t, u = numero(r.get(COL_TEMP)), numero(r.get(COL_UMID))
        if data and hora and t is not None and u is not None:
            yield f"{data}T{hora[:2]}:{hora[2:]}:00Z", t, u


def main(pasta):
    saida = Path(__file__).with_name("dados") / "inmet_a701.csv"
    saida.parent.mkdir(exist_ok=True)
    total = 0
    with saida.open("w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["data_hora_utc", "temperatura_c", "umidade_pct"])
        for z in sorted(Path(pasta).glob("20*.zip")):
            n = 0
            for linha in linhas_do_zip(z):
                w.writerow(linha)
                n += 1
            print(f"{z.name}: {n} horas válidas")
            total += n
    print(f"total {total} -> {saida}")


if __name__ == "__main__":
    main(sys.argv[1])
