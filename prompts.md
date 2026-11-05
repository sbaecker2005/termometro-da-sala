# Pedidos a assistente de IA

Registro do que a equipe pediu, do que entrou no repositório e do que a equipe reviu. O assistente é tratado de forma genérica. A decisão final é da equipe.

## API

Pedido: API em ASP.NET Core com salas, leituras assinadas, previsão em 2 h, ocorrências com foto, ações e revogação, e testes sem SQL Server.

Aceito: rotas do contrato, HMAC, recusa de reenvio e de dispositivo revogado, modo demonstração que responde 404 fora desse ambiente, painel estático e 27 testes.

Revisado: a equipe manteve a regra de nenhum campo de aluno, o nome da foto como identificador opaco e a mesma checagem no MQTT e no `POST /leituras`. Segredo de demonstração não é segredo de produção.

## App

Pedido: app do professor em Expo e TypeScript, quatro telas, câmera e modo que funcione sem a API.

Aceito: identificação no aparelho, lista, detalhe com previsão e ocorrência com foto. O modo simulado vem ligado.

Revisado: a equipe tirou qualquer campo de aluno, desligou o EXIF da foto e deixou a validação fora da tela, testada com o que o Node já traz. Notificação e leitura manual ficaram de fora.

## Desktop

Pedido: terminal da coordenação em Windows Forms para ver as salas, registrar a ação e revogar o sensor.

Aceito: lista, cadastro, ação, revogação, exportação do dia e atualização periódica.

Revisado: a equipe manteve a reserva local quando a API não responde e o aviso de que esse dado local não volta sozinho para o servidor. O dia da ação é o de São Paulo.

## ESP32

Pedido: sensor no Wokwi que meça, acenda o LED sem internet e publique a leitura assinada.

Aceito: sketch, diagrama, bibliotecas e o exemplo de segredo fora do arquivo versionado.

Revisado: a equipe conferiu o vetor de HMAC do contrato e a regra de interromper o início se o vetor falhar. O segredo real não entra no repositório.

## ML

Pedido: prever o índice de calor da rua em 2 h com dados públicos do INMET, comparar com a regra "daqui a 2 h igual a agora" e exportar o modelo para a API.

Aceito: notebook, série da estação A701 e `modelo.json` no formato de árvore.

Revisado: a equipe manteve a escolha pelo menor MAE na validação. No teste o MAE foi 0,958 °C, e o notebook deixa escrito que esse critério piora o reconhecimento de cuidado extremo. Não houve ajuste olhando o ano de teste.

## Infraestrutura

Pedido: roteiro de Windows Server para laboratório, com AD, DNS, IIS, SQL e testes de falha, em scripts.

Aceito: roteiro e scripts de rede, domínio, usuários, IIS, SQL, GPO, monitoramento, publicação e teste de falha.

Revisado: a equipe marcou a VM única como laboratório, não como produção, e tirou dos scripts o download de instalador e qualquer senha gravada.

## Segurança

Pedido: relatório de riscos da solução, com o que já existe no código e o que ainda é cenário.

Aceito: matriz, controles e o roteiro de evidências.

Revisado: a equipe marcou a matriz como julgamento, não como frequência medida, e deixou o "estado observado" do relatório como foto de uma versão anterior da API. Dados de aluno continuam fora do sistema.

## Arquitetura

Pedido: documento e modelo ArchiMate em quatro camadas, com estimativa de impacto e roteiro de pitch.

Aceito: documento descritivo, pitch e modelo Open Exchange.

Revisado: a equipe trocou as parcelas inventadas de climatização pela tabulação publicada dos microdados, manteve 35 alunos por turma como premissa e não tratou os dias a 34 °C como onda de calor oficial. O diagrama continua para abrir no Archi e exportar a imagem.
