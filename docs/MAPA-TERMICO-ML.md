# Mapa térmico da Sala 7 — estudo simulado

## Dados e escopo

Sala de 7 × 9 m, quatro sensores: janela (0, 4), fundo/parede externa (7, 9),
porta (7, 0) e centro (3,5, 4,5). Seis lugares nas posições (1, 4), (6, 8),
(2, 2), (5, 2), (2, 6), (5, 6); os dois primeiros são vulneráveis.
Não há nomes nem outros dados pessoais.

Interpretamos os 14 dias como **14 dias por cenário sazonal**: 1–14 de janeiro
e 1–14 de julho de 2026, hora local de São Paulo, de hora em hora, seed 42.
São 672 instantes, 2.688 leituras de sensores simuladas e 4.032 estimativas de
lugares, totalizando 6.720 registros. Todos levam `origem=simulada`.
Nenhum valor representa medição real. `tipo` distingue sensores de lugares.

A curva tem ciclo diário, variação diária com desvio padrão de 0,8 °C e ruído
por sensor com desvio padrão de 0,2 °C. A janela recebe aquecimento adicional
à tarde no verão; o fundo esfria de madrugada no inverno. A umidade também
varia. O índice de calor usa a fórmula já presente no notebook.
O CSV é `ml/dados/sala7-simulada.csv`, reproduzido pelo notebook.

## Interpolação, grupos e precauções

`ml/mapa.py` usa IDW com pesos 1/d² para temperatura e índice separadamente,
grade de 0,5 m incluindo as bordas (15 × 19 = 285 células). Sobre o sensor,
devolve exatamente sua leitura. `None` e NaN ficam de fora; sem sensores
válidos o resultado é `None`, nunca zero inventado. `leituras_recentes` seleciona
a leitura mais recente por sensor na janela inclusiva de 15 minutos, excluindo
horários futuros; deve ser chamada antes do IDW em um fluxo com leituras ao vivo.
Na simulação, os quatro sensores são simultâneos em cada retrato.

O caso compartilhável com a aplicação está em `ml/caso-idw.json`:
sensores (0, 0) = 20 °C e (2, 0) = 28 °C, mais um sensor sem leitura.
Sobre o primeiro, o resultado é 20; no meio, 24; em (0,5, 0),
os pesos são 4 e 4/9, resultando em **20,8 °C**.
Os testes também cobrem um sensor único, ausência total e leituras vencidas.

K-means compara os dez pontos pelos seus 24 valores médios horários de índice,
separadamente por estação, sem padronização, `random_state=42`, `n_init=10`.
A maior silhueta entre k=2, 3 e 4 escolhe o grupo; empate favorece k menor.

| Estação | Silhueta k=2 | k=3 | k=4 | Escolhido |
|---|---:|---:|---:|---:|
| Inverno | 0,741533 | 0,635126 | 0,639685 | 2 |
| Verão | 0,691635 | 0,661615 | 0,676744 | 2 |

Crônico significa acima/abaixo da média dos quatro sensores em **todas as
leituras individuais** da estação. Inversão de sinal ou igualdade torna o ponto
variável, mesmo que as 24 médias horárias tenham o mesmo sinal. Janela e lugares
1 e 3 são crônicos quentes nas duas estações; lugar 5, apenas no inverno.
Fundo e lugar 2 são crônicos frios nas duas. Centro e porta são variáveis nas duas
estações. Os detalhes ficam em
`ml/grupos-pontos.csv` e `ml/silhuetas-pontos.csv`.

Os dois mapas do notebook mostram dia 12 às 15h no verão e às 4h no inverno,
com eixos em metros, superfície interpolada, cor de cada ponto pela faixa e
marcador pelo grupo. Faixas do índice: frio <18; normal [18,27); atenção
[27,32); cuidado [32,41); perigo ≥41 °C. Azul indica frio e vermelho, cuidado/perigo.

Lugar vulnerável em frio, cuidado ou perigo recebe sugestão de troca para o
**lugar** cujo índice esteja mais próximo da faixa normal, apenas se melhorar.
Empates favorecem normal e depois nome, para resultado determinístico.
Sensores não são destinos. Sem alternativa melhor, procurar outro ambiente.
Amplitude ≥4 °C entre os pontos válidos, incluindo sensores, gera aviso de
ambiente desigual com os rótulos dos extremos e a diferença calculada.
Os textos incluem os valores calculados; não prometem lugar seguro quando
nenhuma alternativa melhora a situação.

## Previsão por ponto em duas horas

A árvore de `modelo.json` fica congelada. Uma regressão linear aprende seu
resíduo usando os oito atributos originais mais `desvio_ponto`: índice do ponto
menos média atual dos quatro sensores. Predição final = árvore + correção.
`modelo_pontos.json` contém a árvore base, coeficientes, intercepto, atributos,
origem simulada e métricas; tem formato separado `tipo=correcao_linear`.
O arquivo original não é substituído. A aplicação precisará interpretar este
formato para servir a previsão por ponto; este trabalho entrega a parte Python.

Treino: dias 1–10 de cada estação, com alvo +2 h dentro do mesmo período.
Teste: dias 11–14 de cada estação. São **4.740 exemplos de treino e 1.880 de
teste**; faltas de vizinhos e cruzamentos da fronteira são removidos.
Não há embaralhamento temporal, ajuste de parâmetros ou escolha de modelo no teste.
A divisão serve ao estudo simulado; os lugares interpolados não são observações
independentes dos sensores.

Mesmas métricas do notebook: MAE e RMSE em °C, R² e F1 macro dos níveis presentes,
agora incluindo frio. Números de `ml/metricas-pontos.csv`:

| Modelo | MAE | RMSE | R² | F1 macro |
|---|---:|---:|---:|---:|
| Persistência (daqui a 2 h = agora no ponto) | 2,034861 | 2,639037 | 0,894602 | 0,653548 |
| Árvore atual aplicada aos atributos do ponto | 2,574577 | 4,199007 | 0,733169 | 0,503186 |
| Árvore atual + desvio atual | 2,600614 | 4,075789 | 0,748600 | 0,583804 |
| Árvore + correção linear (`modelo_pontos`) | **1,341040** | **1,869924** | **0,947084** | **0,797982** |

A correção reduz o MAE em 0,693820 °C (34,10%) frente à persistência nesta
simulação. A árvore original e a soma direta do desvio **não melhoram** o baseline.
As métricas combinam sensores e lugares, verão e inverno; não comprovam ganho
com medições reais nem permitem comparação direta com o teste INMET de 2025.
A exportação é conferida contra a regressão em todos os 1.880 exemplos de teste,
com tolerância absoluta de 1e-9 °C.

## Reprodução e limitações

Sem novos pacotes ou acesso à rede, a partir da raiz:

```powershell
python ml/gerar_notebook.py
python -m pytest ml -o cache_dir=ml/.pytest_cache
```

O primeiro comando monta o notebook; executar suas células do início ao fim
produz os dados, gráficos, grupos, métricas e o modelo separado. As funções de
`mapa.py` são incluídas pelo gerador para o notebook poder executar sozinho.
O modelo atual ainda é exportado pela seção anterior, como já acontecia.
A seção do mapa cria `dados/` antes de exportar, inclusive em uma sessão sem
essa pasta, como no Colab; executar novamente também funciona.

Verificação local: **22 testes passaram**, incluindo a regressão de alertas já
existente, caso IDW calculado à mão, grade, janela de leituras, faixas, precauções,
grupos determinísticos, inversões individuais e igualdade à média, exportação
sem `dados/` (duas execuções), fronteiras temporais e paridade do modelo exportado.
As 19 células de código do notebook foram executadas em sequência, com saídas
e gráficos gravados, usando IPython no mesmo processo; `modelo.json` permaneceu
idêntico. O Colab remoto não foi executado; a regressão local usa a célula do
notebook com `PASTA=Path('.')` em diretório temporário vazio, sem acesso à rede.
Houve apenas aviso do ambiente sobre identificação de núcleos físicos no teste.

Precisamos de **mais sensores reais, calibrados**, em diferentes posições e
alturas, e de um histórico real maior para validar interpolação, grupos e
previsões. IDW ignora paredes, obstáculos, sol e circulação de ar; suaviza os
extremos não medidos. Os grupos recuperam padrões que foram inseridos na
simulação. Desvio do ponto depende de sensores disponíveis e sincronizados;
não foi treinado para falhas ou mudanças na disposição da sala. As precauções
apoiam decisão local e não substituem procedimentos da escola ou abrigo.
