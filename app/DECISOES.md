# Decisões do app

O app é a parte do professor na CitizenTech Termômetro da Sala. A entrega de Mobile pede Expo, TypeScript estrito, quatro telas, câmera e a pasta `services/`. Cada escolha abaixo existe para cumprir isso sem inventar uma segunda arquitetura.

## Expo SDK 58 e expo-router

O SDK 58 é o estável atual e roda no Expo Go, que é o formato pedido (vídeo no celular, sem loja e sem build nativo). A navegação é o expo-router, o roteamento em arquivo que o próprio Expo instala hoje. As rotas são as telas: identificação, lista, detalhe e ocorrência. Não há WebView. React Navigation puro faria o mesmo, com mais código de registro de rotas; o expo-router já traz a pilha nativa.

TypeScript fica com `"strict": true`. Os modelos da API, os níveis e as respostas têm tipo. Não há `any` no código do app. `npx tsc --noEmit` é o critério da disciplina.

## Identificação no aparelho

O nome e a escola não são conta e não vão para a API. `AsyncStorage` guarda os dois e a tela inicial lê isso ao abrir. Se já existe, o app segue para as salas. O botão Professor reabre o formulário. É o menor persistência que sobrevive a fechar o Expo Go.

## API e modo simulado

A API ainda está sendo feita, então o app não pode depender dela para a demonstração. `app.config.ts` lê `EXPO_PUBLIC_API_URL` (padrão `http://localhost:5080`) e `EXPO_PUBLIC_API_MOCK` (padrão ligado). Com o mock desligado, `services/api.ts` chama `GET /ambientes`, `GET /ambientes/{id}/previsao` e `POST /ocorrencias` em multipart, nos formatos do contrato. A resposta é conferida campo a campo antes de entrar na tela: JSON inesperado vira erro com "tentar de novo", em vez de quebrar a lista.

O mock devolve cinco salas, uma de cada nível e uma sem leituras recentes. A sala 3 vem com `demonstracao: true`, no mesmo exemplo do contrato (rua 34, delta 4, sala 38). O `POST` simulado devolve um id e não guarda a foto.

## Níveis

A tabela é a do contrato, numa função só (`nivelDeIndice`), testada nos limites: abaixo de 27 é normal, 27 é atenção, 32 é cuidado extremo, 41 é perigo. O número exato do limite de cima fica na faixa seguinte, senão 32 seria atenção e cuidado ao mesmo tempo. A cor do selo nunca aparece sozinha: o nome do nível vai escrito dentro dele. A ação sugerida é a da tabela (nenhuma, água e ventilação, pausa ou troca, tirar a turma).

Quando `semLeiturasRecentes` é verdadeiro, o detalhe não calcula delta e não mostra índice previsto da sala. Mostra a previsão da rua, o nível que a API mandou e o aviso "sala sem leituras recentes". A etiqueta DEMONSTRAÇÃO só aparece se a previsão vier com `demonstracao`.

## Hora

A API manda UTC. A tela formata com `Intl` no fuso `America/Sao_Paulo`. Desde 2019 não há horário de verão, e o fuso do sistema já aplica UTC−3. Não há conta manual de três horas, que erraria no dia em que o aparelho estivesse noutra zona.

## Ocorrência e câmera

A validação mora em `domain/ocorrencia.ts` e não importa React. Descrição com 10 a 500 caracteres, já sem espaço nas pontas, e foto obrigatória. O rótulo do campo é "O que aconteceu". Não existe campo de aluno: a ocorrência é da sala. Os testes cobrem os limites 9, 10, 500 e 501, o recorte de espaço e a foto faltando.

A foto usa `expo-image-picker`, que abre a câmera do sistema e já vem no Expo Go. Qualidade 0,7 gera JPEG, que é o arquivo do `POST`, e o EXIF fica desligado para a foto não levar localização. Se a permissão é negada, a tela explica. "Tentar de novo" pede a permissão outra vez ou, se o sistema não deixa perguntar de novo, abre os ajustes do aparelho.

## Testes sem biblioteca nova

A validação e a conversão de nível rodam com `node --test` e a leitura de TypeScript que o Node 22 já tem. Jest exigiria preset, transformador e mais dependências para a mesma asserção.

## O que ficou de fora

Notificação quando a sala entra em cuidado extremo e a tela de leitura manual do termo-higrômetro estão na coluna "se sobrar tempo" do plano, não neste degrau. Rotas tipadas do Expo ficaram desligadas: elas geram arquivo na primeira execução do bundler e atrapalhariam o `tsc` limpo de quem só clonou o repositório.
