# Termômetro da Sala

## Relatório de Gestão de Riscos

**Equipe:** Termômetro da Sala  
**Integrante informado:** Samuel Baecker - RM [preencher]  
**Disciplina:** Cybersecurity  
**Instituição:** FIAP — Engenharia de Software, Global Solution 2026.2  
**Data da análise:** 05/11/2026

## 1. Escopo e ficha resumida

| Campo | Descrição |
|---|---|
| Projeto | Termômetro da Sala |
| Local de referência | São Paulo; recorte de uma sala de escola pública sem climatização |
| Beneficiários e operadores | Professor e turma; coordenação pedagógica decide e registra as ações |
| Problema | Calor dentro da sala sem medição e sem informação antecipada para a coordenação |
| Funcionamento previsto | ESP32 mede temperatura e umidade, sinaliza localmente e publica por MQTT; API valida, armazena e fornece nível atual e previsão de duas horas; app registra ocorrências com foto; desktop registra ações e revoga sensores |
| Tecnologia prevista | ESP32/DHT22 no Wokwi, MQTT público, ASP.NET Core, EF Core/SQL Server, Expo/React Native, Windows Forms e IIS/Windows Server no domínio termometro.local |
| Indicadores | Aulas em nível Perigo sem ação registrada e tempo entre alerta e ação |
| ODS | 3, 4, 11 e 13: saúde, ambiente escolar seguro, adaptação urbana e resiliência climática |
| Dados | Medições por ambiente, ocorrências simuladas e meteorologia pública do INMET; nenhum dado de aluno deve ser coletado |

O escopo inclui banco, API REST, assinante MQTT, servidor Windows, dispositivo, configuração de segredos, contas administrativas e imagens. Abrange SQL Injection (SQLi), inclusão/leitura indevida de arquivo local (LFI), exposição por fontes abertas (OSINT), acesso remoto e disponibilidade. Não é certificação de segurança nem validação clínica dos limites de calor.

**Estado observado em 05/11/2026:** a API tem salas, leituras assinadas, ocorrências, ações, revogação e previsão. O HMAC usa segredo distinto por dispositivo. A coordenação escreve com `X-Chave-Coordenacao`. As rotas de escrita têm cota de 30 pedidos por minuto por IP. O upload regrava o JPEG sem APP1. O app do professor continua com simulação por padrão (`app/app.config.ts:4`). GPO, firewall, nmap e IIS não foram executados nesta pasta: ficam para a VM do Samuel. As saídas locais estão em [evidencias/](evidencias/).

## 2. Inventário e classificação dos ativos

Escala de criticidade de confidencialidade (C), integridade (I) e disponibilidade (D): **baixa**, **média** ou **alta**, conforme o dano da exposição, alteração ou indisponibilidade. Criticidade não é probabilidade de ataque.

| ID | Ativo e conteúdo | C | I | D | Responsável funcional e justificativa |
|---|---|---|---|---|---|
| A1 | Banco SQL Server: ambientes, dispositivos, leituras, previsões, ocorrências e ações | Alta | Alta | Alta | Backend/coordenação; alteração muda alertas e histórico; segredos e fotos exigem restrição |
| A2 | API REST e assinante MQTT | Média | Alta | Alta | Backend; é a fronteira de validação entre internet, dispositivo e banco |
| A3 | Windows Server, IIS, AD, disco de fotos e logs | Alta | Alta | Alta | Infraestrutura; comprometimento dá acesso a serviços, arquivos e contas |
| A4 | ESP32, sensor e LED local | Baixa | Alta | Alta | Dispositivo/coordenação; leitura ou sinal incorreto prejudica a decisão; LED deve continuar sem rede |
| A5 | Segredos HMAC por dispositivo, conexão do banco e credenciais administrativas | Alta | Alta | Alta | Backend/infraestrutura; permitem forja ou tomada de controle; indisponibilidade impede operação |
| A6 | Contas da coordenação e grupos administrativos/RDP | Alta | Alta | Alta | Coordenação/infraestrutura; habilitam decisões, revogação e administração |
| A7 | Fotos de ocorrências e metadados | Alta | Alta | Média | Aplicativo/backend; podem identificar pessoas e local, embora isso seja proibido pelo projeto |
| A8 | App, desktop e painel público | Média | Alta | Média | Interfaces; painel mostra somente informação por sala, sem fotos; dados do professor ficam no aparelho |

O app armazena nome e escola do professor localmente (`app/services/professor.ts:5` e `:19`); isso continua sendo dado pessoal de adulto, sem equivaler a autenticação. O objetivo de não coletar dados de aluno não significa ausência de qualquer dado pessoal.

## 3. Ameaças, vulnerabilidades e consequências

| Risco | Ativos | Agente e cenário concreto | Vulnerabilidade a impedir | Consequência |
|---|---|---|---|---|
| R1 — leitura forjada | A1, A2, A4 | Pessoa no tópico público publica calor falso para liberar a turma ou ocultar calor real | Ingestão sem assinatura por dispositivo ou sem associação dispositivo/ambiente | Alerta e previsão incorretos; decisão operacional prejudicada |
| R2 — reenvio | A1, A2 | Ouvinte copia uma mensagem válida e a publica novamente, inclusive após reinício da API | Aceitar timestamp igual/anterior; estado antirreenvio só em memória; corrida entre MQTT e HTTP | Histórico duplicado ou leitura antiga tratada como atual |
| R3 — SQLi na consulta de leituras | A1, A2 | Cliente manda apóstrofo e `OR 1=1` no id de `GET /ambientes/{id}/leituras` | Concatenar o id em SQL | Leitura ou alteração indevida de registros |
| R4 — LFI no download | A3, A7 | Cliente tenta `../`, caminho absoluto ou nome de arquivo do Windows no identificador da foto | Usar caminho fornecido pelo cliente em leitura de disco | Exposição de configuração, credenciais ou arquivos do servidor |
| R5 — upload malicioso | A2, A3, A7 | Cliente envia script como JPEG, nome com travessia ou arquivo grande | Confiar em extensão/MIME, permitir execução no diretório de imagens ou não limitar tamanho | Execução, sobrescrita de arquivos ou disco cheio |
| R6 — OSINT e LGPD | A7, A8 | Terceiro reconhece aluno, uniforme, placa, documento ou localização em foto pública | Fotografar pessoas, enviar EXIF/localização ou publicar fotos no mural/repositório | Identificação e exposição de menor de idade |
| R7 — segredo do Wokwi | A4, A5 | Visitante do projeto copia o segredo e assina novas mensagens daquele dispositivo | Link do simulador permite ver todos os arquivos, inclusive secrets.h | HMAC válido produzido por terceiro até revogação |
| R8 — negação de serviço | A2, A4 | Terceiro ocupa o tópico com lixo; broker público cai ou limita conexões | Dependência de serviço público sem garantia de capacidade; payloads sem limite | Falta de medições recentes e atraso na resposta da coordenação |
| R9 — acesso indevido/RDP | A1, A3, A6 | Conta comum ou atacante tenta logon remoto no servidor | RDP exposto, permissões amplas, senha fraca e auditoria desativada | Tomada do servidor e dos dados |
| R10 — credenciais versionadas | A1, A3, A5 | Leitor de repositório, histórico, print ou log recupera segredo | Segredo em configuração rastreada; acreditar que ignorar arquivo remove histórico | Forja de leituras, acesso ao banco ou servidor |
| R11 — semeadura indevida | A1, A2 | Cliente chama POST /demo/semear no ambiente de produção | Rota de carga simulada registrada em qualquer ambiente e sem HMAC | Histórico artificial influencia previsão real |
| R12 — operações administrativas sem controle | A2, A6, A7 | Cliente direto tenta registrar ação, revogar sensor ou ler foto sem autorização | Confundir identificação local do professor e middleware com autenticação/autorização | Decisões falsas, revogação indevida e acesso a imagens |

`UseAuthorization()` em `api/TermometroDaSala.Api/Program.cs:67` sozinho não protege as rotas. A escrita da coordenação passa pelo filtro `ExigeChaveCoordenacaoAttribute`. Leitura de foto e `POST /ocorrencias` seguem sem essa chave.

## 4. Matriz de risco: probabilidade × impacto

Probabilidade (P): 1 rara, 2 improvável, 3 possível, 4 provável, 5 quase certa. Impacto (I): 1 desprezível, 2 pequeno/reversível, 3 interrupção localizada, 4 perda relevante ou decisão operacional incorreta, 5 comprometimento amplo ou exposição de menores. Pontuação **P × I**: 1–4 baixo; 5–9 moderado; 10–16 alto; 17–25 crítico.

“Antes” estima o cenário sem o conjunto de controles. “Depois” usa os controles que o código desta pasta já executa (HMAC por dispositivo, consulta LINQ, nome GUID, validação, 404 do Demo, chave da coordenação, cota de escrita e remoção de APP1). C7 e a parte de rede de C8 não entram nessa conta: GPO, firewall, nmap e IIS ficam na VM do Samuel, e por isso R9 continua crítico. A escala é julgamento qualitativo da equipe, sem frequência medida. O impacto pode continuar elevado mesmo quando a probabilidade diminui.

| ID | Antes P | Antes I | Antes P×I / classe | Controles | Depois P | Depois I | Depois P×I / classe |
|---|---:|---:|---|---|---:|---:|---|
| R1 | 4 | 4 | 16 alto | C1 | 2 | 4 | 8 moderado |
| R2 | 4 | 4 | 16 alto | C1 | 1 | 4 | 4 baixo |
| R3 | 3 | 5 | 15 alto | C2 | 1 | 5 | 5 moderado |
| R4 | 3 | 5 | 15 alto | C3 | 1 | 5 | 5 moderado |
| R5 | 4 | 5 | 20 crítico | C3, C4 e cota de escrita | 2 | 4 | 8 moderado |
| R6 | 4 | 5 | 20 crítico | C9 | 2 | 5 | 10 alto |
| R7 | 5 | 4 | 20 crítico | C1, C6 | 3 | 3 | 9 moderado |
| R8 | 4 | 4 | 16 alto | cota de 30/min; broker público permanece | 3 | 3 | 9 moderado |
| R9 | 4 | 5 | 20 crítico | C7 ainda na VM | 4 | 5 | 20 crítico |
| R10 | 3 | 5 | 15 alto | C6 | 2 | 4 | 8 moderado |
| R11 | 3 | 4 | 12 alto | C5 | 1 | 4 | 4 baixo |
| R12 | 4 | 5 | 20 crítico | C10 | 2 | 4 | 8 moderado |

Não se aceita exposição real de menores nem produção aberta com risco alto. O recorte demonstrativo reduz o impacto de R7/R8 por usar dados simulados, decisão humana e sinalização local; fora desse recorte é necessário reavaliar a matriz.

## 5. Controles e comprovação

| ID | Controle e critério de aceitação | Onde está implementado / estado observado | Evidência a anexar |
|---|---|---|---|
| C1 | HMAC-SHA256 por dispositivo; bytes UTF-8 `ambienteId\|dispositivo\|temperaturaC\|umidadePct\|medidoEm`, temperatura com uma casa e cultura invariável, comparação em tempo constante. Validar vínculo com ambiente, dispositivo ativo, timestamp estritamente crescente e persistido; rejeitar mais de 10 min atrás ou 1 min à frente. HTTP e MQTT devem usar a mesma regra; atualização atômica impede duplicata concorrente | `Assinatura.Confere` e `FixedTimeEquals` em `api/TermometroDaSala.Api/Dominio/Contrato.cs:71` e `:79`. Motivos em `ValidadorLeitura.Motivo` (`Contrato.cs:98`). HTTP em `LeiturasController.cs:41`; MQTT em `AssinanteMqtt.cs:63`; os dois chamam `ServicoLeituras.Receber` (`ServicoLeituras.cs:26`), que registra a recusa em `:43`. Segredo distinto em `SalaDb.cs:64` e na migração `Dados/Migrations/20261105050515_SegredosPorDispositivo.cs:13`. Testes: `VetoresTestes.HmacDoContrato`, `ApiTestes.RecusasELeituraAceita`, `ApiTestes.SegredoDeOutraSalaNaoAssina` | E02–E04 e `seguranca/evidencias/recusas-log.txt` |
| C2 | Consultas LINQ/EF; nunca concatenar o id em SQL. O texto do cliente vai como parâmetro, nunca dentro do SQL | `GET /ambientes/{id}/leituras` em `LeiturasController.cs:19`: `Where(l => l.AmbienteId == id)`. Não há SQL montado no controlador. `ToQueryString()` (SQLite do teste e provedor SQL Server, sem abrir conexão) mostra o `WHERE` comparando com um parâmetro `@...`; o texto `OR 1=1` não aparece no comando. O GET com esse id responde 404 e não lista as outras salas. Teste: `ApiTestes.ConsultaDeLeiturasParametrizada` | E05 |
| C3 | Gerar nome GUID no servidor; guardar imagem fora de diretório executável e servir por id de ocorrência, resolvido no banco; nunca aceitar caminho do cliente. GUID evita colisão, não substitui autorização | Nome em `OcorrenciasController.cs:31`. Leitura só com `Path.GetFileName` em `:47`. Nome com `..`, `/` ou `\` recusado em `:22` (`nome-invalido`). O `foto.jpg` de `app/app/salas/[id]/ocorrencia.tsx:38` é só o nome enviado pelo cliente. Teste: `ApiTestes.UploadRecusaOQueNaoEJpegENomeComDoisPontos` (`../x.jpg`) | E06–E07 e `seguranca/evidencias/upload-travessia.txt` |
| C4 | Validar na API ambiente existente, campos/tipos, temperatura -10 a 60 °C e umidade 0 a 100%; descrição 10–500 caracteres após trim; foto obrigatória, tamanho limitado, conteúdo de imagem decodificável e tipo permitido. Limite de bytes também na ingestão MQTT/HTTP | Cliente: `app/domain/ocorrencia.ts:6`, `ocorrencia.tsx:54`, `ocorrencia.test.ts:5`. Servidor: descrição, 5 MB e JPEG em `OcorrenciasController.cs:19-24`; teto da requisição em `:15`. Faixa da leitura em `ValidadorLeitura` (`Contrato.cs:106`). Testes: `ApiTestes.RecusasELeituraAceita` (`fora-da-faixa`), `VetoresTestes.LimitesDaJanelaEDaFaixa`, `ApiTestes.CadastroRecusaEntradaInvalida`, `ApiTestes.UploadRecusaOQueNaoEJpegENomeComDoisPontos` | E07–E08 |
| C5 | Registrar `/demo/semear` apenas em Demo; em Production responder 404. Demonstrar que a mesma rota funciona em Demo para evitar falso positivo de rota inexistente | `DemoController.cs:20`: fora do ambiente Demo a ação retorna 404. No ambiente Demo o mesmo caminho grava, se houver leitura ao vivo. Testes: `ApiTestes.DemoSemearForaDoAmbienteDemoResponde404` e `ApiTestes.DemoSemearExigeLeituraAoVivoEAncoraNela` | E09 e `seguranca/evidencias/demo-semear-404.txt` |
| C6 | Segredos fora dos arquivos versionados e dos logs; chave distinta por dispositivo, revogável; configuração protegida no IIS e ACL mínima. Conciliar cadastro de dispositivos e origem dos segredos sem duplicar chave em configuração pública | Semente de demonstração distinta em `SalaDb.cs:64-67` (vetor público do contrato, não chave de produção). `esp32/secrets.h` ignorado em `.gitignore:19`. `ChaveCoordenacao` não está no appsettings; o filtro lê a configuração em `ExigeChaveCoordenacaoAttribute.cs:16`. ACL do IIS e revisão de histórico: pendente na VM do Samuel. Teste do segredo alheio: `ApiTestes.SegredoDeOutraSalaNaoAssina`. Saída de `git check-ignore`: `seguranca/evidencias/git-check-ignore.txt` | E10 |
| C7 | GPO: RDP apenas grupo autorizado do AD, política de senha/bloqueio e firewall restrito à administração; auditoria de logon e da pasta de fotos (política + SACL), NTFS só permite escrita ao pool e administradores; logs protegidos | Pendente na VM do Samuel. O repositório tem o roteiro `infra/06-gpo.ps1`; esta pasta não aplica GPO. R9 permanece crítico até a evidência E11 | E11, na VM |
| C8 | Reduzir superfície de rede: HTTPS para usuários, SQL somente interno, RDP somente rede administrativa, MQTT de saída sem abrir listener no servidor. LED local sem internet e aviso de leitura desatualizada; não fabricar dados atuais | HTTPS só em Production: `Program.cs:62`. Cota das rotas de escrita: `Program.cs:12` (30 por minuto por IP, 429 em `Program.cs:14`). Teste: `ApiTestes.EscritaAcimaDeTrintaPorMinutoResponde429`. Certificado, firewall, nmap, listener MQTT e LED: pendente na VM do Samuel | E12 e E13, na VM; a cota está na suíte |
| C9 | Nenhum dado de aluno: foto de sala/equipamento vazio, texto sem nomes, painel sem imagens e revisão antes de publicar. Minimizar metadados, excluir cópias locais de teste ao encerrar e revisar retenção antes de uso real | O upload regrava o JPEG sem APP1 em `OcorrenciasController.cs:28` (`SemApp1` em `:68`). Teste: `ApiTestes.UploadRemoveSegmentoApp1` (o arquivo devolvido conserva `JFIF` e não contém `GPS-TESTE` nem o marcador APP1). O app pede `exif: false` em `ocorrencia.tsx:36` e orienta em `:84` e `:91`. Isso não impede um rosto na imagem: a revisão humana continua obrigatória | E14 |
| C10 | Autenticar e autorizar ações, revogação e cadastro de sala na API; menor privilégio nas contas/pool/SQL. Foto e ocorrência do professor não usam a chave da coordenação | Filtro `ExigeChaveCoordenacaoAttribute.cs:20`: cabeçalho `X-Chave-Coordenacao`, comparação em tempo constante, 401 `chave-invalida` se faltar, se divergir ou se a configuração estiver vazia. Aplicado em `AmbientesController.cs:17`, `AcoesController.cs:30` e `DispositivosController.cs:15`. Teste: `ApiTestes.CoordenacaoSemChaveNaoGravaELeituraContinuaComHmac` (401 sem gravar; `POST /leituras` e `POST /ocorrencias` seguem sem a chave). Identificação no aparelho (`app/services/professor.ts:19`) não é essa chave. Contas, pool e SQL: pendente na VM do Samuel. `GET` da foto continua só com o id | E15 |

Não há obrigação de acrescentar autenticação AD ao app nesta entrega documental. Há obrigação de declarar a limitação e restringir o laboratório enquanto a autorização da API não estiver comprovada. Qualquer mudança de rota ou DTO requer ajustar os comandos do roteiro, os consumidores e o contrato na integração.

## 6. Riscos residuais e plano de tratamento

| Risco residual | Tratamento e responsável funcional | Prazo/gatilho | Condição de encerramento |
|---|---|---|---|
| Chave de demonstração visível no Wokwi (R7) | Coordenação/backend: aceitar apenas durante avaliação isolada com dados simulados; chave única por dispositivo, revogação após avaliação. Para instalação física, provisionar chave fora do fonte em NVS e proteger acesso físico | Revogar imediatamente após avaliação de 19/11/2026 ou antes se houver abuso | Mensagem assinada com a chave antiga é recusada; nenhuma chave de produção usada no Wokwi |
| Broker público sem SLA/controle de publicação (R8) | Dispositivo/backend: aceitar só no laboratório; LED autônomo, aviso de desatualização e POST /leituras como alternativa de demonstração. Migrar para broker autenticado com TLS e limites antes de uso operacional | Antes de qualquer piloto com decisões reais | Queda ensaiada sem dado antigo parecer atual; migração e disponibilidade reavaliadas |
| Foto pode conter pessoa apesar do aviso (R6) | Equipe/app: evitar coleta; revisar cada imagem e texto antes da entrega; remover ocorrência indevida e cópias, registrar incidente sem reproduzir dado. Não usar rosto real para testar | Antes de qualquer envio, print ou publicação | Conjunto entregue sem identificação de aluno e sem metadados desnecessários |
| Upload, RDP e foto sem chave (R5/R9/R12) | Backend/infraestrutura: a API já recusa travessia, não-JPEG e escrita da coordenação sem chave. RDP, NTFS e o GET da foto sem chave continuam. Não expor o serviço fora do laboratório até a VM | Antes de disponibilizar a API fora do laboratório | E11 na VM; foto continua acessível a quem souber o id |
| Exclusão não remove credencial antiga (R10) | Responsável pelo repositório: revisar histórico sem divulgar valores; se houve exposição, revogar primeiro e tratar cópias/histórico | Antes de publicar repositório e a cada suspeita | Chave exposta inutilizada; revisão documentada |

A coordenação mantém a decisão humana; alertas apoiam o protocolo escolar e não garantem proteção à saúde. A revogação existe na API e exige a chave. A autonomia do LED e a GPO ainda dependem da VM para sustentar qualquer redução de R9.

## 7. Coleta, revisão e entrega

O roteiro [EVIDENCIAS.md](EVIDENCIAS.md) define E01–E15. As saídas que não precisam da VM estão em [evidencias/](evidencias/): suíte `dotnet test`, log das recusas de leitura, upload `../x.jpg`, 404 de `/demo/semear` fora do Demo e `git check-ignore esp32/secrets.h`. nmap, IIS, GPO e LED ficam para o Samuel na VM. Um 404 de rota que não existe não substitui o 404 do ambiente Demo, já coberto pelo teste.

Incorporar o roteiro preenchido como anexo deste relatório e exportar **um único PDF**, com capa, matriz, controles, riscos residuais e evidências legíveis. Fazer um envio por equipe na tarefa de Cybersecurity do Teams até **19/11**, conforme o briefing, página 28. Completar o RM e conferir a composição da equipe antes da exportação; somente Samuel Baecker foi informado para esta versão.

## 8. Referências internas

- `docs/briefing-fiap.txt:551`: Cybersecurity, página 28; critérios de ativos/ameaças (2,5), vulnerabilidades/riscos (3,0), controles (2,5) e evidências técnicas (2,0).
- `docs/PLANO.md:70`: contrato entre partes; `:109`: recusas; `:192`: núcleo de Cybersecurity; `:251`: armadilhas.
- Arquivos de implementação e linhas identificados na seção 5, inspecionados em 05/11/2026.
