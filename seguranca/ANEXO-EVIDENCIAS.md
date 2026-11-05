# Termômetro da Sala — Anexo de evidências de Cybersecurity

Anexo do [RELATORIO-RISCOS.md](RELATORIO-RISCOS.md). Para cada evidência: o controle e o risco, o comando, o resultado esperado e o resultado obtido. Os comandos completos estão no roteiro [EVIDENCIAS.md](EVIDENCIAS.md), que fica fora do PDF.

Há duas colunas de resultado porque há dois ambientes:

- **Local (05/11/2026):** Windows 11, .NET SDK 8.0.427, API com o pipeline dos testes automatizados e banco SQLite em memória com a semente das quatro salas. Arquivos em [evidencias/](evidencias/).
- **Laboratório Windows Server:** API publicada no IIS em Production, com SQL Server, GPO e firewall do roteiro `infra/`. Cada linha recebe o print com data e hora quando a equipe executar. Enquanto a célula diz "a executar", o relatório não conta o controle como comprovado no laboratório.

A saída antiga de `cenarios-ataque.txt` precede a correção de R12. Repetir o script atualizado; o download de S3.3 agora exige a chave. A suíte atual comprova R12 e C11.

Nenhum print mostra chave, senha, string de conexão, rosto ou nome de aluno.

## Cenários de ataque (seção 4.2 do relatório)

Comando: `.\seguranca\cenarios-ataque.ps1 -Api https://api.termometro.local`, com `$env:CHAVE_COORDENACAO` definida para os passos S2.5, S3.3, S3.5 e S3.6. Usar a URL HTTPS, com o certificado importado conforme `infra/README.md` (seção "HTTPS no laboratório"): em Production a API redireciona HTTP para HTTPS, e o script para com aviso se a primeira resposta for um redirecionamento. O código de saída é o número de passos que divergem. Rodar antes do E04, porque o S3 revoga `esp32-sala-04`.

| ID | Controle e risco | Esperado | Obtido local (05/11/2026) | Laboratório (print, data) |
|---|---|---|---|---|
| S1 | C2 contra SQLi (R3) | Injeções no id respondem 404 sem dados de outra sala; parâmetro `formato` não altera o resultado; tabela intacta depois do `DROP` | 7 de 7 passos conferem; linha de base da sala-04 com 1 leitura, igual com o `formato` injetado; log com `@__id_0` de 18, 48 e 33 caracteres e nenhum `OR`, `UNION` ou `DROP` no SQL (`evidencias/cenarios-ataque.txt`) | A executar contra o IIS |
| S2 | C3 e C4 contra LFI e upload (R4, R5); R12 corrigido | Travessias na URL e na rota da foto em 4xx sem conteúdo de arquivo; nomes com `..` em 400 `nome-invalido`; ASPX em 400 `nao-jpeg`; upload legítimo com GUID; foto lida sem chave em 401 | 10 de 10 passos conferem; S2.2 e S2.3 com 403 do HTTP.sys do encaminhador usado só na coleta local (sem ele, 404 do Kestrel e da rota `{id:guid}`; o IIS confirma); S2.10 registrou 200 antes da correção; agora 401 no teste automatizado, repetir o script | A executar contra o IIS |
| S3 | C1, C6, C9 e C10 contra OSINT (R6, R7, R10) | Vetor público assina até a revogação; EXIF removido; revogação sem chave em 401; com chave em 200; forja seguinte em 400 `dispositivo-revogado` | 6 de 6 passos conferem, com S3.4 em 401 `chave-invalida`; busca no histórico do git sem achado além dos vetores e do valor de teste `chave-teste` da suíte (`evidencias/osint.txt`, item 8) | A executar contra o IIS; link do Wokwi (item 9 do `osint.txt`) a conferir depois de publicado |

## Evidências E01 a E15

| ID | Controle e risco | Comando (roteiro) | Esperado | Obtido local (05/11/2026) | Laboratório (print, data) |
|---|---|---|---|---|---|
| E01 | Versão e local dos controles | Seção 1 do roteiro | Arquivo e linha de C1–C12 iguais aos da seção 6 do relatório | Linhas da seção 6 conferidas; suíte completa 47/47 e 20 testes de segurança aprovados (`evidencias/dotnet-test-seguranca.txt`) | A executar |
| E02 | C1, leitura legítima (R1) | Seção 2: `POST /leituras` assinada | 2xx e leitura gravada | 201 nos passos S1.1 e S3.1 (`evidencias/cenarios-ataque.txt`) | A executar |
| E03 | C1, assinatura inválida e reenvio (R1, R2) | Seção 3 | 4xx com `assinatura-invalida` e `reenvio`, sem gravar | Os dois motivos no log (`evidencias/recusas-log.txt`); duplicata concorrente sem trava continua como lacuna declarada (C1 parcial) | A executar, com reinício da API e ingestão MQTT |
| E04 | C1 e C6, revogação e janela (R2, R7) | Seção 4 | `dispositivo-revogado` e `atrasada` | `dispositivo-revogado` no S3.6; `atrasada` no log (`evidencias/recusas-log.txt`) | A executar, com o relógio do Wokwi |
| E05 | C2, SQLi (R3) | Seção 5 e cenário S1 | 404 para o id com apóstrofo; SQL com parâmetro | 404 e `@__id_0` (S1 e `evidencias/consulta-parametrizada.txt`) | A executar |
| E06 | C3, LFI (R4) | Seção 6 e cenário S2 | Foto legítima 200 com nome GUID; travessias 4xx | S2.1 a S2.5 e S2.9 conferem | A executar, com o nome em disco e a ACL da pasta |
| E07 | C3 e C4, upload malicioso e tamanho (R5) | Seção 7 | Falso JPEG e nome com `..` em 400; arquivo acima de 5 MB em 400 `arquivo` | Conteúdo e nome: S2.6 a S2.8 e `evidencias/upload-travessia.txt`. Tamanho: sem saída local | A executar, inclusive o limite de 5 MB |
| E08 | C4, validação na API e no app (R5) | Seção 8 | Descrição fora de 10 a 500 caracteres, sem foto ou sala inexistente em 4xx | Cadastro de sala com entrada inválida recusado (`ApiTestes.CadastroRecusaEntradaInvalida`, em `dotnet-test-seguranca.txt`); `fora-da-faixa` no log | A executar, com prints do app |
| E09 | C5, `/demo/semear` fora de Demo (R11) | Seção 9 | 404 e log `fora-do-ambiente` | Log `fora-do-ambiente` e teste aprovado (`evidencias/demo-semear-404.txt`) | A executar em Production e em Demo |
| E10 | C6, segredo fora do repositório (R7, R10) | Seção 10 | `secrets.h` ignorado; histórico só com vetores de teste | `.gitignore:19` (`evidencias/git-check-ignore.txt`); histórico conferido (`evidencias/osint.txt`, item 8) | Revogação final dos vetores depois de 19/11/2026 |
| E11 | C7, GPO, RDP, NTFS e auditoria (R9) | Seção 11 | GPO aplicada, RDP negado a conta sem grupo, eventos 4625 e 4663 correlacionados | Sem saída local: depende do Windows Server | A executar |
| E12 | C8, portas mínimas com nmap (R9, R14) | Seção 12 | 443 com certificado do laboratório, 80 só redirecionando, 1433 e 3389 fechadas à rede comum | Sem saída local: depende do Windows Server | A executar, de duas origens |
| E13 | C8, broker indisponível (R8) | Seção 13 | LED reage sem internet; a API não inventa leitura atual | `semLeiturasRecentes` verdadeiro sem pares (`ApiTestes.DeltaSemTresParesENuloEComTresFechaOExemplo`) | A executar, com o LED no Wokwi |
| E14 | C9, OSINT e dados de aluno (R6) | Seção 14 e passos S3.2 e S3.3 | Foto devolvida sem APP1; aviso no app; nenhuma pessoa nas fotos | APP1 com `GPS-TESTE` removido (S3.3 e `ApiTestes.UploadRemoveSegmentoApp1`) | A executar: aviso do app e revisão humana das fotos |
| E15 | C10, acesso sem chave (R12) | Seção 15 e passos S3.4 e S2.10 | Escrita da coordenação sem chave em 401; foto sem chave em 401 | 401 `chave-invalida` no S3.4; 200 histórico no S2.10; agora 401 comprovado por `OcorrenciaCriadaVoltaNaListaDaSalaEFotoSoComChave` | A executar pelo desktop |

## Como fechar o anexo

1. Rodar o script de cenários contra a API do laboratório e colar a saída na coluna do laboratório das linhas S1 a S3.
2. Executar E01 a E15 pelo roteiro e trocar cada "A executar" pelo print, com data, hora e ambiente na legenda.
3. Se um resultado divergir do esperado, registrar o obtido como está e manter o risco aberto no relatório.
