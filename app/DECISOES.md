# Decisões do app

O app é a parte do professor na CitizenTech Termômetro da Sala. A entrega de Mobile pede Expo, TypeScript estrito, quatro telas, câmera e a pasta `services/`. Cada escolha abaixo existe para cumprir isso sem inventar uma segunda arquitetura.

## Expo SDK 58 e expo-router

O SDK 58 é o estável atual e roda no Expo Go, que é o formato pedido (vídeo no celular, sem loja e sem build nativo). A navegação é o expo-router, o roteamento em arquivo que o próprio Expo instala hoje. As rotas são as telas: identificação, lista, detalhe e ocorrência. Não há WebView. React Navigation puro faria o mesmo, com mais código de registro de rotas; o expo-router já traz a pilha nativa.

TypeScript fica com `"strict": true`. Os modelos da API, os níveis e as respostas têm tipo. Não há `any` no código do app. `npx tsc --noEmit` é o critério da disciplina.

## Identificação no aparelho

O nome e a escola não são conta e não vão para a API. A regra (os dois preenchidos, já sem espaço nas pontas) fica em `domain/professor.ts`. `AsyncStorage` guarda os dois e a tela inicial lê isso ao abrir. Se já existe, o app segue para as salas. O botão Professor reabre o formulário. É a menor persistência que sobrevive a fechar o Expo Go. Se a gravação falha, a tela avisa e não sai do formulário. Quando o professor edita pelo botão, o app volta para a lista que já estava aberta, em vez de empilhar outra.

## API e modo simulado

A API está em `api/`, mas a demonstração não pode depender de rede na sala de aula. Por isso o modo simulado é uma escolha para o app funcionar sozinho no Expo Go, não um substituto da API. `app.config.ts` lê `EXPO_PUBLIC_API_URL` (padrão `http://localhost:5080`) e `EXPO_PUBLIC_API_MOCK` (padrão ligado). Com `EXPO_PUBLIC_API_MOCK=0`, `services/api.ts` chama as rotas da API: `GET /ambientes`, `GET /ambientes/{id}/previsao`, `GET /ambientes/{id}/ocorrencias` e `POST /ocorrencias` em multipart. A resposta é conferida campo a campo antes de entrar na tela: JSON inesperado vira a mensagem "O servidor respondeu num formato inesperado" com "Tentar de novo", e falha de rede vira "Não foi possível falar com o servidor", em vez do texto técnico do sistema. O `GET` de ocorrências trata 404 como lista vazia. No `GET` da previsão, 404 é "Sala não encontrada" e o 503 com `{"motivo":"sem-dados-para-prever"}` não é erro: a tela mostra a sala e avisa que não há dados para prever. Outro 503 continua sendo falha do servidor.

A API marca a sala que nunca recebeu leitura com `semLeitura: true` (e manda nível normal, índice 0 e data vazia). O app mostra "Sem leitura" no lugar do nível, do índice e da hora, para não exibir "Normal 0,0 °C".

O mock devolve cinco salas, uma de cada nível e uma sem leituras recentes. Cada chamada simulada espera cerca de 600 ms, para o estado "carregando" aparecer. As datas são calculadas a partir do relógio do aparelho (leituras de poucos minutos atrás, a sala 9 com leitura de 5 h atrás e a previsão para daqui a 2 h), então o vídeo mostra horários do dia da gravação. `EXPO_PUBLIC_API_MOCK=vazio` devolve a lista sem salas e `EXPO_PUBLIC_API_MOCK=erro` faz toda chamada falhar, para mostrar os outros dois estados da lista. A sala 3 vem com `demonstracao: true`, no mesmo exemplo do contrato (rua 34, delta 4, sala 38). O `POST` simulado devolve um id local (data e contador; o Hermes do Expo Go não tem `crypto.randomUUID`) e guarda descrição e horário em memória, para o detalhe da sala. A foto não fica guardada.

## Níveis

A tabela é a do contrato, numa função só (`nivelDeIndice`), testada nos limites: abaixo de 27 é normal, 27 é atenção, 32 é cuidado extremo, 41 é perigo. O número exato do limite de cima fica na faixa seguinte, senão 32 seria atenção e cuidado ao mesmo tempo. A cor do selo nunca aparece sozinha: o nome do nível vai escrito dentro dele. A ação sugerida é a da tabela (nenhuma, água e ventilação, pausa ou troca, tirar a turma). O selo e a apresentação recebem o tipo `Nivel`, não texto solto: um nível fora da tabela é recusado ao ler a resposta da API e nunca chega à tela.

Quando `semLeiturasRecentes` é verdadeiro, o detalhe não calcula delta e não mostra índice previsto da sala. Mostra a previsão da rua, o nível que a API mandou e o aviso "sala sem leituras recentes". Quando `ruaDisponivel` é falso, a API mantém a sala no índice da última leitura válida e manda `indiceRuaPrevisto` 0; o detalhe mostra o aviso de que a previsão usa a última leitura válida da sala e não mostra o índice da rua. A conferência da resposta fica em `domain/previsao.ts`, com teste. A etiqueta DEMONSTRAÇÃO só aparece se a previsão vier com `demonstracao`.

## Hora

A API manda UTC. A tela formata com `Intl` no fuso `America/Sao_Paulo`. Desde 2019 não há horário de verão, e o fuso do sistema já aplica UTC−3. Não há conta manual de três horas, que erraria no dia em que o aparelho estivesse noutra zona.

## Ocorrência e câmera

A validação mora em `domain/ocorrencia.ts` e não importa React. Descrição com 10 a 500 caracteres, já sem espaço nas pontas, e foto obrigatória. O rótulo do campo é "O que aconteceu". Não existe campo de aluno: a ocorrência é da sala. Os testes cobrem os limites 9, 10, 500 e 501, o recorte de espaço e a foto faltando.

A foto usa `expo-image-picker`, que abre a câmera do sistema e já vem no Expo Go. Qualidade 0,7 gera JPEG, que é o arquivo do `POST`, e o EXIF fica desligado para a foto não levar localização. Se a permissão é negada, a tela explica. "Tentar de novo" pede a permissão outra vez ou, se o sistema não deixa perguntar de novo, abre os ajustes do aparelho.

## Testes sem biblioteca nova

A validação e a conversão de nível rodam com `node --test` e a leitura de TypeScript que o Node já tem (`--experimental-strip-types`, a partir do 22.6). Jest exigiria preset, transformador e mais dependências para a mesma asserção.

## O que ficou de fora

Notificação quando a sala entra em cuidado extremo e a tela de leitura manual do termo-higrômetro estão na coluna "se sobrar tempo" do plano, não neste degrau. Rotas tipadas do Expo ficaram desligadas: elas geram arquivo na primeira execução do bundler e atrapalhariam o `tsc` limpo de quem só clonou o repositório. Os parâmetros de `router.push` e `useLocalSearchParams` usam o tipo `RotaParams` em `rotas.ts`.
