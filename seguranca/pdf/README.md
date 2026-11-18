# PDF único de Cybersecurity

Gera o arquivo que vai na tarefa do Teams: `RELATORIO-RISCOS.md` + `ANEXO-EVIDENCIAS.md` num PDF só, com capa, sumário, o diagrama `fluxo-dados.svg` como figura, as tabelas largas deitadas na página, os trechos de código e os prints do anexo. O roteiro `EVIDENCIAS.md` continua fora do PDF.

## Gerar

Precisa de `pandoc`, `xelatex` (TeX Live, com os pacotes `pdflscape`, `fvextra`, `float`, `fancyhdr`, `hyphenat` e `ragged2e`) e `rsvg-convert`. Em Linux ou WSL:

```bash
cd seguranca/pdf
rsvg-convert ../fluxo-dados.svg -f pdf -o fluxo-dados.pdf
python3 montar.py .. corpo.md
pandoc metadados.yaml corpo.md -o relatorio-gestao-de-riscos.pdf \
  --pdf-engine=xelatex --from markdown+pipe_tables+raw_tex
```

As partes dos prints em `prints/` já estão no repositório. Se um print de `../evidencias/prints/` mudar, refaça as partes antes de montar (precisa do Pillow):

```bash
python3 fatiar_prints.py ../evidencias/prints prints
```

`montar.py` faz quatro coisas que o export direto do editor não faz: troca os links relativos por URLs do repositório (no PDF um link para `../api/...` não abre nada), dá às colunas largura proporcional ao conteúdo (sem isso a tabela de ameaças sai com uma palavra por linha), deita na página as tabelas de seis colunas ou mais e troca cada print do anexo pelas suas partes, cada uma do tamanho de uma página, para o texto do print continuar legível.

Sem essas ferramentas, dá para exportar os dois arquivos pelo editor de Markdown e juntar os PDFs, aceitando tabelas mais apertadas e prints reduzidos.

## Conferir antes de enviar

- A figura do fluxo de dados aparece e as tabelas deitadas cabem na página.
- Os prints e os trechos de código do anexo estão legíveis, sem linha cortada na margem.
- Nenhum título fica sozinho no pé da página.
