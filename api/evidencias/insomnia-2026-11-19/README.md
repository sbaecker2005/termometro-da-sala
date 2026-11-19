# Insomnia — rodada de 19/11/2026

**29 requisições, 29 testes aprovados (1/1 em cada), zero falhas**, no Insomnia 13.4.0 contra a API atual (commit `3b62a13` em diante) em `http://localhost:5080`, ambiente `Demo`, SQL Server Express em banco novo de ensaio e MQTT desligado. A chave da coordenação é um valor de teste só deste ensaio e não aparece em nenhum print (todos na aba Params).

Os testes 28 e 29 esperavam `inseridas: 4`, mas a API grava 4 sensores × 3 leituras (uma por minuto) e responde `inseridas: 12` (`DemoController.cs:71`). O erro era da coleção e da documentação; `api/insomnia.json`, o `CONTRATO.md` e o `api/README.md` foram corrigidos e a rodada fechou 29/29.

Cada print mostra o método, a rota, o status, o tempo, o corpo da resposta e o resultado do teste.

| Print | Requisição |
|---|---|
| [01.png](01.png) | 01 POST /leituras - sucesso |
| [02.png](02.png) | 02 POST /leituras - reenvio |
| [03.png](03.png) | 03 POST /leituras - assinatura-invalida |
| [04.png](04.png) | 04 POST /leituras - fora-da-faixa |
| [05.png](05.png) | 05 POST /dispositivos/{id}/revogar |
| [06.png](06.png) | 06 POST /leituras - dispositivo-revogado |
| [07.png](07.png) | 07 POST /ambientes |
| [08.png](08.png) | 08 POST /ambientes - repetido |
| [09.png](09.png) | 09 POST /ambientes - sem chave |
| [10.png](10.png) | 10 GET /ambientes/{id} |
| [11.png](11.png) | 11 PUT /ambientes/{id} |
| [12.png](12.png) | 12 DELETE /ambientes/{id} - com leituras |
| [13.png](13.png) | 13 DELETE /ambientes/{id} |
| [14.png](14.png) | 14 POST /acoes |
| [15.png](15.png) | 15 GET /acoes?dia=AAAA-MM-DD |
| [16.png](16.png) | 16 POST /ocorrencias |
| [17.png](17.png) | 17 GET /ambientes/{id}/ocorrencias |
| [18.png](18.png) | 18 GET /ocorrencias/{id}/foto - sem chave |
| [19.png](19.png) | 19 GET /ocorrencias/{id}/foto |
| [20.png](20.png) | 20 GET /ambientes |
| [21.png](21.png) | 21 GET /ambientes/{id}/leituras |
| [22.png](22.png) | 22 GET /ambientes/{id}/leituras?formato=csv |
| [23.png](23.png) | 23 GET /ambientes/{id}/previsao |
| [24.png](24.png) | 24 GET /niveis |
| [25.png](25.png) | 25 POST /demo/semear |
| [26.png](26.png) | 26 Configurar mapa (204; chave obrigatoria) |
| [27.png](27.png) | 27 GET mapa da sala-01 (200; apos configurar) |
| [28.png](28.png) | 28 Demo Sala 7 tarde (201; consultar mapa com GET apos preparar) |
| [29.png](29.png) | 29 Demo Sala 7 madrugada (201; consultar mapa com GET apos preparar) |

A foto do 16/19 é a ilustração sintética da fixture (sem pessoas). Esta rodada não comprova IIS/VM, Wokwi nem broker externo: é a API local com SQL Server real.
