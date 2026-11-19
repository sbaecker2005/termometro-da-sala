"""Extrai a estação A701 (São Paulo, Mirante de Santana) dos arquivos do INMET.

Uso: python extrair_a701.py <pasta com 2022.zip ... 2025.zip>
     python extrair_a701.py <A701_2026.csv> dados/inmet_a701_2026.csv
Fonte: https://portal.inmet.gov.br/dadoshistoricos (dados horários, hora em UTC).
Gera dados/inmet_a701.csv (ou a saída indicada) com data_hora_utc, temperatura_c e umidade_pct.
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


def linhas_do_texto(texto):
    linhas = texto.splitlines()
    leitor = csv.DictReader(io.StringIO("\n".join(linhas[8:])), delimiter=";")  # 8 linhas de cabeçalho da estação
    for r in leitor:
        data = (r.get("Data") or r.get("DATA (YYYY-MM-DD)") or "").replace("/", "-")
        hora = (r.get("Hora UTC") or r.get("HORA (UTC)") or "").replace(" UTC", "").replace(":", "")[:4]
        t, u = numero(r.get(COL_TEMP)), numero(r.get(COL_UMID))
        if data and hora and t is not None and u is not None:
            yield f"{data}T{hora[:2]}:{hora[2:]}:00Z", t, u


def linhas_do_zip(caminho):
    with zipfile.ZipFile(caminho) as z:
        nomes = [n for n in z.namelist() if "_A701_" in n.upper() and n.upper().endswith(".CSV")]
        if len(nomes) != 1:
            raise SystemExit(f"{caminho}: esperava 1 arquivo da A701, achei {nomes}")
        texto = z.read(nomes[0]).decode("latin-1")
    yield from linhas_do_texto(texto)


def linhas_do_csv(caminho):
    texto = Path(caminho).read_bytes().decode("latin-1")
    if "A701" not in "\n".join(texto.splitlines()[:8]):
        raise SystemExit(f"{caminho}: o cabeçalho não é da estação A701")
    yield from linhas_do_texto(texto)


def main(origem, saida=None):
    origem = Path(origem)
    saida = Path(saida) if saida else Path(__file__).with_name("dados") / "inmet_a701.csv"
    fontes = [(origem, linhas_do_csv)] if origem.is_file() else [(z, linhas_do_zip) for z in sorted(origem.glob("20*.zip"))]
    saida.parent.mkdir(exist_ok=True)
    total = 0
    with saida.open("w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["data_hora_utc", "temperatura_c", "umidade_pct"])
        for caminho, ler in fontes:
            n = 0
            for linha in ler(caminho):
                w.writerow(linha)
                n += 1
            print(f"{caminho.name}: {n} horas válidas")
            total += n
    print(f"total {total} -> {saida}")


if __name__ == "__main__":
    main(*sys.argv[1:3])
