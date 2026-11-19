#!/usr/bin/env python3
"""Monta o PDF unico de Cybersecurity: relatorio + anexo."""
import re
import sys
from pathlib import Path

SEG = Path(sys.argv[1])          # pasta seguranca/ do repositorio
SAIDA = Path(sys.argv[2])        # arquivo .md de saida
REPO = "https://github.com/sbaecker2005/termometro-da-sala"
BLOB = f"{REPO}/blob/main"
TREE = f"{REPO}/tree/main"

relatorio = (SEG / "RELATORIO-RISCOS.md").read_text(encoding="utf-8")
anexo = (SEG / "ANEXO-EVIDENCIAS.md").read_text(encoding="utf-8")


def links(texto: str) -> str:
    """Troca caminhos relativos por URLs do repositorio, para o PDF ter link util."""
    def troca(m):
        rotulo, alvo = m.group(1), m.group(2)
        if alvo.startswith(("http://", "https://", "#")):
            return m.group(0)
        if alvo.startswith("../"):
            destino = alvo[3:]
        else:
            destino = f"seguranca/{alvo}"
        base = TREE if destino.endswith("/") else BLOB
        return f"[{rotulo}]({base}/{destino.rstrip('/')})"
    # (?<!!) preserva as imagens, que apontam para arquivo local na hora de compor o PDF.
    return re.sub(r"(?<!!)\[([^\]]+)\]\(([^)]+)\)", troca, texto)


# Relatorio: o titulo e a identificacao vao para a capa; o corpo comeca no resumo.
corpo = relatorio.split("## Resumo executivo", 1)[1]
corpo = "## Resumo executivo" + corpo
corpo = corpo.replace(
    "![Fluxo de dados e fronteiras de confiança](fluxo-dados.svg)",
    "![Fluxo de dados e fronteiras de confiança](fluxo-dados.pdf){width=100%}",
)

PDF = Path(__file__).resolve().parent
PRINT = re.compile(r"^!\[([^\]]+)\]\(evidencias/prints/(E\d+)\.png\)[ \t]*$", re.M)


def prints(texto: str) -> str:
    """Cada print do anexo vira suas partes de pagina (fatiar_prints.py); sem as partes, o print inteiro."""
    def troca(m):
        legenda, nome = m.group(1), m.group(2)
        partes = sorted(PDF.glob(f"prints/{nome}-*.png"), key=lambda p: int(p.stem.split("-")[1]))
        if not partes:
            return f"![{legenda}](../evidencias/prints/{nome}.png){{width=100%}}"
        n = len(partes)
        return "\n\n".join(
            f"![{legenda}{f' (parte {i} de {n})' if n > 1 else ''}](prints/{p.name}){{width=100%}}"
            for i, p in enumerate(partes, start=1))
    texto = PRINT.sub(troca, texto)
    # Prints de outras pastas (../infra/...) sao relativos a seguranca/; o PDF e composto em seguranca/pdf/.
    return re.sub(r"^(!\[[^\]]+\]\()\.\./([^)]+\.png)\)[ \t]*$", r"\1../../\2){width=100%}", texto, flags=re.M)


# Anexo: vira uma secao do mesmo documento, um nivel abaixo do que estava.
anexo = prints(anexo)
anexo = anexo.replace("# Termômetro da Sala — Anexo de evidências de Cybersecurity", "", 1)
anexo = re.sub(r"^(##+) ", r"#\1 ", anexo, flags=re.M)
anexo = "\\newpage\n\n## Anexo de evidências\n" + anexo

SEPARADOR = re.compile(r"^\s*\|[\s:|-]+\|\s*$")
LARGAS = 6  # a partir de 6 colunas a tabela vai deitada na pagina


def celulas(linha: str) -> list[str]:
    return [c.strip() for c in linha.strip().strip("|").split("|")]


def proporcoes(bloco: list[str], n: int) -> list[int]:
    """Larguras relativas pelo conteudo: sem isso o pandoc da a mesma fracao a toda coluna
    e as de texto longo saem com uma palavra por linha."""
    larguras = [6] * n
    for linha in bloco:
        if SEPARADOR.match(linha):
            continue
        for i, celula in enumerate(celulas(linha)[:n]):
            larguras[i] = max(larguras[i], min(len(celula), 90))
    total = sum(larguras)
    return [max(3, round(l / total * 160)) for l in larguras]


def formatar_tabelas(texto: str) -> str:
    linhas = texto.split("\n")
    saida: list[str] = []
    i = 0
    while i < len(linhas):
        # Uma tabela comeca numa linha de cabecalho seguida da linha separadora.
        if (linhas[i].startswith("|") and i + 1 < len(linhas)
                and SEPARADOR.match(linhas[i + 1])):
            fim = i
            while fim < len(linhas) and linhas[fim].startswith("|"):
                fim += 1
            bloco = linhas[i:fim]
            n = len(celulas(bloco[0]))
            alinhamentos = celulas(bloco[1])
            pesos = proporcoes(bloco, n)
            novo = []
            for j, peso in enumerate(pesos):
                a = alinhamentos[j] if j < len(alinhamentos) else "---"
                esquerda = ":" if a.startswith(":") else ""
                direita = ":" if a.endswith(":") else ""
                novo.append(esquerda + "-" * peso + direita)
            bloco[1] = "|" + "|".join(novo) + "|"
            if n >= LARGAS:
                bloco = ["\\blandscape", ""] + bloco + ["", "\\elandscape"]
            saida.extend(bloco)
            i = fim
        else:
            saida.append(linhas[i])
            i += 1
    return "\n".join(saida)


saida = "\\newpage\n\n" + formatar_tabelas(links(corpo) + "\n\n" + links(anexo))
SAIDA.write_text(saida, encoding="utf-8")
imagens = len(re.findall(r"^!\[", saida, flags=re.M))
print(f"{SAIDA}: {len(saida.splitlines())} linhas, "
      f"{saida.count('blandscape')} tabelas deitadas, {imagens} imagens")
