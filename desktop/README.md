# Termômetro da Sala — desktop da coordenação

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | [preencher] |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Problema e pessoa a entrevistar

O terminal permite à coordenação acompanhar o calor nas salas, registrar providências e revogar sensores.
Entrevistar um(a) professor(a) ou coordenador(a) de uma escola pública próxima, sem coletar dados de alunos.
Pessoa real: **[preencher nome, escola e data após a entrevista]**. A entrevista ainda precisa ser realizada; não há depoimento atribuído a alguém.

Equipe: **Termômetro da Sala**. Integrante: Samuel Baecker — RM [preencher]. Demais integrantes: [preencher, se houver].

## Executar (Windows e SDK .NET 8)

Na raiz do repositório, PowerShell:

```powershell
& 'C:/Program Files/dotnet/dotnet.exe' restore desktop/TermometroDaSala.Desktop/TermometroDaSala.Desktop.csproj --source desktop
& 'C:/Program Files/dotnet/dotnet.exe' build desktop/TermometroDaSala.Desktop/TermometroDaSala.Desktop.csproj --no-restore
& 'C:/Program Files/dotnet/dotnet.exe' run --project desktop/TermometroDaSala.Desktop --no-build
```

O restore usa uma pasta local como única fonte, sem rede. Requer o SDK e o targeting pack Windows Desktop já disponíveis.
Configure `ApiUrl` em `TermometroDaSala.Desktop/appsettings.json` e compile novamente para copiar a configuração para a saída.
O padrão é `http://localhost:5080/`. Não coloque credenciais nesse arquivo. O cliente usa HttpClient com timeout de 5 segundos.

## Uso

- **Atualizar** recarrega salas, previsões e ações; ocorre também a cada 30 segundos. O nível sempre tem texto e cor. Avisos identificam leitura antiga, previsão da rua e DEMONSTRAÇÃO.
- **Cadastrar sala** exige `sala-NN`, nome, andar entre 0 e 20 e face norte/sul/leste/oeste. Há opção de ventilador e mensagens por campo via ErrorProvider.
- Selecione uma sala, escolha o nível e descreva a ação (5–300 caracteres), depois use **Registrar ação**. A hora é registrada em UTC e exibida em São Paulo.
- **Exportar dia** gera CSV UTF-8 com BOM, separador ponto e vírgula, aspas escapadas e proteção contra fórmulas de planilha. O dia usa UTC−3, independentemente do fuso do computador.
- **Revogar sensor** pede confirmação, com "Não" como padrão. Na API, depende de `dispositivoId` informado no cadastro retornado. Nunca é deduzido a partir do ID da sala.
- Teclado: Tab percorre os controles; Alt+A atualiza, Alt+C cadastra, Alt+V revoga, Alt+E exporta, Alt+N escolhe nível, Alt+D foca descrição e Alt+R registra. No cadastro, Enter salva e Esc cancela.

## API e modo local

O cliente usa o formato de `app/services/api.ts` para `GET /ambientes` e `GET /ambientes/{id}/previsao`.
Níveis JSON: `normal`, `atencao`, `cuidado`, `perigo` (texto exibido: Normal, Atenção, Cuidado extremo, Perigo).
O contrato das operações administrativas, a confirmar com a API em desenvolvimento, é:

| Rota | Entrada / resposta |
|---|---|
| `POST /ambientes` | `{id, nome, andar, faceSol, temVentilador, tipo: "sala"}` |
| `POST /acoes` | `{ambienteId, nivel, descricao, registradoEm}` (hora ISO 8601 UTC) |
| `GET /acoes?dia=yyyy-MM-dd` | Array de `{id, ambienteId, nivel, descricao, registradoEm}`; dia de São Paulo |
| `POST /dispositivos/{dispositivoId}/revogar` | Corpo `{}`; sucesso HTTP 2xx |

`GET /ambientes` pode acrescentar `temVentilador`, `dispositivoId` e `sensorRevogado` aos campos do app.
Sem `dispositivoId`, a revogação remota apresenta uma mensagem e não envia uma requisição para um sensor presumido.

Falha de conexão, timeout ou HTTP 5xx ao listar salas ativa o aviso **MODO LOCAL**.
HTTP 4xx e JSON inválido são erros apresentados ao operador. Uma falha isolada de previsão aparece na linha da sala.
Gravações não são repetidas automaticamente em outro serviço: após falha de conexão, confira se o servidor recebeu a operação antes de tentar novamente.

O arquivo local é `%LOCALAPPDATA%/TermometroDaSala/dados.json`, separado dos dados da API.
Começa vazio: cadastre salas para demonstrar sem API. As ações e a marcação de revogação persistem após fechar o programa.
Sem sensor/API, os níveis aparecem como **Sem leitura / Sem previsão**; não há medição ou previsão artificial.
Uma revogação local **não bloqueia o sensor no servidor**. A cada atualização o cliente tenta novamente a API e sinaliza a troca de origem.
Não há sincronização automática: cadastros e ações locais continuam no arquivo local quando a API retorna.
Use uma instância por computador para evitar gravações concorrentes no arquivo local.
Arquivo inválido gera erro e é preservado; faça uma cópia antes de corrigir ou recuperar o JSON.

As leituras do Wokwi e do modo demonstração são simuladas. O desktop apenas exibe o marcador recebido da API.
Não calcula delta térmico: sem leituras recentes, exibe a previsão da rua com aviso, conforme a API.
O delta pressupõe que a diferença sala–rua das últimas 3 h persiste por 2 h; pode superestimar ao fim da tarde.

## Tecnologias e verificação

C# / .NET 8, Windows Forms, System.Text.Json, HttpClient e arquivo JSON. Nenhum pacote externo no executável.
POO: Sala, Leitura, Previsao, Acao, NivelCalor e IServicoSalas com implementações HTTP e local.

Os pacotes xUnit 2.5.3 e Microsoft.NET.Test.Sdk 17.8.0 foram restaurados do cache, usando somente fonte local:

```powershell
& 'C:/Program Files/dotnet/dotnet.exe' restore desktop/TermometroDaSala.Desktop.Tests/TermometroDaSala.Desktop.Tests.csproj --source desktop
& 'C:/Program Files/dotnet/dotnet.exe' test desktop/TermometroDaSala.Desktop.Tests/TermometroDaSala.Desktop.Tests.csproj --no-restore
& 'C:/Program Files/dotnet/dotnet.exe' run --project desktop/TermometroDaSala.Desktop --no-build -- --autoteste
```

Em outra máquina sem esses pacotes no cache, use o autoteste sem dependências; ele retorna 0 em sucesso e 1 em falha.
Os quatro casos verificam limites dos campos, CSV/relógio, persistência/arquivo inválido e contrato HTTP com handler em memória (sem rede).
A integração com a API real e a navegação visual/teclado precisam de ensaio no Windows antes da apresentação.

## Entrega

Versão inicial `v0.1.0`, descrita em [CHANGELOG.md](CHANGELOG.md). Entregar ZIP sem `bin/obj`, com [integrantes.txt](integrantes.txt), RM e link preenchidos. Abrir pull request para revisão da versão.

## Chave da coordenação

As rotas de escrita da API (cadastro de sala, ação, revogação) exigem a chave da coordenação. Defina a variável de ambiente `TERMOMETRO_CHAVE_COORDENACAO` com o mesmo valor configurado na API antes de abrir o terminal; sem ela, essas ações voltam 401 e o terminal avisa.
