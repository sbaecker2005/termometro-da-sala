#!/usr/bin/env python3
"""Gera relatorio-gestao-de-riscos.pdf sem pandoc/xelatex: Markdown -> HTML -> Chromium.

Uso: python gerar_pdf_html.py [caminho do chrome]
Reaproveita montar.py (links do repositorio, partes dos prints, larguras das colunas);
onde o montar.py emite LaTeX cru, aqui sai HTML/CSS equivalente.
"""
import html
import os
import re
import subprocess
import sys
import tempfile
from pathlib import Path

import markdown
from pypdf import PdfReader

PDF = Path(__file__).resolve().parent
SEG = PDF.parent
SAIDA = PDF / "relatorio-gestao-de-riscos.pdf"
CANDIDATOS = [
    "C:/Users/samue/AppData/Local/ms-playwright/chromium-1234/chrome-win64/chrome.exe",
    "C:/Program Files/Google/Chrome/Application/chrome.exe",
    "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe",
]
# O Python da Microsoft Store nao enxerga parte de AppData\Local: por isso a lista tem mais de um navegador.
CHROME = (sys.argv[1] if len(sys.argv) > 1 else os.environ.get("CHROME")
          or next((c for c in CANDIDATOS if os.path.exists(c)), None))
if not CHROME:
    sys.exit("Chromium/Chrome/Edge nao encontrado: passe o caminho como argumento ou em CHROME")

# montar.py roda ao ser importado; a saida dele (LaTeX) vai para um arquivo descartavel.
_tmp = Path(tempfile.mkdtemp()) / "corpo.md"
sys.argv = ["montar.py", str(SEG), str(_tmp)]
sys.path.insert(0, str(PDF))
import montar  # noqa: E402

# ---- metadados (capa e cabecalho) ----
meta = (PDF / "metadados.yaml").read_text(encoding="utf-8")


def campo(nome):
    return re.search(rf'^{nome}:\s*"(.*)"\s*$', meta, re.M).group(1)


titulo, subtitulo, data = campo("title"), campo("subtitle"), campo("date")
autores = re.findall(r'^\s+- "(.*)"\s*$', meta.split("author:")[1].split("date:")[0], re.M)
cab_esq = re.search(r"fancyhead\[L\]\{\\footnotesize (.*)\}", meta).group(1)
cab_dir = re.search(r"fancyhead\[R\]\{\\footnotesize (.*)\}", meta).group(1)

# ---- texto: o que montar.py ja prepara, sem o LaTeX ----
corpo = montar.links(montar.corpo)
corpo = corpo.replace("fluxo-dados.pdf){width=100%}", "../fluxo-dados.svg)")
anexo = montar.links(montar.anexo).replace("\\newpage", '<div class="quebra"></div>')
texto = re.sub(r"\)\{width=100%\}", ")", corpo + "\n\n" + anexo)

# larguras por tabela, na ordem em que aparecem (fora de blocos de codigo)
tabelas = []
em_codigo = False
linhas = texto.split("\n")
for i, l in enumerate(linhas):
    if l.lstrip().startswith("```"):
        em_codigo = not em_codigo
    if (not em_codigo and l.startswith("|") and i + 1 < len(linhas)
            and montar.SEPARADOR.match(linhas[i + 1])):
        fim = i
        while fim < len(linhas) and linhas[fim].startswith("|"):
            fim += 1
        n = len(montar.celulas(l))
        pesos = montar.proporcoes(linhas[i:fim], n)
        piso = sum(pesos) * 0.05  # coluna de ID/classe nao pode ficar mais estreita que uma palavra
        tabelas.append((n, [max(x, piso) for x in pesos]))

md = markdown.Markdown(extensions=["tables", "fenced_code", "toc", "attr_list", "md_in_html"])
corpo_html = md.convert(texto)

it = iter(tabelas)
saida, larga = [], False
for p in re.split(r"(<table>|</table>)", corpo_html):
    if p == "<table>":
        n, pesos = next(it)
        larga = n >= montar.LARGAS
        cols = "".join(f'<col style="width:{x / sum(pesos) * 100:.2f}%">' for x in pesos)
        saida.append(('<div class="larga">' if larga else "") + f"<table><colgroup>{cols}</colgroup>")
    elif p == "</table>":
        saida.append("</table>" + ("</div>" if larga else ""))
    else:
        saida.append(p)
corpo_html = "".join(saida)
# titulo logo antes de tabela deitada vai junto para a pagina deitada (senao fica sozinho no pe)
corpo_html = re.sub(r'(<h[2-4][^>]*>.*?</h[2-4]>\s*)<div class="larga">', r'<div class="larga">', corpo_html)
assert next(it, None) is None, "numero de tabelas diverge"

# imagem sozinha no paragrafo vira figura numerada
nfig = 0


def figura(m):
    global nfig
    nfig += 1
    return (f'<figure><img src="{m.group(2)}" alt="{m.group(1)}">'
            f"<figcaption>Figura {nfig}: {m.group(1)}</figcaption></figure>")


corpo_html = re.sub(r'<p><img alt="([^"]*)" src="([^"]*)"\s*/?></p>', figura, corpo_html)

# sumario: so os h2, como no PDF de referencia
h2 = re.findall(r'<h2 id="([^"]+)">(.*?)</h2>', corpo_html)

CSS = """
@page { size: A4; margin: 2.2cm;
  @top-left { content: "ESQ"; font: 9pt 'Times New Roman', serif; border-bottom: .4pt solid #000; padding-bottom: 3pt; }
  @top-right { content: "DIR"; font: 9pt 'Times New Roman', serif; border-bottom: .4pt solid #000; padding-bottom: 3pt; }
  @bottom-center { content: counter(page); font: 9pt 'Times New Roman', serif; } }
@page :first { @top-left { content: none; border: 0 } @top-right { content: none; border: 0 } }
@page land { size: A4 landscape; margin: 2.2cm; }
html { font: 10pt/1.38 'Times New Roman', 'Latin Modern Roman', serif; color: #000; }
body { margin: 0; hyphens: auto; }
a { color: #0b2a6f; text-decoration: none; overflow-wrap: anywhere; }
h2 { font-size: 14pt; margin: 1.6em 0 .5em; break-after: avoid; }
h3 { font-size: 11.5pt; margin: 1.3em 0 .4em; break-after: avoid; }
h4 { font-size: 10pt; margin: 1.1em 0 .3em; break-after: avoid; }
p { margin: .6em 0; orphans: 3; widows: 3; }
li { margin: .2em 0; }
code { font: 8pt 'DejaVu Sans Mono', Consolas, monospace; overflow-wrap: anywhere; }
pre { font: 7.2pt/1.3 'DejaVu Sans Mono', Consolas, monospace; background: #f6f6f6; padding: 6pt;
  white-space: pre-wrap; overflow-wrap: anywhere; word-break: break-all; margin: .8em 0; }
pre code { font: inherit; }
table { width: 100%; border-collapse: collapse; table-layout: fixed; font-size: 8pt; line-height: 1.25;
  border-top: 1pt solid #000; border-bottom: 1pt solid #000; margin: .9em 0; }
thead tr { border-bottom: .5pt solid #000; }
th, td { padding: 2.5pt 4pt 2.5pt 0; vertical-align: top; overflow-wrap: anywhere; hyphens: auto; text-align: left; }
th { font-weight: bold; }
td code, th code { font-size: 6.8pt; word-break: break-all; }
tr { break-inside: avoid; }
thead { display: table-header-group; }
.larga { page: land; }
.larga table { font-size: 7.5pt; }
figure { margin: 1em 0; text-align: center; break-inside: avoid; }
figure img { width: auto; max-width: 100%; max-height: 22.5cm; }
figcaption { font-size: 9pt; margin-top: 4pt; }
.quebra { break-after: page; }
.capa { text-align: center; padding-top: 2.4cm; }
.capa h1 { font-size: 17pt; font-weight: normal; margin: 0 0 .5em; }
.capa .sub { font-size: 10.5pt; margin-bottom: 2em; }
.capa .eq { font-size: 10.5pt; margin-bottom: 2em; line-height: 1.6; }
.capa .eq .eq-nome { margin-bottom: .4em; }
.capa .data { font-size: 10pt; }
.sumario { break-before: page; break-after: page; }
.sumario h2 { margin-top: 0; font-size: 16pt; }
.sumario a { color: #000; display: flex; gap: 6pt; margin: .35em 0; }
.sumario .pt { flex: 1; border-bottom: 1pt dotted #444; transform: translateY(-3pt); }
""".replace("ESQ", cab_esq).replace("DIR", cab_dir)


def montar_html(paginas):
    linhas_toc = "".join(
        f'<a href="#{i}"><span>{t}</span><span class="pt"></span><span>{paginas.get(i, "00")}</span></a>'
        for i, t in h2)
    nomes = autores[1:]
    # equipe numa linha e um integrante por linha, centralizado (em colunas a linha passava da margem)
    eq = "".join(f"<div>{html.escape(n)}</div>" for n in nomes)
    capa = (f'<div class="capa"><h1>{html.escape(titulo)}</h1><div class="sub">{html.escape(subtitulo)}</div>'
            f'<div class="eq"><div class="eq-nome">{html.escape(autores[0])}</div>{eq}</div>'
            f'<div class="data">{html.escape(data)}</div></div>')
    return (f'<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><title>{html.escape(titulo)}</title>'
            f"<style>{CSS}</style></head><body>{capa}"
            f'<div class="sumario"><h2>Sumário</h2>{linhas_toc}</div>{corpo_html}</body></html>')


def renderizar(paginas):
    arq = PDF / "_relatorio.html"
    arq.write_text(montar_html(paginas), encoding="utf-8")
    try:
        subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                        f"--print-to-pdf={SAIDA}", arq.as_uri()], check=True, timeout=180,
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    finally:
        arq.unlink(missing_ok=True)


def achar_paginas():
    """O Chromium nao preenche numero de pagina no sumario: acha o titulo no PDF e renderiza de novo."""
    textos = [re.sub(r"\s+", " ", p.extract_text() or "") for p in PdfReader(str(SAIDA)).pages]
    res = {}
    for i, t in h2:
        alvo = re.sub(r"\s+", " ", html.unescape(re.sub("<[^>]+>", "", t))).strip()
        res[i] = next((n for n, tx in enumerate(textos[2:], start=3) if alvo in tx), "?")
    return res


renderizar({})
renderizar(achar_paginas())
print(f"{SAIDA}: {len(PdfReader(str(SAIDA)).pages)} paginas, {len(tabelas)} tabelas, {nfig} figuras")
