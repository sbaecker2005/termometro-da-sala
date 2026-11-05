# Vídeo de Mobile — até 2 minutos

Meta: **1 min 50 s**. O app é a porta do professor na mesma solução: a sala que o ESP32 mede, que o modelo prevê e que a coordenação vê no desktop. Grave no Expo Go, no celular. Não mostre chave da coordenação, segredo HMAC nem aluno na foto.

Antes de gravar: `npx expo start` na pasta `app`, Expo Go na mesma rede, identificação ainda não salva (apague o app no Expo Go se já houver nome gravado). O modo simulado vem ligado e basta para as quatro telas. Se for mostrar a API, suba com `EXPO_PUBLIC_API_MOCK=0` e o IP da máquina, não `localhost`. Tenha a sala 3 à mão: no simulado ela traz previsão (rua 34, delta 4, sala 38) e a etiqueta DEMONSTRAÇÃO. Fotografe só a sala ou o equipamento, sem pessoas.

| Tempo | Mostrar | O que dizer |
|---|---|---|
| 0:00–0:15 | Nome do projeto e a frase do problema. Diga que o professor usa este app e a coordenação usa o desktop. | O app não mede e não prevê. Ele mostra o que o sensor e o modelo já produziram. |
| 0:15–0:35 | Identificação: nome e escola, entrar nas salas, fechar o Expo Go e abrir de novo. O app volta direto na lista. | O nome fica no aparelho. Não vai para a API e não é dado de aluno. |
| 0:35–0:55 | Lista de salas com nível escrito e cor, índice e hora em São Paulo. Se der, force o estado vazio ou o erro com "Tentar de novo"; senão, aponte a sala sem leitura recente e a sala 3. | Carregando, erro e vazio são estados da lista. A hora na tela é UTC−3, a mesma do contrato. |
| 0:55–1:20 | Detalhe da sala 3: nível de agora, previsão de 2 h, ação sugerida e a etiqueta DEMONSTRAÇÃO. Aponte o aviso quando a sala está sem leituras recentes. | A previsão é a rua do modelo mais o delta do ESP32. A API é quem oficializa; o app só exibe. |
| 1:20–1:50 | Nova ocorrência: descrição curta demais é recusada; texto de 10 a 500 caracteres passa. Abra a câmera, negue a permissão e mostre a explicação, depois permita e fotografe a sala vazia. A ocorrência aparece no detalhe. Feche dizendo que a mesma sala está no desktop da coordenação, onde a ação fica registrada. | O campo é "o que aconteceu", sem nome de aluno. A foto sai com o EXIF desligado. A decisão continua com a coordenação. |
