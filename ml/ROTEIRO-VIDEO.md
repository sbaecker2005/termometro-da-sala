# Vídeo de IA e Machine Learning — até 2 minutos

Meta: **1 min 50 s**. O modelo prevê o índice de calor da rua daqui a 2 h; a sala entra pelo delta medido pelo ESP32 e a previsão aparece no app. Grave o notebook já executado e, no trecho final, a sala 3 no app. Não mostre chave, segredo HMAC nem connection string.

Antes de gravar: notebook aberto nas seções 1, 4, 5 e 6 (previsão em uso e gráfico de previsto contra real); `ml/modelo.json` e `api/TermometroDaSala.Api/modelo/modelo.json` no mesmo conteúdo; **API ligada** e app apontando para ela (`EXPO_PUBLIC_API_MOCK=0 EXPO_PUBLIC_API_URL=http://IP_DO_COMPUTADOR:5080 npx expo start`), na sala 3. Não grave no modo simulado: nele a previsão é fixa no código do app e não prova que o número veio do modelo. Confira antes que `GET /ambientes/sala-03/previsao` responde com o `indiceRuaPrevisto` calculado pelo `modelo.json`.

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:20 | Nome do projeto e a frase do problema: a sala esquenta e a escola só reage quando alguém passa mal. Aponte o cidadão (professor e turma) e quem decide (coordenação, no desktop). | O modelo não mede a sala. Ele prevê a rua. O ESP32 mede a sala, e a API soma os dois. |
| 0:20–0:45 | Seção 1 do notebook: `dados/inmet_a701.csv`, estação A701 (Mirante de Santana), 2022–2025, e a conversão de UTC para São Paulo. Mostre o assert 15/01/2026 às 1700 UTC = 14h. | Dado público do INMET, não inventado. Sem essa conversão, o ciclo do dia sai 3 h errado e a previsão da sala erra. |
| 0:45–1:05 | Divisão por tempo: treino 2022–2023, validação 2024, teste 2025. Árvore de profundidade 8 e folha 30, escolhida pelo MAE de 2024, sem retreino no teste. | O teste de 2025 só entra na avaliação final. |
| 1:05–1:25 | Tabela do teste: árvore MAE 0,958 °C contra 1,442 °C do baseline "daqui a 2 h = agora", e o gráfico de previsto contra real da seção 6. Diga o limite: na rua o nível perigo não aparece, e o recall de cuidado é 14,6 %. | Vence no erro em graus. Não vence o alerta do nível mais alto na rua; dentro da sala quem empurra o índice é o delta. |
| 1:25–1:50 | `modelo.json` (`tipo: arvore`), a resposta de `GET /ambientes/sala-03/previsao` na API ligada e, ao lado, a sala 3 no app com o mesmo número: nível de agora, previsão de 2 h e a ação sugerida. | Esse JSON é o que a API carrega. A previsão da sala é a rua prevista mais o delta das últimas 3 h. Sem leituras recentes, a tela mostra só a rua. |
