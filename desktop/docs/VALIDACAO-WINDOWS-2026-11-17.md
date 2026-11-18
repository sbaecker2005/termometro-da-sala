# Validação do desktop no Windows — 17/11/2026

Ensaio pela interface real do Windows Forms, com API ASP.NET Core em Demo e SQL Server Express 2022. Banco inicial exclusivo: `TermometroGS_20261117_Local`. **Resultado atual: as correções e o novo visual foram revalidados; 19 testes Windows e 6 verificações do autoteste passaram.** A pedido do usuário, o mapa também foi ampliado para 20 cadeiras e ganhou legenda colorida fixa, incluindo frio em azul. Os ensaios anteriores estão preservados abaixo como histórico; a [integração final de ML](#integração-final-de-ml--17112026-às-09h45) acrescenta o iniciador em banco novo e a previsão após a correção do Demo.

## Primeiro ensaio — antes das correções

| Verificação | Resultado |
|---|---|
| Conexão e previsão | A tela mostrou API conectada, leitura e previsão de +2 h da Sala 3. Leituras envelhecidas receberam o aviso Leitura antiga. |
| Nome obrigatório | Salvar sem nome manteve o formulário aberto e mostrou Informe o nome da sala junto ao campo. |
| Identificador inválido | ID `x` foi recusado com Use sala-NN, por exemplo sala-03. |
| Cadastro válido | `sala-06`, nome Sala 6 - ensaio desktop, andar 0, face norte, sem ventilador: apareceu na grade como Sem leitura. Registro também confirmado no SQL Server. |
| Ação inválida | Descrição `abc` foi recusada, com indicador de erro, sem adicionar ação na grade. |
| Ação válida | Ensaio: ventilacao e agua disponibilizadas. foi gravada para sala-06, nível Normal, às 01:30:29 de São Paulo. Conferida na grade, no SQL e após reiniciar a API. |
| CSV | Exportado pela interface. Arquivo validacao-acoes-2026-11-17.csv contém cabeçalho, sala, nível, descrição e horário esperados; separador ponto e vírgula e BOM UTF-8 confirmados. |
| Mapa Agora | Sala 7: planta 6 × 4 m, quatro sensores e seis cadeiras. Janela a 35 °C, demais sensores a 24 °C. Grade colorida, lista textual e precauções presentes. |
| Mapa +1 h | Seletor alterou valores (janela a 34,8 °C, demais sensores a 23,8 °C no instante do ensaio) e mostrou a ressalva da aproximação linear nas precauções. |
| PNG | Exportado pela interface e reaberto para inspeção. Arquivo mapa-termico.png, 50.371 bytes. Apresenta as ressalvas de layout abaixo. |
| Cancelar revogação | Sala 1: diálogo com Não como padrão; após cancelar, SQL confirmou esp32-sala-01 com Ativo=1. |
| Confirmar revogação | Confirmar alterou o mesmo sensor para Ativo=0 no SQL. O aviso correspondente não apareceu na grade. |
| API indisponível | Parada somente a API deste ensaio. Atualizar mostrou carregamento e depois MODO LOCAL — DEMONSTRAÇÃO, com aviso de alteração da origem e salas de exemplo. |
| Recuperação | Reiniciada a API no mesmo banco. Atualizar na mesma janela recuperou API conectada, sala-06 e a ação persistida. |

Para o mapa, as quatro leituras simuladas da Sala 7 foram renovadas por `/demo/mapa`, pois as anteriores tinham envelhecido. Isso não é evidência de sensor físico ou Wokwi.

## Problemas encontrados no primeiro ensaio — corrigidos

1. **Revogação sem indicação na grade.** A operação desativa o sensor, mas a tela continua sem Sensor revogado. `Telas/TelaCoordenacao.cs:107` depende de `Sala.SensorRevogado`; `api/TermometroDaSala.Api/Contratos/Saidas.cs` não declara o campo e `Controllers/AmbientesController.cs:119` só inclui o identificador do dispositivo. A API precisa informar o estado do dispositivo selecionado para a tela poder exibi-lo.
2. **Textos sobrepostos no mapa.** Rótulos extensos de sensores/cadeiras coincidentes ou próximos se sobrepõem no centro e no topo direito da planta, tanto em Agora quanto em +1 h. A lista lateral continua legível. `Telas/TelaMapa.cs:74` desenha `p.Texto` completo, incluindo recomendações, junto ao ponto. Evidência preservada em mapa-termico.png.
3. **PNG corta parte da janela.** O arquivo exportado inclui a barra de título, mas perde o rodapé com período/dimensões e parte da borda direita, visíveis na janela real. Em `TelaMapa.Exportar`, o bitmap usa ClientSize, enquanto DrawToBitmap recebe o formulário inteiro. Ajustar a captura e repetir a inspeção do PNG.

## Correções e revalidação

| Verificação | Evidência na versão final |
|---|---|
| Estado do sensor | A API retorna `sensorRevogado` do mesmo dispositivo selecionado por `dispositivoId`, com falso quando não há sensor. Na tela, o aviso vem primeiro para não ficar escondido pela coluna estreita. Sala 1, já revogada, apareceu corretamente. |
| Cancelar e confirmar novamente | Sala 2 ativa: cancelar manteve o aviso sem revogação; confirmar mostrou imediatamente **Sensor revogado**. A consulta real à API confirmou verdadeiro para salas 1/2 e falso para sala 6, sem sensor. |
| Identificação do mapa | Textos extensos saíram da planta e ficaram na lista. No acabamento final, cadeiras recebem C01–C20 e sensores S1–S4. Pontos exatamente coincidentes mantêm seus códigos individuais, desenhados juntos. Nenhuma sobreposição observada na cena validada. |
| 20 cadeiras | Cena antiga de seis cadeiras migrada para 20 em cinco fileiras de quatro, dentro de 6 × 4 m. Quatro sensores permanecem. Cadeiras são quadrados; sensores, círculos. Lista e planta mostram os 24 pontos. A migração só aceita o layout original exato; personalizações são recusadas sem gravação. |
| Frio / Agora | `/demo/mapa?periodo=madrugada`: fundo a 15 °C, duas cadeiras classificadas como frias e região azul visível. Legenda fixa mostra todas as cores, mesmo quando uma faixa não ocorre nos dados. |
| Frio / +1 h | Fundo passou a aproximadamente 14,8 °C no instante do ensaio. Numeração e disposição permaneceram iguais. Ressalva da aproximação presente nas precauções. |
| Calor / Agora e +1 h | `/demo/mapa?periodo=tarde`: janela a 35 °C e aproximadamente 34,8 °C em +1 h. Regiões verde/amarela/laranja/vermelha, 20 cadeiras e números legíveis em ambos os períodos. |
| PNG completo | Exportação pela interface gera título, período/dimensões, legenda, planta e todo o texto lateral, inclusive o conteúdo que exige rolagem na janela. Os três arquivos foram reabertos e inspecionados, sem corte nas bordas ou nas precauções. |
| Compilação | API e desktop, incluindo projetos de testes, compilaram com zero erros e zero avisos. Suites e autoteste não foram executados, conforme divisão com o Cérebro. |
| Publicação usada | A primeira publicação preservou uma DLL antiga do kit com timestamp posterior à compilação. Após detectar o desenho antigo na tela e hashes divergentes, a saída foi copiada explicitamente e o hash da DLL publicada foi comparado ao binário Release. A revalidação final usou `executavel-desktop/TermometroDaSala.Desktop.exe`. |

Arquivos exportados nesta revalidação:

| Arquivo | Dimensões | Bytes |
|---|---|---|
| [mapa-20-cadeiras-frio.png](mapa-20-cadeiras-frio.png) | 1084 × 813 | 57.136 |
| [mapa-20-cadeiras-frio-1h.png](mapa-20-cadeiras-frio-1h.png) | 1084 × 893 | 62.497 |
| [mapa-20-cadeiras-calor.png](mapa-20-cadeiras-calor.png) | 1084 × 813 | 61.707 |

`mapa-termico.png` permanece como evidência anterior. `mapa-termico 2.png` é a exportação de +1 h preservada também com o nome descritivo acima.

O mapa atual interpola as leituras pela distância (IDW). O ML prevê o índice do ambiente em +2 h; +1 h aplica metade dessa variação aos sensores e interpola os lugares. Não existe modelo de temperatura individual por cadeira. As cenas usadas aqui são simuladas, sem sensor físico ou Wokwi.

## Acabamento final — códigos de cadeiras e sensores

- C01–C20 substituem os números gerais nas vinte cadeiras; S1–S4 identificam os quatro sensores. A legenda, a planta, a lista acessível e o PNG usam os mesmos códigos. Códigos são apenas apresentação: IDs e nomes da API/banco não foram alterados.
- Tela real conferida em Agora e Previsão +1 h. Os 24 códigos permaneceram presentes, únicos e nas mesmas posições. Símbolos foram ajustados para manter códigos, cores e formas legíveis.
- Novo [PNG com C01–C20 e S1–S4](mapa-cadeiras-C01-C20-sensores-S1-S4.png) exportado pelo desktop publicado e reaberto para inspeção: 1084 × 813 pixels, 61.105 bytes, lista completa e precauções sem cortes. As imagens anteriores documentam a etapa anterior ao acabamento.
- `MapaTests` atualizado para verificar contadores separados, preservação de pontos/precauções coincidentes, C01–C20 e estabilidade entre períodos. Compilação da solução e publicação Release concluídas sem erros/avisos; hash da DLL publicada conferido. Revisão independente de leitura não encontrou falha material. Testes automatizados continuam reservados ao coordenador.

## Estado deixado — ensaio anterior

- API em `http://localhost:5080`, PID 23588; desktop publicado PID 31860. Mapa da Sala 7 aberto em Agora, cenário frio, com C01–C20 e S1–S4. PIDs são apenas referência deste ensaio.
- Banco preservado, incluindo sala-06 e a ação. Sensores das Salas 1 e 2 permanecem revogados apenas nesse banco de ensaio. Sala 3 preservada; Sala 7 usa agora o layout demonstrativo de 20 cadeiras.
- Após reiniciar a API, uma nova chave temporária foi gerada apenas na memória e herdada pela nova janela do desktop. Não foi gravada em arquivo ou log.
- Nenhum arquivo de dados locais foi criado pelo ensaio; o modo local foi validado apenas quanto à entrada/aviso/retorno à API.

## Testes para o coordenador — orientação do ensaio anterior

Na raiz do kit:

```powershell
dotnet test .\desktop\TermometroDaSala.Desktop.sln
dotnet test .\api\TermometroDaSala.sln
dotnet run --project .\desktop\TermometroDaSala.Desktop -- --autoteste
```

Esperado: testes e autoteste aprovados, incluindo revogação do dispositivo selecionado, códigos separados por tipo, pontos coincidentes, 20 cadeiras, frio, migração idempotente e preservação de personalizações. Não executados nesta etapa, conforme divisão de trabalho com o Cérebro. A repetição visual de revogação, mapas e PNGs está concluída.

## Arquivos alterados

- `api/TermometroDaSala.Api/Contratos/Saidas.cs`
- `api/TermometroDaSala.Api/Controllers/AmbientesController.cs`
- `api/TermometroDaSala.Api/Controllers/DemoController.cs`
- `api/TermometroDaSala.Api.Tests/ClienteDesktopTestes.cs`
- `api/TermometroDaSala.Api.Tests/MapaTestes.cs`
- `desktop/TermometroDaSala.Desktop/Dominio/Mapa.cs`
- `desktop/TermometroDaSala.Desktop/Telas/TelaMapa.cs`
- `desktop/TermometroDaSala.Desktop/Telas/TelaCoordenacao.cs`
- `desktop/TermometroDaSala.Desktop.Tests/MapaTests.cs`
- `CONTRATO.md`, `docs/MAPA-TERMICO.md`, `desktop/README.md`, este relatório e PNGs acima.
- `executavel-desktop/`: publicação atualizada (EXE, DLL, PDB e arquivos de execução/configuração gerados pelo projeto).

## Fechamento do ensaio anterior

Atualização da preparação para o grupo (17/11, aproximadamente 02h18 em São Paulo): API e projetos de testes da API/desktop compilados em Release, sem avisos nem erros. As suítes não foram executadas. API reiniciada com as correções de contrato e concorrência, PID 18544; desktop PID 3452. Banco isolado anterior preservado, MQTT desligado, chave temporária somente nos processos. `GET /ambientes` respondeu 200. Na tela, "API conectada" e a Sala 7 com C01-C20/S1-S4 foram reconferidos. O cenário de madrugada foi renovado pelo endpoint Demo: S2, C16 e C20 em frio azul. O PNG final já exportado foi aberto novamente e permanece completo e legível. Logs desta inicialização em `api/TermometroDaSala.Api/bin/validacao-grupo.out.log` e `validacao-grupo.err.log`.

Comandos de compilação usados nesta atualização (não executam testes):

```powershell
dotnet build .\api\TermometroDaSala.Api.Tests\TermometroDaSala.Api.Tests.csproj -c Release --no-restore --nologo -v minimal -m:1 /nodeReuse:false
dotnet build .\desktop\TermometroDaSala.Desktop.Tests\TermometroDaSala.Desktop.Tests.csproj -c Release --no-restore --nologo -v minimal -m:1 /nodeReuse:false
```

A compilação paralela padrão da solução encerrou com falha sem diagnóstico nesta sessão restrita; a compilação explícita dos projetos acima, com um processo MSBuild, concluiu com código 0. A aprovação de compilação não é resultado da suíte.

- Feito: três correções concluídas; mapa ampliado para 20 cadeiras; azul de frio e distinção cadeira/sensor conferidos; cancelamento/confirmação da revogação e mapa nos dois períodos/cenários revalidados; três PNGs da primeira etapa e um PNG final com C01–C20/S1–S4 inspecionados; compilações sem erros/avisos e publicação conferida por hash.
- Arquivos: lista acima; CSV e PNG antigos preservados. Logs de reinício em `api/TermometroDaSala.Api/bin/correcao-ui.out.log` e `correcao-ui.err.log`.
- Testes para o coordenador: comandos acima; esperado todos aprovados.
- Pendências: executar suítes/autoteste e conferir ZIP extraído. Na entrega geral de ML/SOA/C#, seguem evidências do Insomnia, vídeo ML, pacote final, Git e envio pela equipe.
- Premissas que caíram: publicar por cima do kit não garantiu atualizar a DLL com timestamp futuro; hashes divergentes e desenho antigo revelaram isso. Publicação foi corrigida e conferida por hash antes do ensaio final.

Não foram validados persistência de novos registros no modo local, câmera/Wokwi, Insomnia, ZIP extraído nem suíte automatizada. Limites: dados simulados, +1 h aproximado e posições distintas muito próximas ainda podem sobrepor os números; isso não ocorreu no layout validado.

## Sessão do novo visual — 17/11/2026

Após autorização de Samuel para executar as suítes, o desktop recebeu cabeçalho azul-escuro, botões turquesa, cartões claros de resumo, grades com avisos quebrados em linhas e mais espaço entre os controles. Os resumos são calculados a partir das salas e ações carregadas; sala sem leitura não é contada como alerta. Eventos, atalhos, contratos, códigos C01–C20/S1–S4 e cores térmicas foram preservados. A revisão independente de leitura não encontrou defeito material.

| Verificação pela interface real | Resultado observado nesta sessão |
|---|---|
| Painel maximizado | Cabeçalho, origem API, três cartões, comandos, legenda, grades e registro de ação legíveis. |
| Janela de 958 × 707 | Rolagem permitiu acessar o conteúdo e os botões; aviso longo de modo local permaneceu legível. |
| Cadastro | Validação dos campos e cancelamento conferidos no novo formulário. |
| Mapa | Agora e Previsão +1 h conferidos, incluindo cena fria, vinte cadeiras C01–C20 e quatro sensores S1–S4. |
| PNG | Os dois arquivos abaixo foram exportados e abertos: legenda, lista completa, C20, precauções e rodapé sem cortes. |
| API indisponível | A API do ensaio foi parada intencionalmente. O desktop passou ao modo local, com origem e aviso longo visíveis. |
| Persistência local após reabrir | Às 02h58 de São Paulo, com API desligada e arquivo local originalmente ausente, foram cadastrados `sala-99` / `Validação local descartável` e a ação `Teste de persistência local após reiniciar.`. Depois de fechar e reabrir o desktop, o modo local mostrou uma sala e uma ação, com os valores corretos. São dados exclusivos do ensaio. |

| PNG do novo visual | Dimensões | Bytes |
|---|---|---|
| [mapa-visual-final-agora.png](mapa-visual-final-agora.png) | 1144 × 929 | 66.714 |
| [mapa-visual-final-1h.png](mapa-visual-final-1h.png) | 1144 × 1018 | 72.341 |

### Verificação automatizada concluída

| Execução em 17/11 | Resultado | Registro |
|---|---|---|
| API, incluindo regressões de contrato e replay | 100/100 aprovados | [api-inicial-20261117.trx](../../docs/resultados-testes-2026-11-17/api-inicial-20261117.trx) |
| Desktop após o novo visual | 19/19 aprovados, zero falhas | [desktop-visual-20261117.trx](../../docs/resultados-testes-2026-11-17/desktop-visual-20261117.trx) |
| Autoteste após o novo visual | 6 verificações aprovadas, saída 0 | [desktop-autoteste-visual-20261117.txt](../../docs/resultados-testes-2026-11-17/desktop-autoteste-visual-20261117.txt) |
| ZIP desktop extraído, versão anterior ao novo visual | 19/19 e autoteste 6/6 aprovados, saída 0 | [desktop-zip-20261117.trx](../../docs/resultados-testes-2026-11-17/desktop-zip-20261117.trx); autoteste registrado na saída da execução |
| Mobile após seu acabamento visual | 23/23 aprovados; `tsc --noEmit` sem erros | [mobile.txt](../../docs/resultados-testes-2026-11-17/mobile.txt) |

Comandos do desktop executados na raiz do kit, após compilação Release sem erros nem avisos:

```powershell
dotnet test .\desktop\TermometroDaSala.Desktop.Tests\TermometroDaSala.Desktop.Tests.csproj -c Release --no-restore -m:1 /nodeReuse:false --logger 'trx;LogFileName=desktop-visual-20261117.trx' --results-directory docs/resultados-testes-2026-11-17
dotnet run --project .\desktop\TermometroDaSala.Desktop\TermometroDaSala.Desktop.csproj -c Release --no-build -- --autoteste
```

Os testes usam handlers em memória e arquivos temporários próprios; o autoteste encerra antes da inicialização normal do aplicativo. Não usaram o banco SQL do ensaio nem os dados locais do usuário. No ZIP antigo, a consulta de vulnerabilidades do NuGet emitiu NU1900 por indisponibilidade da fonte; restauração, compilação, 19 testes e autoteste concluíram com saída 0.

### Publicação e limites atuais

A interface desta sessão foi executada de `output/desktop-visual-20261117`, cuja DLL e EXE foram comparados por SHA-256 com a saída Release. A pasta `executavel-desktop` recebeu cópia explícita dessa publicação após a conferência, preservando o processo aberto em `output`. Hashes da versão validada:

- EXE: `E8E158E5407F2E387043237E4A8903CDF3503CDC18C864F8949DFBF93270ACDF`.
- DLL: `50B0D7E0F09C6D9CC6E027E9D810468479600DBA8B0D4E543870F5A3185FC482`.

Na passagem inicial desta sessão, a API PID 31012 foi parada intencionalmente e o desktop PID 34612 entrou no modo local. O ensaio posterior de persistência reabriu o desktop como PID 11668. A coordenação conferiu uma sala e uma ação próprias do ensaio e moveu o arquivo para [desktop-persistencia-local.json](../../docs/resultados-testes-2026-11-17/desktop-persistencia-local.json), restaurando a ausência original de `dados.json` em LocalAppData. Esta atualização documental não alterou esses dados. API PID 33976 e `executavel-desktop` PID 6172 foram reabertos; a interface da publicação de entrega confirmou **API conectada** e dados do servidor. Esses PIDs são apenas referências temporárias.

Pendências anotadas naquela passagem: preparar e conferir o ZIP que contenha o novo visual; observar o cadastro e o PNG em DPI ampliado; ensaios nativos Mobile e de VM/Wokwi; evidências manuais do Insomnia; conferência do enunciado, vídeos, Git e envio pela equipe. A atualização abaixo e o [fechamento corrente](../../docs/FECHAMENTO-ML-SOA-CSHARP.md) substituem essa lista histórica. As cenas continuam simuladas, e +1 h continua sendo aproximação, sem modelo por cadeira.

## Integração final de ML — 17/11/2026 às 09h45

O comando `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\ferramentas\abrir-apresentacao.ps1`, executado na raiz do kit, compilou a API, preparou um banco exclusivo `TermometroGS_Apresentacao_20261117_094533`, gerou uma chave temporária somente em memória e abriu a publicação `executavel-desktop`. O script recusa a porta 5080 ocupada, não encerra outro ensaio e não apaga bancos anteriores.

| Verificação | Resultado observado |
|---|---|
| Previsão real da API, Sala 3 | Rua 30,3 °C + delta 12,5 °C = sala prevista 42,8 °C; `nivelPrevisto=perigo`, `semLeiturasRecentes=false`, `demonstracao=true`. [Resposta JSON](../../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/previsao-ml.txt) |
| Interface da publicação de entrega | A árvore de acessibilidade e a [captura da tela](../../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/desktop-previsao-ml.png) mostraram Sala 3, nível atual Cuidado extremo, previsão Perigo em +2 h e identificação de DEMONSTRAÇÃO. Painel maximizado em 1920 px conferido sem cortes. Não é leitura em campo. |
| Mapa e exportação repetidos | [Tela do mapa](../../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/mapa-tela.png) e [PNG exportado](../../output/apresentacao/TermometroGS_Apresentacao_20261117_094533/mapa-exportado.png) inspecionados: C01–C20, S1–S4, frio em azul, legenda e lista completas sem cortes. |
| Demo repetido em outro dia | A preparação passou a ancorar o histórico na leitura ao vivo mais recente. O teste de regressão falhou antes da correção e passou depois, preservando as leituras reais. |
| Suíte da API depois da correção | **101/101 aprovados**, sem falhas ou ignorados; [TRX atual](../../docs/resultados-testes-2026-11-17/api-fechamento-101.trx). O resultado 100/100 acima permanece como histórico anterior à nova regressão. |
| ZIP com o novo visual | A cópia extraída passou **19/19 testes e 6/6 verificações do autoteste**; [TRX](../../docs/resultados-testes-2026-11-17/desktop-zip-final-20261117.trx) e [autoteste](../../docs/resultados-testes-2026-11-17/desktop-autoteste-zip-final-20261117.txt). A atualização documental posterior exige atualizar o arquivo e seu manifesto, sem mudança de binário. |

A previsão é da sala para +2 h; o mapa +1 h continua uma aproximação espacial documentada. As leituras usadas na apresentação foram simuladas por HTTP. Não representam Wokwi, medições de Magister/Salgueiro nem validação independente do modelo em campo. O enunciado oficial e a entrevista foram conferidos/confirmados; DPI ampliado não foi inspecionado e é um limite de cobertura visual, não exigência explícita de C#. Captura visual complementar, mapa/PNG e [Insomnia 29/29](../../api/evidencias/insomnia-2026-11-17/README.md) estão concluídos. Publicação pelo Cérebro, vídeo ML e envio permanecem separados dos testes aprovados.

## Modelo geral com ajuste por sala — conferência atual

O [relatório da versão atual](modelo-geral-v1/VALIDACAO.md) substitui as referências de binários e pendências visuais anteriores. Tela, modo local/retorno à API, horários, avisos, mapa Agora/+1 h e PNGs completos foram conferidos novamente na revisão do modelo geral. SQL Server real: 98 verificações do coletor e 33 do ensaio HTTP/MQTT/SQL aprovadas. O mapa +1 h usa o tempo restante até o horário-alvo; fora do Demo, esse alvo é a observação externa +2 h, podendo ficar a menos de duas horas da consulta. Binários do iniciador e pacotes foram atualizados, com os registros de integridade ligados no relatório.
