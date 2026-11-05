# Changelog

Todas as mudanças relevantes do terminal desktop ficam registradas aqui.

O formato segue o [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto usa [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [Não publicado]

### Adicionado

- Campo `ruaDisponivel` na previsão. Quando é falso, o aviso da linha diz que a previsão usa a última leitura válida da sala.
- Previsão com 503 `sem-dados-para-prever` aparece como "Sem dados para prever", sem tratar como falha; 404 aparece como sala não encontrada.
- Casos de teste de contrato para previsão sem a rua, 503 com e sem o motivo e 404.
- Modo DEMONSTRAÇÃO no modo local: sem API e sem arquivo de dados, a tela mostra quatro salas de exemplo (uma por nível), previsões e duas ações, tudo marcado como DEMONSTRAÇÃO. Os exemplos nunca são gravados.
- Cabeçalho com o nome do sistema, legenda de cores na tela e ícone do termômetro na janela e no executável.
- Casos de teste do encapsulamento do domínio, da demonstração e das telas montadas pelo Designer (6 no total).

### Alterado

- `Form1` virou `TelaCoordenacao`, com o layout inteiro no `TelaCoordenacao.Designer.cs`, para abrir no Designer do Visual Studio. O cadastro (`CadastroSala`) ganhou arquivo e Designer próprios.
- Domínio encapsulado: `Sala.Criar` e o construtor de `Acao` validam, as propriedades têm `private set`, e a sala muda só por `Revogar()` e `ReceberLeitura()`. A classe estática `Validacoes` saiu; cada regra ficou na sua entidade.
- `NivelCalor` em PascalCase (`Normal`, `Atencao`, `Cuidado`, `Perigo`), com conversor que mantém o JSON em minúsculas. O contrato com a API e o arquivo local da versão anterior não mudam.
- Um arquivo por classe, nas pastas `Dominio/`, `Servicos/` e `Telas/`.

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

[Não publicado]: https://github.com/sbaecker2005/termometro-da-sala/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/sbaecker2005/termometro-da-sala/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/sbaecker2005/termometro-da-sala/releases/tag/v0.1.0
