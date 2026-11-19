"""Gera arquitetura-togaf.pdf a partir de DOCUMENTO.md (python 3.11: markdown, pypdf; Chromium sem tela).
Uso: python gerar_pdf_html.py [--chrome CAMINHO]. Roda em duas passagens para preencher as paginas do sumario."""
import argparse, html, re, subprocess, sys, tempfile
from pathlib import Path
import markdown
from pypdf import PdfReader

AQUI = Path(__file__).resolve().parent
# O Python da Microsoft Store nao enxerga AppData\Local\ms-playwright; por isso o Chrome instalado vem primeiro.
CHROME = next((c for c in ["C:/Program Files/Google/Chrome/Application/chrome.exe",
    "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe",
    "C:/Users/samue/AppData/Local/ms-playwright/chromium-1234/chrome-win64/chrome.exe"] if Path(c).exists()), "chrome")
SAIDA = AQUI / "arquitetura-togaf.pdf"
EQUIPE = [("Guilherme Caseiro", "559765"), ("Samuel Baecker", "559269"), ("Lucas Villar", "560005")]

CSS = """
@page { size: A4; margin: 22mm 18mm 20mm 18mm;
  @top-left { content: "Termômetro da Sala · Arquitetura TOGAF"; font: 8pt Georgia, serif; color: #555; }
  @top-right { content: "Global Solution 2026.2 · FIAP"; font: 8pt Georgia, serif; color: #555; }
  @bottom-center { content: counter(page); font: 9pt Georgia, serif; color: #333; } }
@page capa { margin: 0; @top-left{content:none} @top-right{content:none} @bottom-center{content:none} }
@page larga { size: A4 landscape; margin: 20mm 14mm 16mm 14mm; }
body { font: 10pt/1.45 Georgia, "Times New Roman", serif; color: #1a1a1a; }
h1 { font-size: 18pt; margin: 0 0 .4em; } h2 { font-size: 14pt; margin: 1.4em 0 .4em; border-bottom: 1px solid #999; padding-bottom: 2px; break-after: avoid; }
h3 { font-size: 11.5pt; margin: 1.1em 0 .3em; break-after: avoid; }
p { margin: .4em 0; text-align: justify; }
table { border-collapse: collapse; width: 100%; margin: .6em 0; font: 8.3pt/1.3 Arial, sans-serif; }
th, td { border: .6pt solid #888; padding: 2.5pt 4pt; vertical-align: top; overflow-wrap: anywhere; }
td:first-child, th:first-child { min-width: 7mm; overflow-wrap: normal; }
th { background: #e8e8e8; text-align: left; } tr { break-inside: avoid; }
code { font: 8.3pt Consolas, monospace; overflow-wrap: anywhere; }
pre { font: 8pt Consolas, monospace; background: #f4f4f4; padding: 5pt; white-space: pre-wrap; }
a { color: #1a1a1a; text-decoration: none; overflow-wrap: anywhere; }
blockquote { margin: .5em 1em; color: #333; border-left: 2pt solid #999; padding-left: 8pt; }
.capa { page: capa; height: 250mm; padding: 70mm 25mm 0; box-sizing: border-box; break-after: page; text-align: left; }
.capa .k { font: 11pt Arial, sans-serif; letter-spacing: .12em; text-transform: uppercase; color: #555; }
.capa h1 { font-size: 30pt; margin: 14mm 0 4mm; line-height: 1.15; border-left: 4pt solid #1a1a1a; padding-left: 8mm; }
.capa .s { font-size: 14pt; color: #333; margin-left: 12mm; }
.capa .d { margin: 28mm 0 0 0; font-size: 11.5pt; } .capa table { font: 11pt Georgia, serif; width: auto; margin-top: 4mm; }
.capa td, .capa th { border: none; border-bottom: .5pt solid #bbb; padding: 3pt 14pt 3pt 0; background: none; }
.sumario { break-after: page; } .sumario h2 { border: none; } .sumario li { list-style: none; margin: 3pt 0; display: flex; }
.sumario ul { padding: 0; } .sumario .t { flex: 1; border-bottom: 1px dotted #888; } .sumario .n { padding-left: 6pt; } .sumario li.s3 { margin-left: 14pt; font-size: 9.5pt; }
.fig { page: larga; break-before: page; break-after: page; text-align: center; }
.fig img { max-width: 100%; max-height: 150mm; object-fit: contain; } .fig p { text-align: center; font-size: 9pt; color: #444; }
"""

def slug(t): return re.sub(r"[^a-z0-9]+", "-", t.lower()).strip("-")

def montar(paginas):
    md = (AQUI / "DOCUMENTO.md").read_text(encoding="utf-8")
    md = md.split("\n", 1)[1]  # titulo vai na capa
    corpo = markdown.markdown(md, extensions=["tables", "fenced_code", "sane_lists"])
    # imagens em pagina paisagem propria, com legenda
    corpo = re.sub(r'<p><img alt="([^"]*)" src="([^"]+)" ?/?></p>',
        lambda m: f'<div class="fig"><img src="{m[2]}"><p>{m[1]}</p></div>', corpo)
    titulos = []
    def marca(m):
        i = len(titulos); titulos.append((m[1], html.unescape(re.sub("<[^>]+>", "", m[2]))))
        return f'<h{m[1]} id="t{i}">{m[2]}</h{m[1]}>'
    corpo = re.sub(r"<h([23])>(.*?)</h\1>", marca, corpo)
    corpo = re.sub(r"<h([23]) id", lambda m: m[0], corpo)
    li = "".join(f'<li class="s{n}"><span class="t">{html.escape(t)}</span><span class="n">{paginas.get(i, "")}</span></li>'
                 for i, (n, t) in enumerate(titulos) if n == "2" or t.startswith(("4.", "7.")))
    eq = "".join(f"<tr><td>{n}</td><td>RM {rm}</td></tr>" for n, rm in EQUIPE)
    capa = f"""<div class="capa"><div class="k">Global Solution 2026.2 · FIAP</div>
<h1>Termômetro da Sala</h1><div class="s">Documento descritivo da arquitetura (TOGAF)</div>
<div class="d"><b>Disciplina:</b> Testing, Compliance &amp; Quality Assurance, Arquitetura de Soluções baseada no TOGAF<br>
<b>Projeto:</b> CitizenTech Termômetro da Sala<br><br><b>Equipe</b><table>{eq}</table></div></div>"""
    sumario = f'<div class="sumario"><h2>Sumário</h2><ul>{li}</ul></div>'
    return f'<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><style>{CSS}</style></head><body>{capa}{sumario}{corpo}</body></html>', titulos

def pdf(htm, destino, chrome):
    with tempfile.NamedTemporaryFile("w", suffix=".html", dir=AQUI, delete=False, encoding="utf-8") as f:
        f.write(htm); p = Path(f.name)
    try:
        subprocess.run([chrome, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                        f"--print-to-pdf={destino}", p.as_uri()], check=True, capture_output=True, timeout=180)
    finally: p.unlink()

def paginas_dos_titulos(titulos):
    norm = lambda s: re.sub(r"\s+", " ", s).strip()
    r = PdfReader(SAIDA); txt = [norm(p.extract_text() or "") for p in r.pages]
    res = {}
    for i, (_, t) in enumerate(titulos):
        for n in range(2, len(txt)):  # pula capa e sumario
            if norm(t) in txt[n]: res[i] = n + 1; break
    return res

if __name__ == "__main__":
    a = argparse.ArgumentParser(); a.add_argument("--chrome", default=CHROME); a = a.parse_args()
    htm, tit = montar({}); pdf(htm, SAIDA, a.chrome)
    htm, tit = montar(paginas_dos_titulos(tit)); pdf(htm, SAIDA, a.chrome)
    print(SAIDA, len(PdfReader(SAIDA).pages), "paginas")
