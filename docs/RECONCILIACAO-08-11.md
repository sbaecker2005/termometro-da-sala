# Reconciliação documental — 08/11/2026

Escopo: incoerências locais da auditoria, somente documentos. Fonte de comportamento: código desta pasta. Nenhum código, teste, fixture, configuração, exportação, notebook ou gerador foi alterado. Não houve comandos de versionamento, rede ou instalação. Identificação e marcadores foram preservados.

Resultados de 08/11/2026 (~04h40), informados pelo coordenador nesta pasta: API 85 aprovados/0 falhas; desktop 18/0; app 20/0; `tsc --noEmit` do app sem erros. Não reexecutados nesta reconciliação.

A contagem estática confirmou API com 50 Fact + 35 InlineData = 85 casos e desktop com 11 Fact + 7 InlineData = 18 casos; a aprovação vem do resultado informado. As contagens de verificações e do notebook foram conferidas por leitura dos arquivos: 97 linhas OK/0 DIVERGE no TSV; 19 células de código/49 saídas no notebook. Esses universos não se somam aos testes. A auditoria original fica como diagnóstico datado, com aviso apontando para este registro.

## Incoerências e referências

Cada referência antes usa a numeração encontrada ao iniciar esta tarefa; depois usa o documento final. O trecho do código é o que decide o comportamento; para resultados executados, a prova é o relato do coordenador, e o código apenas identifica os casos.

| Incoerência | Antes | Depois | Código ou prova que decide |
|---|---|---|---|
| Mapa +1 h | `README.md:7` | `README.md:7` | `api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21` |
| Mapa +1 h | `arquitetura/PITCH.md:34` | `arquitetura/PITCH.md:34` | `api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21` |
| Comando Demo | `api/README.md:42` | `api/README.md:42` | `api/TermometroDaSala.Api/Properties/launchSettings.json:19` |
| Comando Demo | `api/README.md:45` | `api/README.md:45` | `api/TermometroDaSala.Api/Properties/launchSettings.json:19` |
| Testes API | `api/README.md:341` | `api/README.md:342` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Proveniência dos testes | `api/README.md:350` | `api/README.md:351` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Cinco migrações | `api/README.md:251` | `api/README.md:251` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Cinco migrações | `ENTREGA.md:40` | `ENTREGA.md:40` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Cinco migrações | `ENTREGA.md:186` | `ENTREGA.md:186` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Cinco migrações | `infra/EVIDENCIAS.md:101` | `infra/EVIDENCIAS.md:101` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Cinco migrações | `infra/README.md:195` | `infra/README.md:195` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Cinco níveis | `CONTRATO.md:214` | `CONTRATO.md:224` | `api/TermometroDaSala.Api/Dominio/Contrato.cs:13` |
| Frio e Normal | `docs/PLANO.md:134` | `docs/PLANO.md:134` | `api/TermometroDaSala.Api/Dominio/Contrato.cs:13` |
| Valores iniciais | `docs/PLANO.md:163` | `docs/PLANO.md:164` | `esp32/diagram.json:7`; `esp32/diagram.json:18` |
| Passos de demonstração | `docs/PLANO.md:165` | `docs/PLANO.md:166` | `api/TermometroDaSala.Api/Dominio/Contrato.cs:22`; `api/TermometroDaSala.Api/Dominio/Contrato.cs:112` |
| Preparação do circuito | `CONTRATO.md:177` | `CONTRATO.md:180` | `esp32/diagram.json:7`; `esp32/diagram.json:18` |
| Âncora real | `CONTRATO.md:180` | `CONTRATO.md:183` | `api/TermometroDaSala.Api/Controllers/DemoController.cs:74` |
| Âncora real | `CONTRATO.md:184` | `CONTRATO.md:187` | `api/TermometroDaSala.Api/Controllers/DemoController.cs:74`; `api/TermometroDaSala.Api/Controllers/DemoController.cs:79`; `api/TermometroDaSala.Api/Controllers/DemoController.cs:101` |
| Passos de demonstração | `CONTRATO.md:192` | `CONTRATO.md:200` | `api/TermometroDaSala.Api/Dominio/Contrato.cs:22` |
| Renovação do mapa | `docs/MAPA-TERMICO.md:50` | `docs/MAPA-TERMICO.md:50` | `api/TermometroDaSala.Api/Controllers/DemoController.cs:24` |
| Prazos distintos | `esp32/ROTEIRO-VIDEO.md:47` | `esp32/ROTEIRO-VIDEO.md:47` | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:108`; `api/TermometroDaSala.Api/Servicos/MapaTermico.cs:12` |
| Testes atuais | `arquitetura/PITCH.md:103` | `arquitetura/PITCH.md:103` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Testes atuais | `arquitetura/PITCH.md:106` | `arquitetura/PITCH.md:106` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Testes atuais | `arquitetura/PITCH.md:108` | `arquitetura/PITCH.md:108` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Testes atuais | `arquitetura/DOCUMENTO.md:219` | `arquitetura/DOCUMENTO.md:219` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Testes atuais | `arquitetura/DOCUMENTO.md:220` | `arquitetura/DOCUMENTO.md:220` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Testes atuais | `seguranca/RELATORIO-RISCOS.md:15` | `seguranca/RELATORIO-RISCOS.md:15` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Caminhos desktop | `ENTREGA.md:72` | `ENTREGA.md:72` | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:147` |
| Caminhos desktop | `ENTREGA.md:73` | `ENTREGA.md:73` | `desktop/TermometroDaSala.Desktop/Dominio/Acao.cs:1` |
| Caminhos desktop | `ENTREGA.md:74` | `ENTREGA.md:74` | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:108` |
| Testes atuais | `ENTREGA.md:82` | `ENTREGA.md:82` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Contagem do notebook | `ml/README.md:5` | `ml/README.md:5` | `ml/termometro_da_sala.ipynb (JSON: células code e outputs)` |
| Contagem do notebook | `ml/README.md:144` | `ml/README.md:148` | `ml/termometro_da_sala.ipynb (JSON: células code e outputs)` |
| Estudo e previsão servida | `ml/README.md:107` | `ml/README.md:107` | `api/TermometroDaSala.Api/Servicos/MapaTermico.cs:21`; `api/TermometroDaSala.Api/Servicos/PrevisaoSala.cs:241` |
| Universo do simulado | `app/DECISOES.md:21` | `app/DECISOES.md:21` | `app/services/api.ts:37` |
| Estado da auditoria | `docs/AUDITORIA-ENTREGA.md:5` | `docs/AUDITORIA-ENTREGA.md:5` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Atual versus histórico | `api/evidencias/dotnet-test.txt:1` | `api/evidencias/dotnet-test.txt:1` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10`; `api/evidencias/saidas/conferencia.tsv:1` |
| Índice SQL e verificações | `api/evidencias/README.md:19` | `api/evidencias/README.md:19` | `api/evidencias/saidas/B1.txt:6`; `api/evidencias/saidas/B2.txt:7`; `api/evidencias/saidas/conferencia.tsv:1` |
| Testes atuais | `api/evidencias/README.md:45` | `api/evidencias/README.md:29` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| SQL histórico e quinta migração | `api/evidencias/README.md:46` | `api/evidencias/README.md:30` | `api/TermometroDaSala.Api/Dados/Migrations/20261107091447_MapaTermico.cs:8` |
| Testes atuais e seleção histórica | `seguranca/ANEXO-EVIDENCIAS.md:28` | `seguranca/ANEXO-EVIDENCIAS.md:28` | `api/TermometroDaSala.Api.Tests/MapaTestes.cs:100`; `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs:10` |
| Prova local de upload | `seguranca/ANEXO-EVIDENCIAS.md:34` | `seguranca/ANEXO-EVIDENCIAS.md:34` | `api/evidencias/saidas/E07-tamanho-resposta.txt:8` |
| Limite de segredo compartilhado | `CONTRATO.md:77` | `CONTRATO.md:77` | `api/TermometroDaSala.Api/Controllers/DemoController.cs:33` |

| Sinalização de Frio | `docs/PLANO.md:134` | `docs/PLANO.md:134` | `esp32/sketch.ino:123` |
| Prazos de leitura antiga | `esp32/README.md:138` | `esp32/README.md:138` | `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs:108`; `api/TermometroDaSala.Api/Servicos/MapaTermico.cs:12` |
| Referência de IP | `ENTREGA.md:175` | `ENTREGA.md:175` | `infra/EVIDENCIAS.md:10` |
| Prova E13 | `infra/EVIDENCIAS.md:5` | `infra/EVIDENCIAS.md:5` | `infra/10-coletar-evidencias.ps1 (coleta automática)`; `infra/EVIDENCIAS.md:164` |
| Entrevista registrada | `ENTREGA.md:80` | `ENTREGA.md:80` | `desktop/README.md:29` |
| Entrevista e piloto | `ENTREGA.md:238` | `ENTREGA.md:238` | `arquitetura/PITCH.md:24`; `arquitetura/DOCUMENTO.md:262` |
| Busca histórica existente | `ENTREGA.md:264` | `ENTREGA.md:264` | `seguranca/RELATORIO-RISCOS.md:151` |
| Pendências reais do desktop | `ENTREGA.md:265` | `ENTREGA.md:265` | `desktop/README.md:29`; `desktop/README.md:257` |
| Entrevista registrada | `ENTREGA.md:270` | `ENTREGA.md:270` | `arquitetura/PITCH.md:24` |
| Mediana de ação | `arquitetura/PITCH.md:112` | `arquitetura/PITCH.md:112` | `arquitetura/DOCUMENTO.md:271` |
| Mediana de ação | `arquitetura/PITCH.md:166` | `arquitetura/PITCH.md:166` | `arquitetura/DOCUMENTO.md:271` |
| Universo da estimativa de impacto | `docs/PLANO.md:198` | `docs/PLANO.md:199` | `arquitetura/DOCUMENTO.md:260` |
| Datas das coletas | `seguranca/ANEXO-EVIDENCIAS.md:26` | `seguranca/ANEXO-EVIDENCIAS.md:26` | `api/evidencias/saidas/E07-tamanho-resposta.txt:1` |

| PR e tags já registrados | `ENTREGA.md:81` | `ENTREGA.md:81` | `desktop/README.md:257` (registro documental; sem consulta remota) |
| Pendência antiga de coleta do mapa removida; conferência visual e laboratório mantidos | `api/evidencias/README.md:86` | `api/evidencias/README.md:86` | `api/evidencias/saidas/26-resposta.txt:15`, `27-resposta.txt:15`, `28-resposta.txt:15`, `29-resposta.txt:15` e `conferencia.tsv:30` (registros da coleta) |

## Limites preservados

- +1 h: `(índice previsto em +2 h − média dos índices dos sensores recentes) / 2`, somado à temperatura e ao índice. Não é persistência nem o modelo +2 h por ponto do estudo. Sem previsão ou sensores recentes, valores futuros nulos (`MapaTermico.cs:16`).
- Semeadura: descarta zero; âncora de valor no menor minuto positivo e última leitura ao vivo; horários na primeira leitura; atribuição ao primeiro dispositivo encontrado. Não garante curva individual por sensor nem continuidade no ponto zero.
- Leitura antiga: desktop após 10 min; ingestão recusa mais de 10 min atrás/1 min à frente (`Dominio/Contrato.cs:106`); mapa usa janela inclusiva de 15 min; previsão de ambiente consulta até 3 h (`Servicos/PrevisaoSala.cs:233`). Não existe prazo único.
- Concorrência de ingestão, validação completa dos pixels JPEG e compartilhamento do valor público de demonstração permanecem limites de código, fora do escopo. Nenhum ensaio de VM, dispositivo ou revisão visual foi declarado concluído.

Opção menor para a equipe: manter esta reconciliação documental e executar os ensaios já previstos; correções de código ficam para decisão separada.

## Fechamento

- Feito: reconciliação documental com fontes e referências antes/depois; contagens estáticas conferidas.
- Arquivos: `README.md`, `ENTREGA.md`, `CONTRATO.md`, `api/README.md`, `api/evidencias/README.md`, `api/evidencias/dotnet-test.txt`, `arquitetura/DOCUMENTO.md`, `arquitetura/PITCH.md`, `docs/PLANO.md`, `docs/MAPA-TERMICO.md`, `docs/AUDITORIA-ENTREGA.md`, `infra/README.md`, `infra/EVIDENCIAS.md`, `esp32/ROTEIRO-VIDEO.md`, `ml/README.md`, `seguranca/ANEXO-EVIDENCIAS.md`, `seguranca/RELATORIO-RISCOS.md`, `app/DECISOES.md`, `esp32/README.md`, `docs/RECONCILIACAO-08-11.md`.
- Testes para o coordenador: `dotnet test api/TermometroDaSala.sln` (85/0); `dotnet test desktop/TermometroDaSala.Desktop.sln` (18/0); em `app/`, `node --test domain/*.test.ts` (20/0) e `tsc --noEmit` (sem erros). Resultados recebidos, não repetidos nesta tarefa.
- Pendências: ensaios reais, conferência visual, identificação, pacotes, PDFs, gravações e envio permanecem com a equipe, conforme auditoria.
- Premissas que caíram: nenhuma premissa impeditiva; divergências documentais confirmadas e corrigidas conforme solicitado.
Não foram reexecutadas suítes, banco, firmware, notebook nem VM. Risco: evidência histórica e resultado informado não comprovam integração ao vivo.
