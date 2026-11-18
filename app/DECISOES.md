# Decisões do app

O app é a parte do professor na CitizenTech Termômetro da Sala. A entrega de Mobile pede Expo, TypeScript estrito, quatro telas (o mapa térmico fica dentro do detalhe da sala), câmera e a pasta `services/`. Cada escolha abaixo existe para cumprir isso sem inventar uma segunda arquitetura.

## Expo SDK 58 e expo-router

O SDK 58 é o estável atual e roda no Expo Go, que é o formato pedido (vídeo no celular, sem loja e sem build nativo). A navegação é o expo-router, o roteamento em arquivo que o próprio Expo instala hoje. As rotas são as telas: identificação, lista, detalhe e ocorrência. Não há WebView.

TypeScript fica com `"strict": true`. Os modelos da API, os níveis e as respostas têm tipo. Não há `any` no código do app. `npx tsc --noEmit` é o critério da disciplina.

## Identificação no aparelho

O nome e a escola não são conta e não vão para a API. A regra (os dois preenchidos, já sem espaço nas pontas) fica em `domain/professor.ts`. `AsyncStorage` guarda os dois e a tela inicial lê isso ao abrir. Se já existe, o app segue para as salas. O botão Professor reabre o formulário. É a menor persistência que sobrevive a fechar o Expo Go. Se a gravação falha, a tela avisa e não sai do formulário.

## API e modo simulado

A API está em `api/`, mas a demonstração não pode depender de rede na sala de aula. Por isso o modo simulado deixa o app funcionar sozinho no Expo Go, sem substituir a API. `app.config.ts` lê `EXPO_PUBLIC_API_URL` (padrão `http://localhost:5080`) e `EXPO_PUBLIC_API_MOCK` (padrão ligado). Com `EXPO_PUBLIC_API_MOCK=0`, `services/api.ts` chama as rotas da API: `GET /ambientes`, `GET /ambientes/{id}/previsao`, `GET /ambientes/{id}/ocorrencias` e `POST /ocorrencias` em multipart. A resposta é conferida campo a campo antes de entrar na tela: JSON inesperado vira a mensagem "O servidor respondeu num formato inesperado" com "Tentar de novo", e falha de rede vira "Não foi possível falar com o servidor", em vez do texto técnico do sistema. No `GET` da previsão, 404 é "Sala não encontrada" e o 503 com `{"motivo":"sem-dados-para-prever"}` não é erro: a tela mostra a sala e avisa que não há dados para prever. Outro 503 continua sendo falha do servidor.

A API marca a sala que nunca recebeu leitura com `semLeitura: true` (e manda nível normal, índice 0 e data vazia). O app mostra "Sem leitura" no lugar do nível, do índice e da hora, para não exibir "Normal 0,0 °C".

O mock devolve seis salas, uma de cada nível e uma com leitura antiga. Cada chamada simulada espera cerca de 600 ms, para o estado "carregando" aparecer. `EXPO_PUBLIC_API_MOCK=vazio` devolve a lista sem salas e `EXPO_PUBLIC_API_MOCK=erro` faz toda chamada falhar, para mostrar os outros dois estados da lista. O `POST` simulado devolve um id local (data e contador; o Hermes do Expo Go não tem `crypto.randomUUID`) e guarda descrição e horário em memória, para o detalhe da sala. A foto não fica guardada.

## Mapa térmico

O detalhe da sala desenha a planta e a grade térmica com `View`s, sem biblioteca de SVG: o Expo Go já traz tudo e o desenho é só retângulos e círculos posicionados em proporção. Cada ponto mostra nome, temperatura, nível e precaução ao toque, e uma lista abaixo repete o conteúdo para leitor de tela. A cor nunca vem sozinha. A previsão +1 h é aproximada: não há modelo de temperatura por ponto, então a API aplica a todos os pontos metade da variação do índice prevista para +2 h, e as precauções dizem isso. `lerMapa` (`domain/mapa.ts`) confere a resposta antes de ela chegar à tela, sem afirmar o tipo antes: ambiente igual ao pedido, planta de 0,5 a 50 m, coordenadas dentro da planta, níveis da tabela e limites de tamanho. No modo simulado o mapa acompanha o nível da sala (a Sala 1, normal, mostra mapa normal), com 4 sensores e 20 cadeiras, e a etiqueta DEMONSTRAÇÃO.

## Níveis

A tabela é a do contrato, numa função só (`nivelDeIndice`), testada nos limites: abaixo de 18 é frio, de 18 até menos de 27 é normal, 27 é atenção, 32 é cuidado extremo, 41 é perigo. A cor do selo nunca aparece sozinha: o nome do nível vai escrito dentro dele. A ação sugerida é a da tabela (agasalhar e oferecer cobertor no frio; nenhuma, água e ventilação, pausa ou troca, tirar a turma nas demais faixas).

Quando `semLeiturasRecentes` é verdadeiro, o detalhe não calcula delta e não mostra índice previsto da sala. Mostra a previsão da rua, o nível que a API mandou e o aviso "sala sem leituras recentes". Quando `ruaDisponivel` é falso, a API mantém a sala no índice da última leitura válida e manda `indiceRuaPrevisto` 0; o detalhe mostra o aviso de que a previsão usa a última leitura válida da sala e não mostra o índice da rua.

## Hora

A API manda UTC. A tela formata com `Intl` no fuso `America/Sao_Paulo`. Não há conta manual de três horas, que erraria no dia em que o aparelho estivesse noutra zona.

## Ocorrência e câmera

A validação mora em `domain/ocorrencia.ts` e não importa React. Descrição com 10 a 500 caracteres, já sem espaço nas pontas, e foto obrigatória. O rótulo do campo é "O que aconteceu". Não existe campo de aluno: a ocorrência é da sala.

A foto usa `expo-image-picker`, que abre a câmera do sistema e já vem no Expo Go. Qualidade 0,7 gera JPEG, que é o arquivo do `POST`, e o EXIF fica desligado para a foto não levar localização. Se a permissão é negada, a tela explica. "Tentar de novo" consulta a permissão atual: abre os ajustes somente se ela continua negada e o sistema não deixa perguntar de novo. Ao voltar dos ajustes com autorização, segue para a câmera. O teste de `services/camera.ts` cobre essa mudança de permissão e a nova tentativa quando o sistema ainda pode perguntar.

## Envio da foto

O `FormData` do React Native aceita um objeto `{ uri, name, type }` no lugar de um `Blob`, mas o tipo do DOM não prevê isso. Em `services/api.ts` há um único `as unknown as Blob`, só nesse ponto, para satisfazer o compilador. O arquivo vem do seletor de imagem, não de resposta de rede.

## Testes sem biblioteca nova

A validação, a conversão de nível e a retomada da câmera rodam com `node --test` e a leitura de TypeScript que o Node já tem (`--experimental-strip-types`). O teste do serviço HTTP usa `module.registerHooks`, disponível desde Node 22.15; o lock do Expo/React Native aceita 22.x desde 22.13, 24.x desde 24.3 ou 26 e posteriores. Para instalar e testar, usamos 22.x desde 22.15, 24.x desde 24.3 ou 26 e posteriores.

## O que ficou de fora

Notificação quando a sala entra em cuidado extremo e a tela de leitura manual do termo-higrômetro estão na coluna "se sobrar tempo" do plano, não neste degrau. Rotas tipadas do Expo ficaram desligadas: elas geram arquivo na primeira execução do bundler e atrapalhariam o `tsc` limpo de quem só clonou o repositório. Os parâmetros de `router.push` e `useLocalSearchParams` usam o tipo `RotaParams` em `rotas.ts`.
