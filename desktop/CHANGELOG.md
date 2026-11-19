# Changelog

Todas as mudanças relevantes do terminal desktop ficam registradas aqui.

O formato segue o [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto usa [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [Não publicado]

Sem mudanças pendentes.

## [0.3.0] - 2026-11-19

### Corrigido

- As duas ações de exemplo do modo DEMONSTRAÇÃO ficam sempre no dia da escola. Entre 00h00 e 00h20 (São Paulo), a ação de "20 minutos atrás" caía no dia anterior e sumia da lista do dia; o autoteste falhava nesse horário. Novo teste cobre 00h05, 23h59 e o meio da tarde.

### Adicionado

- Campo `ruaDisponivel` na previsão. Quando é falso, o aviso da linha diz que a previsão usa a última leitura válida da sala.
- Previsão com 503 `sem-dados-para-prever` aparece como "Sem dados para prever", sem tratar como falha; 404 aparece como sala não encontrada.
- Casos de teste de contrato para previsão sem a rua, 503 com e sem o motivo e 404.
- Modo DEMONSTRAÇÃO no modo local: sem API e sem arquivo de dados, a tela mostra cinco salas de exemplo (uma por nível), previsões e duas ações, tudo marcado como DEMONSTRAÇÃO. Os exemplos nunca são gravados.
- Cabeçalho com o nome do sistema, legenda de cores na tela e ícone do termômetro na janela e no executável.
- Casos de teste do encapsulamento do domínio, da demonstração e das telas montadas pelo Designer (6 no total).
- Legenda colorida fixa no mapa, incluindo Frio em azul; cadeiras quadradas e sensores circulares, com códigos C01–C20 e S1–S4 consistentes na planta, lista e PNG.

### Alterado

- Visual nativo com cabeçalho azul-escuro, ações principais turquesa, cartões claros de salas/alertas/ações, grades com avisos legíveis e rolagem em janela menor. Cadastro e mapa usam o mesmo tema; eventos, atalhos e cores térmicas foram preservados.
- `Form1` virou `TelaCoordenacao`, com o layout inteiro no `TelaCoordenacao.Designer.cs`, para abrir no Designer do Visual Studio. O cadastro (`CadastroSala`) ganhou arquivo e Designer próprios.
- Domínio encapsulado: `Sala.Criar` e o construtor de `Acao` validam, as propriedades têm `private set`, e a sala muda só por `Revogar()` e `ReceberLeitura()`. A classe estática `Validacoes` saiu; cada regra ficou na sua entidade.
- `NivelCalor` em PascalCase (`Normal`, `Atencao`, `Cuidado`, `Perigo`), com conversor que mantém o JSON em minúsculas. O contrato com a API e o arquivo local da versão anterior não mudam.
- Um arquivo por classe, nas pastas `Dominio/`, `Servicos/` e `Telas/`.
- Mapa de demonstração ampliado para vinte cadeiras em cinco fileiras de quatro. A API atualiza o arranjo original de seis cadeiras e preserva os IDs nas repetições; configurações personalizadas são recusadas sem alteração.

### Corrigido

- Revogação remota passa a exibir **Sensor revogado** no início dos avisos da grade, usando o estado do mesmo dispositivo informado pela API.
- Textos extensos e precauções ficam na lista lateral do mapa; a planta usa códigos curtos e preserva a identificação dos pontos coincidentes.
- Exportação PNG inclui título, período, dimensões, legenda, planta e todo o texto lateral, inclusive o conteúdo que exige rolagem na janela.

Ensaio visual no Windows em **17/11/2026**, com API Demo e SQL Server Express, documentado em [VALIDACAO-WINDOWS-2026-11-17.md](docs/VALIDACAO-WINDOWS-2026-11-17.md). Após o novo visual, **19 testes e 6 verificações do autoteste passaram**, com saída 0, também no ZIP final extraído. Tela maximizada e em 958 × 707, cadastro, mapas e PNGs foram conferidos; cadastro e ação locais persistiram após reabrir o programa. Relato e autorização da entrevista confirmados em 17/11; Magister e Salgueiro registradas como candidatas ao piloto.

## [0.2.0] - 2026-11-05

### Adicionado

- Solução `TermometroDaSala.Desktop.sln` com o aplicativo e os testes, para abrir no Visual Studio 2022 e rodar com F5.
- Campo `semLeitura` na sala, vindo de `GET /ambientes`.
- Casos de teste com as respostas da API: sala sem leitura e ação com `id` numérico.
- README com seção do Visual Studio 2022, legenda de cores, contratos da API, diagrama de classes e histórico de versões.

### Alterado

- `Acao.Id` passou de texto para número (`long`), como a API devolve. No modo local, as ações são numeradas em sequência.
- O teste de contrato usa as respostas reais da API no lugar de um JSON escrito à mão.
- Comandos de execução sem fonte local de pacotes: `dotnet build` e `dotnet test` direto na solução.

### Corrigido

- A grade quebrava com a API ligada quando havia sala sem leitura (`ultimaLeituraEm: ""`) ou depois da primeira ação registrada (`id` numérico).
- Sala sem leitura aparecia como "Normal"; agora aparece como "Sem leitura", sem cor.

## [0.1.0] - 2026-11-04

### Adicionado

- Terminal Windows Forms da coordenação com salas, níveis por texto e cor e atualização a cada 30 segundos.
- Cadastro validado por campo, registro e consulta das ações do dia e exportação CSV.
- Revogação de sensor com confirmação e identificação explícita do dispositivo.
- Serviços HTTP e JSON local, aviso de indisponibilidade e persistência sem previsões inventadas.
- Atalhos, ordem de tabulação, testes de domínio, persistência e contrato HTTP sem rede.

[Não publicado]: https://github.com/sbaecker2005/termometro-da-sala/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/sbaecker2005/termometro-da-sala/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/sbaecker2005/termometro-da-sala/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/sbaecker2005/termometro-da-sala/releases/tag/v0.1.0
