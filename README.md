# Termômetro da Sala

## Mapa térmico

O desktop e o detalhe do ambiente no app mostram a planta em escala, a grade térmica interpolada e os pontos de sensores e lugares. Azul indica frio, verde normal, amarelo atenção e vermelho cuidado extremo ou perigo; cinza indica ausência de leitura. O tamanho da bola acompanha o desvio absoluto da temperatura em relação à média dos sensores, com limite visual de 8 °C. Nome, temperatura e nível também aparecem em texto; as precauções ficam em lista.

Na coordenação, selecione uma sala e abra **Mapa térmico** (ou dê dois cliques na sala); **Exportar PNG** salva a tela para os prints. No app, abra o ambiente e toque em um ponto, ou use a lista acessível. O seletor oferece **Agora / Previsão +1 h**. No modo local/simulado, o mapa e a previsão usam valores explicitamente marcados como demonstração. No modo API, +1 h soma à temperatura e ao índice de cada sensor recente metade da diferença entre o índice previsto do ambiente em +2 h e a média dos índices atuais desses sensores. É uma aproximação linear, sem modelo de temperatura por ponto; sensores sem leitura continuam sem valores. O aviso aparece nas precauções; a previsão de +2 h do ambiente continua na seção própria.

O mesmo sensor avisa calor no verão (escolas) e frio no inverno (dormitórios e abrigos).

Global Solution 2026.2 (CitizenTech) — Engenharia de Software, 3º ano, FIAP.

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo; Magister e Salgueiro são candidatas ao piloto, indicadas por Samuel em 17/11/2026. Unidade, bairro e autorização a confirmar; a entrevista foi no CRAS Santo Amaro |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Equipe

| Nome completo | RM |
|---|---|
| Samuel David Baecker | 559269 |
| Lucas de Almeida Villar | 560005 |
| Guilherme Augusto Caseiro | 559765 |

## Por disciplina

Cada disciplina tem a sua pasta. O que enviar em cada uma está em [ENTREGA.md](ENTREGA.md).

| Disciplina | Pasta | Parte | Como rodar ou conferir |
|---|---|---|---|
| SOA e Web Services | [`api/`](api/) | API REST, banco e painel web | `dotnet test .\api\TermometroDaSala.sln` |
| C# Windows Forms | [`desktop/`](desktop/) | Terminal da coordenação | `dotnet test .\desktop\TermometroDaSala.Desktop.sln` |
| Mobile e IoT | [`app/`](app/) | App do professor | `npm --prefix .\app ci`, depois `npm --prefix .\app test` |
| IA e Machine Learning | [`ml/`](ml/) | Notebook e modelo exportado para a API | `python -m pytest ml` |
| Physical Computing | [`esp32/`](esp32/) | Sensor da porta da sala (Wokwi) | `python esp32\conferir_circuito.py` e o projeto no Wokwi |
| Operating Systems | [`infra/`](infra/) | Windows Server, IIS, AD, GPO | scripts `01` a `10` na VM, conforme `infra/README.md` |
| Cybersecurity | [`seguranca/`](seguranca/) | Gestão de riscos | relatório e anexo; cenários em `seguranca/cenarios-ataque.ps1` |
| Testing (TOGAF) | [`arquitetura/`](arquitetura/) | ArchiMate, documento e pitch | `python arquitetura\conferir_modelo.py` |

Pastas comuns: `demo/` (leituras simuladas do modo demonstração), `ferramentas/` (scripts de apoio) e `docs/` (plano, briefing e resultados de testes).

## Rodar o projeto inteiro

Num Windows com .NET SDK 8 e SQL Server Express, na raiz do repositório:

```powershell
powershell -ExecutionPolicy Bypass -File .\rodar-projeto.ps1        # API, previsão, mapa, desktop e painel web
powershell -ExecutionPolicy Bypass -File .\rodar-projeto.ps1 -App   # o mesmo, mais o app do professor no Expo
```

A API sobe em `http://localhost:5080` no modo demonstração, com um banco novo e dados simulados.

O contrato entre as partes está em [CONTRATO.md](CONTRATO.md). O plano completo está em [docs/PLANO.md](docs/PLANO.md).

A referência de 18 °C como mínimo para ambiente interno é a recomendação da OMS para moradias. O alerta usa o índice: abaixo de 18 °C é Frio; de 18 até menos de 27 °C é Normal. As faixas de calor continuam iguais.

## Dados

As leituras do sensor no Wokwi, as leituras do modo demonstração e as ocorrências são simuladas. Os dados meteorológicos são públicos, do INMET (estação A701, Mirante de Santana). Nenhum dado de aluno é coletado.
