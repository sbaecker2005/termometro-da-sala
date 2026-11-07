# Mapa térmico do ambiente

## Etapas

- M1: modelo, migração, API, demonstração e testes (esta entrega).
- M2: plano cartesiano no desktop e no aplicativo, com bolas azuis e vermelhas e precauções.
- M3: previsão por ponto, preservando o vínculo entre dispositivo e posição.

## Modelo e cálculo

Um ambiente tem um ou mais dispositivos. Cada sensor pode ter `posX`, `posY` em metros e `rotulo` (janela, fundo, porta, centro). `larguraM` e `profundidadeM` são opcionais. Sem posição, o sensor fica no centro; sem dimensões, o centro é `(0, 0)` e a grade contém esse único ponto. Um sensor sozinho mantém sua temperatura, índice e nível em toda a grade. As rotas anteriores mantêm seus contratos.

Lugares têm nome, coordenadas e `vulneravel`. O GET devolve sensores e lugares separados por `tipo`; a interface deve desenhar bolas nos lugares, ou nos sensores quando não houver lugares.

A grade regular tem passo de 0,5 m, começa em `(0, 0)` e inclui coordenadas até os limites da planta. Cada célula recebe temperatura e índice por IDW, potência 2: soma dos valores multiplicados por `1 / distância²`, dividida pela soma dos pesos. Sobre um sensor, usa sua leitura diretamente. Se sensores coincidirem, o primeiro por identificador prevalece na grade; seus valores individuais continuam nos pontos. Posicione sensores distintos para representar diferenças espaciais.

Somente a leitura mais recente de cada sensor entre agora menos 15 minutos (inclusive) e agora participa. Em empate de horário, prevalece o maior identificador de leitura. Sensores sem leitura recente continuam nos pontos com `semLeitura: true` e valores nulos. Sem nenhuma leitura, células e lugares também ficam sem valores; nível normal não é presumido. `leituraMaisAntigaEm` é o menor horário entre as leituras efetivamente usadas, ou nulo.

O nível usa `IndiceCalor.Nivel`: frio abaixo de 18; normal de 18 até menos de 27; atenção de 27 até menos de 32; cuidado de 32 até menos de 41; perigo a partir de 41 °C de índice. O índice de cada sensor é o calculado na ingestão; temperatura e índice são interpolados independentemente. `desvioC` compara a temperatura do ponto com a média aritmética das últimas temperaturas dos sensores participantes.

## Precauções

Lugar vulnerável em frio, cuidado ou perigo recebe indicação de troca para o lugar mais próximo da faixa normal **do índice**. Prioriza lugar normal, depois distância numérica à faixa `[18, 27)`, depois proximidade física e nome. A indicação inclui os índices calculados de origem e destino. Se não houver lugar com condição mais próxima da faixa normal, informa a ausência de destino melhor; não recomenda uma troca que agrave a condição. A indicação não garante disponibilidade do destino.

Diferença de temperatura entre os sensores extremos maior ou igual a 4 °C gera “ambiente desigual: verificar janela, sol e corrente de ar perto de [rótulos]”, com as temperaturas calculadas. Lugares interpolados não criam extremos além dos sensores. As regras são determinísticas e não substituem avaliação presencial.

## Rotas da M1

`GET /ambientes/{id}/mapa` é público, como as consultas atuais; ambiente inexistente retorna 404. Retorna `ambienteId`, dimensões, `passoM`, `grade`, `pontos`, `precaucoes`, `leituraMaisAntigaEm`. Cada ponto contém identificador, rótulo, tipo, coordenadas, vulnerabilidade, temperatura, índice, nível, desvio e estado de leitura. Não retorna segredos dos dispositivos.

`PUT /ambientes/{id}/mapa` exige `X-Chave-Coordenacao` e o limite de escrita existente; retorna 204. Configura dimensões, posiciona dispositivos já cadastrados no ambiente e substitui a lista de lugares. Sensor omitido mantém posição e rótulo; lugar omitido é removido. Lugar com mesmo nome preserva seu identificador. Lista vazia de lugares remove todos os lugares, sem apagar leituras. Nomes são únicos no ambiente, sem distinguir maiúsculas de minúsculas.

```json
{
  "larguraM": 6,
  "profundidadeM": 4,
  "sensores": [{"id": "esp32-sala-01", "posX": 0, "posY": 2, "rotulo": "janela"}],
  "lugares": [{"nome": "Cadeira 1", "x": 1, "y": 2, "vulneravel": true}]
}
```

Dimensões são fornecidas juntas, de 0,5 a 50 m; posições devem estar dentro da planta. Coordenadas do sensor são ambas nulas (centro) ou ambas preenchidas. Sem dimensões, só sensores sem posição e nenhum lugar são aceitos. Reduzir a planta também valida sensores omitidos. Limites: 100 sensores por pedido, 200 lugares e nomes/rótulos até 80 caracteres. A grade tem no máximo 10.201 células. O PUT não cadastra nem revoga dispositivos.

## Demonstração

Somente no ambiente `Demo`, `POST /demo/mapa?periodo=tarde` cria Sala 7 (`sala-07`), 6 × 4 m, quatro sensores e seis cadeiras, duas vulneráveis. Janela: 35 °C; demais: 24 °C; umidade 60%. `periodo=madrugada` usa fundo a 15 °C e os demais a 22 °C. São cenários explícitos, com horário atual, para permanecerem recentes e reproduzíveis em qualquer horário. Execute novamente após 15 minutos. A resposta aponta para `/ambientes/sala-07/mapa`.

Repetir substitui somente leituras simuladas dessa sala. Não semeia automaticamente em outros ambientes de execução. Sala 7 com leitura real ou configuração de dispositivos incompatível é recusada com 409. A demonstração anterior em `/demo/semear` continua disponível.

## Migração e validação

`MapaTermico` acrescenta colunas opcionais e a tabela `Lugares`, sem apagar histórico. O script `api/evidencias/mapa-termico-idempotente.sql` usa o histórico de migrações EF para permitir repetição após `AlertaFrio`. Reverter a migração remove posições, dimensões e lugares.

```powershell
dotnet test api/TermometroDaSala.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false
```

Os testes HTTP usam SQLite em memória conforme a suíte existente. Os testes de esquema geram SQL Server idempotente e verificam o modelo sem abrir conexão. A aplicação da migração no SQL Server requer validação adicional no servidor local.
