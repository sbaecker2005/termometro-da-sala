# Insomnia — validação manual de 17/11/2026

**Resultado final: 29 requisições executadas, 29 asserções aprovadas e zero falhas**, observado na interface do Insomnia 13.4.0 e registrado em [colecao-final.png](colecao-final.png). O runner mostrou duração de 978 ms.

Ambiente local Scratch Pad, sem conta/sincronização: API em `http://127.0.0.1:5083`, SQL Server Express no banco exclusivo `TermometroGS_Insomnia_20261117`. Foi usada a coleção `api/insomnia.json`, com os scripts de resposta dos quatro pedidos de mapa 26–29 também carregados na interface. O ensaio não utiliza produção.

Após a validação, a API temporária do Insomnia foi encerrada; o banco e os resultados na interface foram preservados. A porta 5083 não deve ser presumida ativa. A apresentação ML/desktop usa outro processo e outro banco, documentados em [FECHAMENTO-ML-SOA-CSHARP.md](../../../docs/FECHAMENTO-ML-SOA-CSHARP.md).

| Evidência | Resultado |
|---|---|
| [Coleção completa](colecao-final.png) | 29/29 asserções aprovadas após executar as 29 requisições |
| [Revogação](05-revogacao.png) | HTTP 200, dispositivo inativo, teste aprovado |
| [Upload](16-upload-201.png) | HTTP 201, ocorrência criada e teste 1/1 aprovado |

O runner percorreu as pastas por recurso da coleção. Para cumprir a dependência da leitura 06, a revogação 05 foi executada antes da rodada; não se alega que o runner seguiu a ordem numérica dos nomes. Para reprodução manual, seguir a numeração de `api/README.md` ou ajustar a ordem no runner. A sala de CRUD é criada e removida durante o roteiro; a revogação permanece no banco de ensaio.

A fixture `sala-sintetica.jpg` é uma ilustração sintética sem pessoas; não é foto de escola nem evidência de piloto. SHA-256: `658173E65646BC7A6F4E8BE60345B392820C849F14430028405574C39511064B`.

A primeira rodada ficou incompleta porque o Insomnia bloqueava o acesso à pasta do JPEG. Isso impediu o upload e deixou sem ID os pedidos seguintes de foto. Foi autorizada apenas a pasta das evidências sintéticas em Preferences > General > Security, sem desativar a proteção geral. Depois, o upload passou; os quatro pedidos de mapa receberam asserções explícitas; a rodada final aprovou 29/29. O resultado incompleto anterior não foi apresentado como aprovação.

As capturas demonstram o cliente Insomnia real consumindo a API/SQL locais. Não comprovam IIS/VM, Wokwi, broker externo nem operação em escola. A API usa fixture pública de autenticação do projeto de testes neste banco isolado; nenhuma credencial privada é reproduzida neste índice ou nas capturas.
