# Termômetro da Sala — desktop da coordenação

## Problema e pessoa real beneficiada

Nas tardes quentes, a coordenação não sabe quais salas passaram do limite seguro de calor. O terminal mostra o nível de cada sala, a previsão para as próximas 2 horas e as ações tomadas no dia, e permite revogar um sensor suspeito.

Pessoa real: **Monalisa Daniela Pereira Amaral, coordenadora do CRAS Santo Amaro (São Paulo)**. Data da entrevista: **07/11/2026**, por escrito, com autorização para citar o nome e as frases. O CRAS entra como referência de ambiente público; a escola continua sendo o cenário principal.

A fidelidade do relato e a autorização de uso do nome e das falas foram confirmadas em **17/11/2026**. Magister e Salgueiro são candidatas aos testes; ainda não há piloto realizado nem autorização dessas escolas.

Relato: No CRAS Santo Amaro, em São Paulo, passam no mínimo 150 pessoas por dia, porque ali também funciona um posto do Cadastro Único. A sala dos técnicos recebe sol à tarde, e o calor pesa de janeiro a março; a unidade conta só com janelas e ventiladores e nunca teve climatização, por limitação orçamentária. A coordenadora gostaria de receber o aviso no celular, para ligar ventiladores, abrir janelas e providenciar água gelada e leques, e de ter o histórico de temperatura para levar à Supervisão de Assistência Social, junto com o pedido de manutenção que já fez.

A entrevista não coleta dados de alunos. Os números do CRAS não substituem os da escola do piloto, ainda não existem.

Equipe: **Termômetro da Sala**.

| Nome completo | RM |
|---|---|
| Samuel David Baecker | 559269 |
| Lucas de Almeida Villar | 560005 |
| Guilherme Augusto Caseiro | 559765 |

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo; as escolas Magister e Salgueiro são candidatas ao piloto. Unidade, bairro, contato e autorização dos testes ainda não foram definidos |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Mapa térmico

Selecione uma sala e clique em **Mapa térmico**, ou dê dois cliques na linha. A janela mostra a planta em escala com origem no canto inferior esquerdo, grade interpolada, sensores em círculos e cadeiras em quadrados, legenda colorida fixa e lista rolável com todos os pontos e precauções. As cadeiras recebem códigos C01, C02… e os sensores S1, S2… na planta, lista e PNG. A demonstração API tem C01–C20 e S1–S4; a local tem C01–C20 e S1–S2. Pontos na mesma posição mantêm seus códigos individuais, apresentados juntos na planta. Azul é frio, verde normal, amarelo atenção, laranja cuidado extremo e vermelho perigo; cinza é sem leitura. O tamanho cresce com o desvio absoluto da média, limitado visualmente a 8 °C e com mínimo para manter o código legível. As demonstrações API/local têm 20 cadeiras em 5 fileiras de 4; o cenário API `madrugada` permite conferir a região azul de frio.

Use **Agora / Previsão +1 h** e **Exportar PNG** para salvar a planta, período/dimensões, legenda e lista completa de pontos/precauções, inclusive o trecho que exige rolagem na tela. No modo local, os mapas são exemplos identificados como **DEMONSTRAÇÃO**, inclusive em salas cadastradas: não representam sensores reais. No modo API, +1 h consulta `/ambientes/{id}/mapa?futuro=true`: previsão do ambiente aplicada aos pontos, usando metade da variação do índice prevista para +2 h como deslocamento da temperatura e do índice de cada sensor. A lista de precauções explica essa aproximação, que não tem modelo de temperatura por ponto. A previsão de +2 h do ambiente continua separada.

O mesmo sensor avisa calor no verão (escolas) e frio no inverno (dormitórios e abrigos).

## Abrir no Visual Studio 2022

1. Instale o Visual Studio 2022 (17.8 ou mais novo) com a carga de trabalho **Desenvolvimento para desktop com .NET**, que traz o .NET 8 e o Windows Forms.
2. Abra `TermometroDaSala.Desktop.sln`, nesta pasta.
3. No **Gerenciador de Soluções**, clique com o botão direito em `TermometroDaSala.Desktop` e escolha **Definir como Projeto de Inicialização**.
4. Aperte **F5**. Os testes ficam em **Teste > Gerenciador de Testes > Executar Todos os Testes**.
5. Para ver os controles no Designer, dê dois cliques em `Telas/TelaCoordenacao.cs` (tela principal) ou em `Telas/CadastroSala.cs` (cadastro). Os controles e eventos ficam no `.Designer.cs`; o tema e a organização visual final são aplicados em tempo de execução, incluindo `TelaCoordenacao.Visual.cs` e `TemaVisual.cs`.

## Executar pela linha de comando (Windows e SDK .NET 8)

Se o SDK ainda não estiver instalado, execute `ferramentas/preparar-windows.ps1` a partir da raiz do repositório. Ele verifica o SDK 8, instala o pacote oficial pelo winget somente quando necessário e confere o Windows Desktop Runtime 8. O roteiro conjunto de API, banco, previsão e desktop está em [FECHAMENTO-ML-SOA-CSHARP.md](../docs/FECHAMENTO-ML-SOA-CSHARP.md).

Dentro da pasta `desktop`, no PowerShell:

```powershell
dotnet build TermometroDaSala.Desktop.sln
dotnet run --project TermometroDaSala.Desktop
```

A primeira compilação baixa do NuGet os pacotes de teste (xUnit). O executável não usa pacote externo.

Configure `ApiUrl` em `TermometroDaSala.Desktop/appsettings.json` e compile novamente para copiar a configuração para a saída.
O padrão é `http://localhost:5080/`. Não coloque credenciais nesse arquivo. O cliente usa HttpClient com timeout de 5 segundos.

## Uso

- O cabeçalho azul-escuro identifica o sistema; a faixa de origem informa API, MODO LOCAL ou DEMONSTRAÇÃO. Cartões claros resumem salas, alertas atuais e ações do dia. Sala sem leitura não conta como alerta. Botões turquesa destacam as ações principais; a janela menor permite rolagem. A janela e o executável usam o ícone do termômetro (`termometro.ico`).
- **Atualizar** recarrega salas, previsões e ações; ocorre também a cada 30 segundos. O nível sempre tem texto e cor. Avisos identificam leitura antiga, previsão da rua e DEMONSTRAÇÃO.
- **Cadastrar sala** exige `sala-NN`, nome, andar entre 0 e 20 e face norte/sul/leste/oeste. Há opção de ventilador e mensagens por campo via ErrorProvider.
- Selecione uma sala, escolha o nível e descreva a ação (5–300 caracteres), depois use **Registrar ação**. A hora aparece no horário de São Paulo.
- **Exportar dia** gera CSV UTF-8 com BOM, separador ponto e vírgula, aspas escapadas e proteção contra fórmulas de planilha. O dia usa UTC−3, independentemente do fuso do computador.
- **Revogar sensor** pede confirmação, com "Não" como padrão. Na API, depende do `dispositivoId` que vem no cadastro da sala. Nunca é deduzido a partir do ID da sala.
- Teclado: Tab percorre os controles; Alt+A atualiza, Alt+C cadastra, Alt+V revoga, Alt+E exporta, Alt+N escolhe nível, Alt+D foca descrição e Alt+R registra. No cadastro, Enter salva e Esc cancela.

### Legenda de cores

O nível aparece sempre por texto e cor, para não depender só da cor. A mesma legenda fica na tela, logo acima da grade, e o autoteste confere que as cores dela são as da grade.

| Texto na grade | Cor da célula | Significado |
|---|---|---|
| Frio | azul claro | Agasalhar e oferecer cobertor |
| Normal | verde claro | Índice de calor seguro |
| Atenção | amarelo | Calor subindo: acompanhar a sala |
| Cuidado extremo | laranja claro | Agir: ventilar, oferecer água, pausar atividade física |
| Perigo | vermelho claro | Tirar a turma da sala |
| Sem leitura / Sem previsão | branco | A sala ainda não tem medição do sensor, ou a previsão não veio |

### Telas

Tela principal sem API e sem dados neste computador (modo DEMONSTRAÇÃO, uma sala de exemplo por nível):

![Tela da coordenação em modo demonstração](docs/tela-demonstracao.png)

Tela principal com a API e o SQL Server ligados:

![Tela da coordenação com a API ligada](docs/modelo-geral-v1/desktop-sql-janela.jpg)

Mesma tela maximizada:

![Tela da coordenação maximizada](docs/modelo-geral-v1/desktop-sql-maximizado.jpg)

Aviso MODO LOCAL — DEMONSTRAÇÃO, com a API desligada:

![Tela da coordenação no modo local](docs/modelo-geral-v1/desktop-modo-local.jpg)

Mapa térmico de uma sala (Agora), na tela e exportado em PNG:

![Mapa térmico na tela](docs/modelo-geral-v1/mapa-agora-tela.jpg)

![Mapa térmico exportado em PNG](docs/modelo-geral-v1/mapa-agora-exportado.png)

## API e modo local

O cliente segue os contratos de `api/TermometroDaSala.Api/Contratos`. Níveis JSON: `frio`, `normal`, `atencao`, `cuidado`, `perigo` (texto exibido: Frio, Normal, Atenção, Cuidado extremo, Perigo).

| Rota | Entrada / resposta |
|---|---|
| `GET /ambientes` | Array de `{id, nome, andar, faceSol, nivelAtual, indiceAtual, ultimaLeituraEm, semLeitura, temVentilador, dispositivoId, sensorRevogado}` |
| `GET /ambientes/{id}/previsao` | `{ambienteId, previstoPara, indiceRuaPrevisto, deltaSala, indiceSalaPrevisto, nivelPrevisto, semLeiturasRecentes, demonstracao, ruaDisponivel}`; 404 sala inexistente; 503 `{motivo: "sem-dados-para-prever"}` |
| `POST /ambientes` | `{id, nome, andar, faceSol, temVentilador, tipo: "sala"}` |
| `POST /acoes` | `{ambienteId, nivel, descricao}`; a API grava a hora do servidor e devolve `{id}` numérico |
| `GET /acoes?dia=yyyy-MM-dd` | Array de `{id, ambienteId, nivel, descricao, registradoEm}`, com `id` numérico; dia de São Paulo |
| `POST /dispositivos/{dispositivoId}/revogar` | Corpo `{}`; sucesso HTTP 2xx |

Sala que nunca foi medida chega com `semLeitura: true`, `ultimaLeituraEm: ""`, `nivelAtual: "normal"` e `indiceAtual: 0`. O terminal mostra **Sem leitura**, e não Normal.
Sem `dispositivoId`, a revogação remota apresenta uma mensagem e não envia uma requisição para um sensor presumido.

Falha de conexão, timeout ou HTTP 5xx ao listar salas ativa o aviso **MODO LOCAL**.
HTTP 4xx e JSON inválido são erros apresentados ao operador. Uma falha isolada de previsão aparece na linha da sala. O 503 `sem-dados-para-prever` não é falha: a linha mostra "Sem previsão" e o aviso "Sem dados para prever". O 404 aparece como "Sala não encontrada na API". Com `ruaDisponivel: false`, o aviso diz que a previsão usa a última leitura válida da sala.
Gravações não são repetidas automaticamente em outro serviço: após falha de conexão, confira se o servidor recebeu a operação antes de tentar novamente.

O arquivo local é `%LOCALAPPDATA%/TermometroDaSala/dados.json`, separado dos dados da API.
As ações (numeradas em sequência) e a marcação de revogação persistem após fechar o programa.

**DEMONSTRAÇÃO.** Enquanto esse arquivo não existe (por exemplo, quem abre o ZIP sem a API), o modo local mostra cinco salas de exemplo, uma por nível, com previsão e duas ações. A faixa de situação diz "MODO LOCAL — DEMONSTRAÇÃO", cada linha traz o aviso DEMONSTRAÇÃO, os nomes terminam em "(exemplo)" e as duas ações começam com "DEMONSTRAÇÃO:", inclusive no CSV exportado. Os exemplos nunca são gravados: registrar ação ou revogar sensor numa sala de exemplo é recusado, e o primeiro cadastro de sala cria o arquivo só com a sala cadastrada. A partir daí, sem sensor nem API, as salas aparecem como **Sem leitura / Sem previsão**; fora da demonstração não há medição ou previsão artificial.
Uma revogação local **não bloqueia o sensor no servidor**. A cada atualização o cliente tenta novamente a API e sinaliza a troca de origem.
Não há sincronização automática: cadastros e ações locais continuam no arquivo local quando a API retorna.
Use uma instância por computador para evitar gravações concorrentes no arquivo local.
Arquivo inválido gera erro e é preservado; faça uma cópia antes de corrigir ou recuperar o JSON.

As leituras do Wokwi e do modo demonstração da API são simuladas. Com a API ligada, o desktop apenas exibe o marcador recebido dela e não calcula delta térmico: sem leituras recentes, exibe a previsão da rua com aviso, conforme a API.
Sem API e sem `dados.json`, os exemplos da DEMONSTRAÇÃO (acima) trazem leitura, previsão e delta fixos, só ilustrativos; nada é calculado a partir de sensor.
O delta pressupõe que a diferença sala–rua das últimas 3 h persiste por 2 h; pode superestimar ao fim da tarde.

## Chave da coordenação

As rotas de escrita da API (cadastro de sala, ação, revogação) exigem a chave da coordenação. Defina a variável de ambiente `TERMOMETRO_CHAVE_COORDENACAO` com o mesmo valor configurado na API antes de abrir o terminal; sem ela, essas ações voltam 401 e o terminal avisa.

## Organização do código

| Pasta | Conteúdo |
|---|---|
| `Dominio/` | Uma classe por arquivo: `Sala`, `Acao`, `Previsao`, `Leitura`, o enum `NivelCalor` (com o texto de cada nível), `HorarioEscola` e `CadastroInvalidoException` |
| `Servicos/` | `IServicoSalas` e as duas implementações (`ServicoSalasApi` e `ServicoSalasLocal`), `DadosLocais`, `DadosDemonstracao`, `ExportacaoCsv`, `Configuracao` e os conversores de JSON |
| `Telas/` | `TelaCoordenacao` e `CadastroSala` com `.Designer.cs`, composição em `TelaCoordenacao.Visual.cs`, `TelaMapa` e tema compartilhado em `TemaVisual` |

O domínio é encapsulado: as propriedades têm `private set`, então só a própria classe muda o seu estado.

- `Sala.Criar(...)` valida antes de criar e, se algum campo estiver errado, lança `CadastroInvalidoException` com a mensagem de cada campo, que o cadastro mostra no ErrorProvider. A sala só muda por `Revogar()` e `ReceberLeitura(...)`.
- `Acao` tem um construtor que valida sala, nível e descrição (5 a 300 caracteres, sem espaços nas pontas). O número da ação vem da API ou do modo local.
- A sala que chega da API ou do arquivo passa por `Problemas()` antes de aparecer na tela.
- O enum segue a convenção do C# (`Normal`, `Atencao`, `Cuidado`, `Perigo`, `Frio`). O conversor `NivelCalorJson` grava em minúsculas, como a API, e por isso o arquivo local da versão anterior continua abrindo.

## Diagrama de classes

```mermaid
classDiagram
    direction LR
    class NivelCalor {
        <<enumeration>>
        Normal
        Atencao
        Cuidado
        Perigo
          Frio
    }
    class Sala {
        +string Id
        +string Nome
        +int Andar
        +string FaceSol
        +bool TemVentilador
        +string DispositivoId
        +bool SensorRevogado
        +bool SemLeitura
        +bool Demonstracao
        +Leitura Leitura
        +Criar(id, nome, andar, faceSol, temVentilador)$ Sala
        +Validar(id, nome, andar, faceSol)$ erros
        +Problemas() erros
        +Revogar()
        +ReceberLeitura(leitura)
    }
    class Leitura {
        <<record>>
        +double IndiceCalor
        +NivelCalor Nivel
        +DateTimeOffset MedidoEm
    }
    class Previsao {
        +string AmbienteId
        +DateTimeOffset PrevistoPara
        +NivelCalor NivelPrevisto
        +bool SemLeiturasRecentes
        +bool Demonstracao
        +bool RuaDisponivel
        +Valida(ambienteId) bool
    }
    class Acao {
        +long Id
        +string AmbienteId
        +NivelCalor Nivel
        +string Descricao
        +DateTimeOffset RegistradoEm
        +Acao(ambienteId, nivel, descricao, registradoEm)
        +ValidarDescricao(texto)$ string
        +Conferir()
    }
    class CadastroInvalidoException {
        +Erros por campo
    }
    class IServicoSalas {
        <<interface>>
        +Listar() List~Sala~
        +Prever(id) Previsao
        +Cadastrar(sala)
        +Registrar(acao)
        +AcoesDoDia(dia) List~Acao~
        +Revogar(sala)
    }
    class ServicoSalasApi
    class ServicoSalasLocal
    class DadosDemonstracao {
        <<static>>
    }
    class ExportacaoCsv {
        <<static>>
    }
    class TelaCoordenacao
    class CadastroSala

    Sala --> Leitura : leitura atual
    Sala ..> CadastroInvalidoException : lança
    Leitura --> NivelCalor
    Previsao --> NivelCalor
    Acao --> NivelCalor
    IServicoSalas <|.. ServicoSalasApi
    IServicoSalas <|.. ServicoSalasLocal
    ServicoSalasLocal ..> DadosDemonstracao : sem arquivo
    TelaCoordenacao --> IServicoSalas : API e local
    TelaCoordenacao --> CadastroSala : abre
    TelaCoordenacao ..> ExportacaoCsv
    CadastroSala ..> Sala : Criar
```

`ServicoSalasApi` fala com a API por HTTP e `ServicoSalasLocal` grava no arquivo JSON. A tela usa só a interface `IServicoSalas`, então troca de origem sem mudar o resto do código.

## Tecnologias e verificação

C# / .NET 8, Windows Forms, System.Text.Json, HttpClient e arquivo JSON. Nenhum pacote externo no executável. Testes com xUnit 2.5.3 e Microsoft.NET.Test.Sdk 17.8.0.

```powershell
dotnet test TermometroDaSala.Desktop.sln
dotnet run --project TermometroDaSala.Desktop -- --autoteste
```

Há também quatro testes de integração do cliente desktop em `api/TermometroDaSala.Api.Tests/ClienteDesktopTestes.cs`: eles compilam o mesmo domínio e os mesmos serviços HTTP do executável e os ligam aos controllers da API com SQLite em memória. Execute da raiz `dotnet test api/TermometroDaSala.sln --filter FullyQualifiedName~ClienteDesktopTestes`. Esses testes rodam no Linux e no Windows; não carregam o Windows Forms nem substituem os 22 testes Windows e o ensaio das telas.

O autoteste não depende de pacote e retorna 0 em sucesso e 1 em falha. Os seis casos verificam limites dos campos e o encapsulamento do domínio, CSV/relógio, persistência/arquivo inválido, o contrato HTTP com um handler em memória (sem rede), a demonstração do modo local (que nunca é gravada) e as telas montadas pelo Designer, com a legenda nas mesmas cores da grade.
As respostas usadas no teste de contrato são as que a API devolve: sala com leitura, sala sem leitura (`ultimaLeituraEm: ""`), previsão da rua, previsão sem a rua (`ruaDisponivel: false`), 503 `sem-dados-para-prever`, 503 sem esse motivo, 404 e ação com `id` numérico.

Em 19/11/2026, com a correção das ações de exemplo, passaram **22 testes e 6 verificações do autoteste**. Em 18/11/2026 (reconferido) e em 17/11/2026, após o novo visual, passaram **19 testes Windows e 6 verificações do autoteste**, com saída 0, também na cópia extraída do ZIP final. A interface foi conferida maximizada e em 958 × 707 com rolagem, cadastro, mapas Agora/+1 h/frio e dois PNGs completos. Cadastro e ação locais persistiram após fechar e reabrir o programa com a API desligada. Evidências e limites estão em [VALIDACAO-WINDOWS-2026-11-17.md](docs/VALIDACAO-WINDOWS-2026-11-17.md). DPI ampliado ainda não foi exercitado.

## Versões

O projeto segue versionamento semântico. O histórico completo está no [CHANGELOG.md](CHANGELOG.md).

| Versão | Data | Resumo |
|---|---|---|
| `v0.3.0` | 2026-11-19 | Novo visual, `TelaCoordenacao` com Designer, domínio encapsulado, modo DEMONSTRAÇÃO, mapa térmico de 20 cadeiras com legenda e exportação PNG; ações de exemplo sempre no dia da escola |
| `v0.2.0` | 2026-11-05 | Contrato com a API real (sala sem leitura e id numérico da ação), solução do Visual Studio e README |
| `v0.1.0` | 2026-11-04 | Primeira versão do terminal da coordenação |

Pull request da v0.2.0: [Desktop v0.2.0](https://github.com/sbaecker2005/termometro-da-sala/pull/1). Tags: [v0.1.0](https://github.com/sbaecker2005/termometro-da-sala/releases/tag/v0.1.0) e [v0.2.0](https://github.com/sbaecker2005/termometro-da-sala/releases/tag/v0.2.0).

## Entrega

ZIP desta pasta sem `bin/` e `obj/`, com o [integrantes.txt](integrantes.txt) (RM, nome completo e link do repositório).
