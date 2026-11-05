# Termômetro da Sala

Global Solution 2026.2 (CitizenTech) — Engenharia de Software, 3º ano, FIAP.

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

## Estrutura

| Pasta | Parte | Disciplina |
|---|---|---|
| `api/` | API REST, banco e painel web | SOA e Web Services |
| `desktop/` | Terminal da coordenação | C# Windows Forms |
| `app/` | App do professor | Mobile e IoT |
| `esp32/` | Sensor da porta da sala (Wokwi) | Physical Computing |
| `ml/` | Notebook do modelo | IA e Machine Learning |
| `infra/` | Windows Server, IIS, AD, GPO | Operating Systems |
| `seguranca/` | Gestão de riscos | Cybersecurity |
| `arquitetura/` | ArchiMate e pitch | Testing (TOGAF) |
| `demo/` | Gerador de leituras do modo demonstração | — |
| `docs/` | Plano e briefing | — |

O contrato entre as partes está em [CONTRATO.md](CONTRATO.md). O plano completo está em [docs/PLANO.md](docs/PLANO.md).

## Dados

As leituras do sensor no Wokwi, as leituras do modo demonstração e as ocorrências são simuladas. Os dados meteorológicos são públicos, do INMET (estação A701, Mirante de Santana). Nenhum dado de aluno é coletado.
