# Termômetro da Sala

O mesmo sensor avisa calor no verão (escolas) e frio no inverno (dormitórios e abrigos).

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

App do professor. A coordenação usa o desktop; este app só acompanha a sala e registra ocorrência, sem dado de aluno.

## Equipe

| Nome completo | RM |
|---|---|
| Samuel Baecker | [preencher] |
| [preencher] | [preencher] |

Os mesmos nomes e RMs ficam no campo `authors` do `package.json`.

## Como rodar (Expo Go)

Requisitos: Node.js 22.6 ou mais novo (os testes usam `--experimental-strip-types`) e o aplicativo Expo Go no celular, na mesma rede do computador.

```bash
cd app
npm install
npx expo start
```

Escaneie o QR code com o Expo Go (Android) ou com a câmera (iOS). O modo simulado vem ligado: o app não precisa da API para mostrar salas, previsão e o registro de ocorrência.

Para apontar para a API de verdade:

```bash
EXPO_PUBLIC_API_MOCK=0 EXPO_PUBLIC_API_URL=http://IP_DO_COMPUTADOR:5080 npx expo start
```

No celular, `localhost` é o próprio aparelho. Use o IP da máquina onde a API está rodando.

### Forçar os estados da lista no modo simulado

O simulado espera cerca de 600 ms em cada chamada, então o "Carregando salas" aparece sempre. Para mostrar os outros dois estados:

```bash
EXPO_PUBLIC_API_MOCK=vazio npx expo start --clear   # lista sem salas
EXPO_PUBLIC_API_MOCK=erro npx expo start --clear    # erro com "Tentar de novo"
```

No PowerShell: `$env:EXPO_PUBLIC_API_MOCK='vazio'; npx expo start --clear`. Para voltar ao normal, feche o Expo, rode `Remove-Item Env:EXPO_PUBLIC_API_MOCK` (ou abra outro terminal) e suba de novo com `--clear`. As datas do simulado acompanham o relógio do aparelho.

## Telas

1. **Identificação.** Nome do professor e escola. Fica gravado no aparelho e volta ao reabrir. Pelo botão Professor, na lista, dá para alterar.
2. **Salas.** Nível (nome e cor), índice de calor e hora da última leitura, no horário de São Paulo. Mostra carregando, erro com "Tentar de novo" e a mensagem de lista vazia. Sala que ainda não recebeu leitura aparece como "Sem leitura".
3. **Sala.** Nível de agora e o previsto para daqui a 2 h, com a ação sugerida. Se a sala está sem leituras recentes, entra só a previsão da rua e o aviso. Se a previsão da rua não está disponível (`ruaDisponivel: false`), o aviso diz que a previsão usa a última leitura válida da sala. Se a API responde 503 `sem-dados-para-prever`, a sala aparece normalmente e o lugar da previsão explica que ainda não há dados para prever; 404 é sala não encontrada. Se a previsão é do modo demonstração, aparece a etiqueta DEMONSTRAÇÃO. Abaixo, as ocorrências da sala (no modo simulado, as registradas nesta sessão; na API, `GET /ambientes/{id}/ocorrencias`, e 404 vira lista vazia).
4. **Nova ocorrência.** O que aconteceu (10 a 500 caracteres) e uma foto pela câmera. O aviso pede para fotografar a sala ou o equipamento, sem pessoas. Se a permissão for negada, a tela explica e oferece tentar de novo.

## Testes

```bash
npm test
npx tsc --noEmit
```

O que é simulado: as salas, as previsões e o protocolo da ocorrência quando `EXPO_PUBLIC_API_MOCK` não é `0` (`vazio` e `erro` são variações do simulado). Nenhum dado de aluno é pedido nem guardado.
