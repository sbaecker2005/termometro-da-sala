"""Gera as duas cópias de revisão a partir dos Markdown e diagramas do kit.

Usa Python/ReportLab e Node/marked/sharp do runtime indicado, sem downloads.
Exemplo: python ferramentas/gerar-pdfs-revisao.py --runtime <dependencies>
"""
from __future__ import annotations

import argparse
import html
import json
import re
import subprocess
from pathlib import Path

from PIL import Image as PillowImage
from reportlab.lib import colors
from reportlab.lib.pagesizes import A3, A4, landscape
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate, Frame, Image, LongTable, NextPageTemplate,
    PageBreak, PageTemplate, Paragraph, Spacer, TableStyle,
)

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'output' / 'pdf'
TMP = ROOT / 'tmp' / 'pdfs'
INK = colors.HexColor('#172B40')
BLUE = colors.HexColor('#155E80')
GREY = colors.HexColor('#526474')
BODY_SIZE = landscape(A4)
FIGURE_SIZE = landscape(A3)
MARGIN = 38
WIDTH = BODY_SIZE[0] - MARGIN * 2


def normalize(text):
    return str(text).translate(str.maketrans({c: '-' for c in '\u2010\u2011\u2012\u2013\u2014\u2212'}))


def escape(text):
    return html.escape(normalize(text), quote=False)


def setup_fonts():
    fonts = Path('C:/Windows/Fonts')
    for name, filename in [('Arial', 'arial.ttf'), ('Arial-Bold', 'arialbd.ttf'),
                           ('Arial-Italic', 'ariali.ttf'), ('Arial-BoldItalic', 'arialbi.ttf'),
                           ('Consolas', 'consola.ttf')]:
        pdfmetrics.registerFont(TTFont(name, str(fonts / filename)))
    pdfmetrics.registerFontFamily('Arial', normal='Arial', bold='Arial-Bold',
                                italic='Arial-Italic', boldItalic='Arial-BoldItalic')


def styles():
    common = dict(fontName='Arial', textColor=INK, leading=13, fontSize=10,
                  spaceAfter=7, splitLongWords=True, allowWidows=0, allowOrphans=0)
    result = {'body': ParagraphStyle('body', **common)}
    result['cell'] = ParagraphStyle('cell', **(common | dict(fontSize=9, leading=11.7, spaceAfter=0)))
    result['headcell'] = ParagraphStyle('headcell', parent=result['cell'], fontName='Arial-Bold', textColor=colors.white)
    result['caption'] = ParagraphStyle('caption', parent=result['body'], fontSize=10, leading=13, textColor=GREY)
    result['bullet'] = ParagraphStyle('bullet', parent=result['body'], leftIndent=14, firstLineIndent=-11)
    for level, size in [(1, 21), (2, 15), (3, 12)]:
        result[f'h{level}'] = ParagraphStyle(f'h{level}', parent=result['body'], fontName='Arial-Bold',
                                            fontSize=size, leading=size * 1.25, textColor=BLUE,
                                            spaceBefore=12, spaceAfter=9, keepWithNext=True)
    result['cover'] = ParagraphStyle('cover', parent=result['h1'], fontSize=31, leading=37, spaceAfter=20)
    result['kicker'] = ParagraphStyle('kicker', parent=result['body'], fontSize=12, fontName='Arial-Bold', textColor=BLUE)
    return result


def markdown_tokens(runtime, source):
    module = (runtime / 'node/node_modules/marked/lib/marked.esm.js').as_uri()
    code = f"""import {{marked}} from {json.dumps(module)};
import fs from 'node:fs';
process.stdout.write(JSON.stringify(marked.lexer(fs.readFileSync(process.argv[1], 'utf8'))));"""
    result = subprocess.run([str(runtime / 'node/bin/node.exe'), '--input-type=module', '-e', code, str(source)],
                            check=True, capture_output=True, encoding='utf-8')
    return json.loads(result.stdout)


def inlines(tokens):
    result = []
    for token in tokens:
        kind = token['type']
        children = token.get('tokens')
        text = inlines(children) if children else escape(token.get('text', token.get('raw', '')))
        if kind == 'strong':
            text = f'<b>{text}</b>'
        elif kind == 'em':
            text = f'<i>{text}</i>'
        elif kind == 'codespan':
            text = f'<font name="Consolas" size="8.4">{text}</font>'
        elif kind == 'link':
            href = token['href']
            if href.startswith(('https://', 'http://')):
                text = f'<a href="{html.escape(href, quote=True)}" color="#155E80">{text}</a>'
            elif text != escape(href):
                text += f' <font color="#526474">({escape(href)})</font>'
        elif kind == 'br':
            text = '<br/>'
        elif kind == 'html':
            raw = token.get('raw', '')
            text = '<br/>' if re.fullmatch(r'<br\s*/?>', raw, flags=re.I) else escape(raw)
        elif kind == 'image':
            raise ValueError('Imagem deve estar em parágrafo próprio')
        elif kind not in ('text', 'escape', 'del'):
            raise ValueError(f'Token inline não tratado: {kind}')
        result.append(text)
    return ''.join(result)


def table_widths(header):
    names = [cell['text'] for cell in header]
    n = len(names)
    if n == 8 and names[0] == 'Risco':
        weights = [1.05, .5, 1.9, 1.65, 1.65, .6, .65, .7]
    elif n == 8:
        weights = [.5, .6, .6, .95, 3.0, .6, .6, 1.1]
    elif names[:2] == ['ID', 'Ativo e conteúdo']:
        weights = [.45, 2.2, .5, .5, .5, 2.2, 1.8]
    elif names[:2] == ['ID', 'Agente']:
        weights = [.4, 1.45, 1.4, 1.9, .85, 1.8, .8]
    elif names[:2] == ['ID', 'Controle e critério de aceitação']:
        weights = [.35, 2.55, 4.85, 1.0]
    elif names[:2] == ['ID', 'Controle e risco']:
        weights = ([.4, 1.25, 1.2, 2.0, 3.2, 1.3] if n == 6 else [.4, 1.2, 2.3, 3.4, 1.3])
    elif names[:2] == ['ID', 'Tipo']:
        weights = [.35, .45, 2.0, 2.1, 1.0, 2.5]
    elif names[:2] == ['Passo', 'Ação do atacante']:
        weights = [.55, 1.9, 3.4, 1.1, 1.8]
    elif names[:2] == ['Nº', 'Fonte']:
        weights = [.35, 2.6, 3.2, 2.1]
    elif n == 2:
        weights = [1.4, 4.6]
    elif n == 3:
        weights = [1.7, 3.0, 2.5]
    elif n == 4:
        weights = [1.4, 2.5, 2.7, 2.7]
    elif n == 5:
        weights = [1.1, 2.1, 1.4, 1.6, 2.4]
    elif n == 6:
        weights = [.5, 1.7, 2.5, 1.4, 2.0, 1.2]
    else:
        weights = [1] * n
    return [WIDTH * weight / sum(weights) for weight in weights]


def diagram(runtime, source, token, st):
    path = (source.parent / token['href']).resolve()
    if path.suffix.lower() == '.svg':
        converted = TMP / f'{path.stem}.png'
        sharp = str(runtime / 'node/node_modules/sharp')
        code = f"require({json.dumps(sharp)})(process.argv[1], {{density: 180}}).png().toFile(process.argv[2]);"
        subprocess.run([str(runtime / 'node/bin/node.exe'), '-e', code, str(path), str(converted)], check=True)
        path = converted
    with PillowImage.open(path) as img:
        w, h = img.size
    max_w, max_h = FIGURE_SIZE[0] - MARGIN * 2, FIGURE_SIZE[1] - MARGIN * 2 - 140
    scale = min(max_w / w, max_h / h)
    figure = Image(str(path), width=w * scale, height=h * scale, kind='direct')
    figure.hAlign = 'CENTER'
    return [NextPageTemplate('figure'), PageBreak(),
            Paragraph(escape(token.get('text', path.stem)), st['h2']), figure,
            Spacer(1, 9), Paragraph(f'Fonte: {escape(source.parent.name + "/" + token["href"])}. Imagem original do projeto.', st['caption']),
            NextPageTemplate('body'), PageBreak()]


def blocks(tokens, source, runtime, st):
    story = []
    for token in tokens:
        kind = token['type']
        if kind in ('space', 'def'):
            continue
        if kind == 'heading':
            level = min(token['depth'], 3)
            story.append(Paragraph(inlines(token['tokens']), st[f'h{level}']))
        elif kind in ('paragraph', 'text'):
            children = token.get('tokens', [])
            if len(children) == 1 and children[0]['type'] == 'image':
                # Keep the preceding section heading with its diagram on the same page.
                heading = story.pop() if story and isinstance(story[-1], Paragraph) and story[-1].style.name.startswith('h') else None
                content = diagram(runtime, source, children[0], st)
                if heading:
                    content.insert(2, heading)
                story.extend(content)
            else:
                paragraph_style = st['h3'] if len(children) == 1 and children[0]['type'] == 'strong' else st['body']
                story.append(Paragraph(inlines(children) if children else escape(token['text']), paragraph_style))
        elif kind == 'table':
            rows = [[Paragraph(inlines(c['tokens']), st['headcell']) for c in token['header']]]
            rows.extend([[Paragraph(inlines(c['tokens']), st['cell']) for c in row] for row in token['rows']])
            table = LongTable(rows, colWidths=table_widths(token['header']), repeatRows=1,
                              hAlign='LEFT', splitByRow=1, splitInRow=0, spaceAfter=11)
            table.setStyle(TableStyle([
                ('BACKGROUND', (0, 0), (-1, 0), BLUE), ('VALIGN', (0, 0), (-1, -1), 'TOP'),
                ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.HexColor('#F0F5F8'), colors.white]),
                ('LINEBELOW', (0, 0), (-1, 0), .6, BLUE),
                ('LINEBELOW', (0, 1), (-1, -1), .3, colors.HexColor('#D3DFE7')),
                ('LEFTPADDING', (0, 0), (-1, -1), 7), ('RIGHTPADDING', (0, 0), (-1, -1), 7),
                ('TOPPADDING', (0, 0), (-1, -1), 7), ('BOTTOMPADDING', (0, 0), (-1, -1), 7),
            ]))
            story.append(table)
        elif kind == 'list':
            for i, item in enumerate(token['items']):
                prefix = str(token.get('start', 1) + i) + '.' if token['ordered'] else '-'
                nested = blocks(item['tokens'], source, runtime, st)
                if nested and isinstance(nested[0], Paragraph):
                    nested[0] = Paragraph(escape(prefix) + ' ' + nested[0].text, st['bullet'])
                story.extend(nested)
        elif kind == 'blockquote':
            story.extend(blocks(token['tokens'], source, runtime, st))
        elif kind == 'hr':
            story.append(Spacer(1, 12))
        elif kind == 'code':
            story.append(Paragraph('<font name="Consolas" size="8.4">' + escape(token['text']).replace('\n', '<br/>') + '</font>', st['body']))
        else:
            raise ValueError(f'Bloco não tratado: {kind}')
    return story


def decorate(canvas, doc, label):
    w, h = canvas._pagesize
    canvas.saveState()
    canvas.setFont('Arial', 8)
    canvas.setFillColor(GREY)
    canvas.drawString(MARGIN, h - 21, 'TERMÔMETRO DA SALA  /  ' + label)
    canvas.drawRightString(w - MARGIN, h - 21, 'REVISÃO DO GRUPO - 17/11/2026')
    canvas.setStrokeColor(colors.HexColor('#D3DFE7'))
    canvas.line(MARGIN, 28, w - MARGIN, 28)
    canvas.drawString(MARGIN, 16, 'Versão de revisão. Evidências do laboratório Windows Server/VM pendentes.')
    canvas.drawRightString(w - MARGIN, 16, str(doc.page))
    canvas.restoreState()


def build(runtime, title, label, filename, sources, st):
    doc = BaseDocTemplate(str(OUT / filename), pagesize=BODY_SIZE, leftMargin=MARGIN,
                         rightMargin=MARGIN, topMargin=MARGIN, bottomMargin=MARGIN,
                         title=title + ' - Revisão do grupo 17/11/2026', author='Equipe Termômetro da Sala')
    templates = []
    for name, size in [('body', BODY_SIZE), ('figure', FIGURE_SIZE)]:
        frame = Frame(MARGIN, MARGIN, size[0] - MARGIN * 2, size[1] - MARGIN * 2,
                      leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0, id=name)
        templates.append(PageTemplate(id=name, frames=[frame], pagesize=size,
                                      onPage=lambda canvas, d: decorate(canvas, d, label)))
    doc.addPageTemplates(templates)
    story = [Spacer(1, 38), Paragraph('GLOBAL SOLUTION 2026.2  /  CITIZENTECH', st['kicker']),
             Spacer(1, 18), Paragraph('Termômetro da Sala', st['cover']),
             Paragraph(escape(title), st['h1']), Spacer(1, 15),
             Paragraph('Revisão do grupo - 17/11/2026', st['h2']),
             Paragraph('Cópia para revisão, não final. Preserva o conteúdo, as referências e as datas históricas dos documentos do projeto. A data desta capa identifica a preparação do PDF.', st['body']),
             Paragraph('Em 17/11/2026: API 100 testes aprovados; desktop 19 testes e 6 autotestes; Mobile 23 testes e TypeScript sem erros. As evidências do laboratório Windows Server/VM continuam pendentes. Estes resultados locais não comprovam a implantação no IIS, GPO, firewall ou recuperação do serviço na VM.', st['body']),
             Paragraph('Antes do envio: revisar as fontes, completar as evidências pendentes e conferir a versão aprovada pela equipe.', st['body']),
             Spacer(1, 15), Paragraph('Conteúdo desta cópia', st['h3'])]
    for source in sources:
        story.append(Paragraph(escape(source.relative_to(ROOT).as_posix()), st['body']))
    story.append(PageBreak())
    for index, source in enumerate(sources):
        if index:
            story.append(PageBreak())
        story.extend(blocks(markdown_tokens(runtime, source), source, runtime, st))
    while isinstance(story[-1], (PageBreak, NextPageTemplate)):
        story.pop()
    doc.build(story)
    print(f'Criado: {filename}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--runtime', type=Path, required=True, help='Pasta dependencies do runtime Codex')
    parser.add_argument('--documento', choices=['todos', 'cyber', 'togaf'], default='todos')
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    TMP.mkdir(parents=True, exist_ok=True)
    setup_fonts()
    st = styles()
    if args.documento in ('todos', 'togaf'):
        build(args.runtime, 'Arquitetura de soluções baseada no TOGAF', 'ARQUITETURA / TOGAF',
              'arquitetura-togaf-revisao.pdf', [ROOT / 'arquitetura/DOCUMENTO.md'], st)
    if args.documento in ('todos', 'cyber'):
        build(args.runtime, 'Relatório de gestão de riscos e anexo de evidências', 'CYBERSECURITY',
              'cybersecurity-revisao.pdf', [ROOT / 'seguranca/RELATORIO-RISCOS.md', ROOT / 'seguranca/ANEXO-EVIDENCIAS.md'], st)


if __name__ == '__main__':
    main()
