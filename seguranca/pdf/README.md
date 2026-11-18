# PDF único de Cybersecurity

Gera o arquivo que vai na tarefa do Teams: `RELATORIO-RISCOS.md` + `ANEXO-EVIDENCIAS.md` num PDF só, com capa, sumário, o diagrama `fluxo-dados.svg` como figura e as tabelas largas deitadas na página. O roteiro `EVIDENCIAS.md` continua fora do PDF.

## Antes de gerar

1. Preencher os RMs em `metadados.yaml` (campo `author`) e no cabeçalho do `RELATORIO-RISCOS.md`.
2. Trocar cada "A executar" da coluna de laboratório do anexo pelo print com data, hora e ambiente.

## Gerar

Precisa de `pandoc`, `xelatex` (TeX Live) e `rsvg-convert`. Em Linux ou WSL:

```bash
cd seguranca/pdf
rsvg-convert ../fluxo-dados.svg -f pdf -o fluxo-dados.pdf
python3 montar.py .. corpo.md
pandoc metadados.yaml corpo.md -o relatorio-gestao-de-riscos.pdf \
  --pdf-engine=xelatex --from markdown+pipe_tables+raw_tex
```

`montar.py` faz três coisas que o export direto do editor não faz: troca os links relativos por URLs do repositório (no PDF um link para `../api/...` não abre nada), dá às colunas largura proporcional ao conteúdo (sem isso a tabela de ameaças sai com uma palavra por linha) e deita na página as tabelas de seis colunas ou mais.

Sem essas ferramentas, dá para exportar os dois arquivos pelo editor de Markdown e juntar os PDFs, aceitando tabelas mais apertadas.
