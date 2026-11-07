# Termômetro da Sala — arquitetura

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

O documento descritivo está em [DOCUMENTO.md](DOCUMENTO.md), com as Fases A a F do TOGAF. O modelo ArchiMate está em `termometro-da-sala.archimate` (Archi) e em `termometro-da-sala.xml` (Open Exchange 3.1), com 111 elementos documentados, 190 relações e seis visões: Motivação, Negócio, Aplicação, Tecnologia, Implementação e migração e "Visão em quatro camadas", o resumo das quatro. As imagens geradas pelo Archi estão em `visao-motivacao.png`, `visao-negocio.png`, `visao-aplicacao.png`, `visao-tecnologia.png`, `visao-implementacao.png` e `diagrama-archi.png`. O pitch está em [PITCH.md](PITCH.md).

Conferência do modelo: `python arquitetura/conferir_modelo.py` (XML bem formado, ids únicos, relações e visões apontando para o que existe, documentação em todo elemento, `.xml` igual ao `.archimate` e, com o Archi instalado, cada relação permitida pelo ArchiMate). `python arquitetura/conferir_modelo.py --autoteste` confere que o script acusa cada defeito plantado.
