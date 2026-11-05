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

**Estado observado:** a API contém o exemplo WeatherForecast, sem rotas de negócio, persistência ou validação HMAC. `esp32/` e `infra/` contêm apenas `.gitkeep`. Há app com formulário e serviço preparado para API, mas o modo simulado está habilitado por padrão (`app/app.config.ts:4`; `app/services/api.ts:227`). Pacotes EF Core e MQTTnet instalados no projeto não comprovam controles em funcionamento. O teste `api/TermometroDaSala.Api.Tests/UnitTest1.cs:6` está vazio.

Assim, os cenários a seguir são vulnerabilidades potenciais da arquitetura, não resultados de exploração de serviços ativos. Os controles existentes são identificados com arquivo e linha; os demais estão marcados **a conferir na implementação**. Não há evidências de execução anexadas nesta versão.

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
| R3 — SQLi na busca | A1, A2 | Cliente envia apóstrofos e expressão booleana na pesquisa de ocorrências | Concatenar texto do usuário em SQL ou SQL bruto interpolado de forma insegura | Leitura ou alteração indevida de registros |
| R4 — LFI no download | A3, A7 | Cliente tenta `../`, caminho absoluto ou nome de arquivo do Windows no identificador da foto | Usar caminho fornecido pelo cliente em leitura de disco | Exposição de configuração, credenciais ou arquivos do servidor |
| R5 — upload malicioso | A2, A3, A7 | Cliente envia script como JPEG, nome com travessia ou arquivo grande | Confiar em extensão/MIME, permitir execução no diretório de imagens ou não limitar tamanho | Execução, sobrescrita de arquivos ou disco cheio |
| R6 — OSINT e LGPD | A7, A8 | Terceiro reconhece aluno, uniforme, placa, documento ou localização em foto pública | Fotografar pessoas, enviar EXIF/localização ou publicar fotos no mural/repositório | Identificação e exposição de menor de idade |
| R7 — segredo do Wokwi | A4, A5 | Visitante do projeto copia o segredo e assina novas mensagens daquele dispositivo | Link do simulador permite ver todos os arquivos, inclusive secrets.h | HMAC válido produzido por terceiro até revogação |
| R8 — negação de serviço | A2, A4 | Terceiro ocupa o tópico com lixo; broker público cai ou limita conexões | Dependência de serviço público sem garantia de capacidade; payloads sem limite | Falta de medições recentes e atraso na resposta da coordenação |
| R9 — acesso indevido/RDP | A1, A3, A6 | Conta comum ou atacante tenta logon remoto no servidor | RDP exposto, permissões amplas, senha fraca e auditoria desativada | Tomada do servidor e dos dados |
| R10 — credenciais versionadas | A1, A3, A5 | Leitor de repositório, histórico, print ou log recupera segredo | Segredo em configuração rastreada; acreditar que ignorar arquivo remove histórico | Forja de leituras, acesso ao banco ou servidor |
| R11 — semeadura indevida | A1, A2 | Cliente chama POST /demo/semear no ambiente de produção | Rota de carga simulada registrada em qualquer ambiente e sem HMAC | Histórico artificial influencia previsão real |
| R12 — operações administrativas sem controle | A2, A6, A7 | Cliente direto tenta registrar ação, revogar sensor ou ler foto sem autorização | Confundir identificação local do professor e middleware com autenticação/autorização | Decisões falsas, revogação indevida e acesso a imagens |

`UseAuthorization()` em `api/TermometroDaSala.Api/Program.cs:21` isoladamente não comprova proteção: não há autenticação configurada nem política de acesso nas rotas de negócio, que ainda não existem.

## 4. Matriz de risco: probabilidade × impacto

Probabilidade (P): 1 rara, 2 improvável, 3 possível, 4 provável, 5 quase certa. Impacto (I): 1 desprezível, 2 pequeno/reversível, 3 interrupção localizada, 4 perda relevante ou decisão operacional incorreta, 5 comprometimento amplo ou exposição de menores. Pontuação **P × I**: 1–4 baixo; 5–9 moderado; 10–16 alto; 17–25 crítico.

“Antes” estima o cenário sem o conjunto de controles, quando a funcionalidade estiver exposta. **“Depois” é meta estimada, condicionada à implementação e aprovação das evidências C1–C10; não representa risco já reduzido nesta versão.** A escala é julgamento qualitativo da equipe, sem frequência medida. O impacto pode continuar elevado mesmo quando a probabilidade diminui.

| ID | Antes P | Antes I | Antes P×I / classe | Controles | Depois P | Depois I | Depois P×I / classe |
|---|---:|---:|---|---|---:|---:|---|
| R1 | 4 | 4 | 16 alto | C1, C8 | 2 | 4 | 8 moderado |
| R2 | 4 | 4 | 16 alto | C1 | 1 | 4 | 4 baixo |
| R3 | 3 | 5 | 15 alto | C2, C4 | 1 | 5 | 5 moderado |
| R4 | 3 | 5 | 15 alto | C3, C10 | 1 | 5 | 5 moderado |
| R5 | 4 | 5 | 20 crítico | C3, C4, C7 | 2 | 5 | 10 alto |
| R6 | 4 | 5 | 20 crítico | C9, C10 | 2 | 5 | 10 alto |
| R7 | 5 | 4 | 20 crítico | C1, C6, C8 | 4 | 3 | 12 alto |
| R8 | 4 | 4 | 16 alto | C4, C8 | 4 | 3 | 12 alto |
| R9 | 4 | 5 | 20 crítico | C7, C10 | 2 | 5 | 10 alto |
| R10 | 3 | 5 | 15 alto | C6 | 1 | 5 | 5 moderado |
| R11 | 3 | 4 | 12 alto | C5 | 1 | 4 | 4 baixo |
| R12 | 4 | 5 | 20 crítico | C10 | 2 | 5 | 10 alto |

Não se aceita exposição real de menores nem produção aberta com risco alto. O recorte demonstrativo reduz o impacto de R7/R8 por usar dados simulados, decisão humana e sinalização local; fora desse recorte é necessário reavaliar a matriz.

## 5. Controles e comprovação

| ID | Controle e critério de aceitação | Onde está implementado / estado observado | Evidência a anexar |
|---|---|---|---|
| C1 | HMAC-SHA256 por dispositivo; bytes UTF-8 `ambienteId\|dispositivo\|temperaturaC\|umidadePct\|medidoEm`, temperatura com uma casa e cultura invariável, comparação em tempo constante. Validar vínculo com ambiente, dispositivo ativo, timestamp estritamente crescente e persistido; rejeitar mais de 10 min atrás ou 1 min à frente. HTTP e MQTT devem usar a mesma regra; atualização atômica impede duplicata concorrente | **a conferir na implementação** em API/ESP32; contrato previsto em `docs/PLANO.md:85`. Não existe ainda serviço compartilhado de ingestão | E02–E04: vetor comum, aceite válido, recusas `assinatura-invalida`, `reenvio`, `dispositivo-revogado`, `atrasada`; reinício e concorrência não duplicam |
| C2 | Consultas LINQ/EF parametrizadas; nunca concatenar busca em SQL. Parâmetro de usuário deve aparecer separado do texto do comando | **a conferir na implementação**; referências EF em `api/TermometroDaSala.Api/TermometroDaSala.Api.csproj:10` não demonstram consulta segura | E05: código da busca e comando SQL com parâmetros; busca maliciosa não amplia resultados nem altera dados |
| C3 | Gerar nome GUID no servidor; guardar imagem fora de diretório executável e servir por id de ocorrência, resolvido no banco; nunca aceitar caminho do cliente. GUID evita colisão, não substitui autorização | **a conferir na implementação**; o nome `foto.jpg` em `app/app/salas/[id]/ocorrencia.tsx:38` é apenas o nome enviado pelo cliente | E06–E07: imagem legítima, nome GUID em disco, código de resolução por id, tentativas de travessia sem conteúdo local |
| C4 | Validar na API ambiente existente, campos/tipos, temperatura -10 a 60 °C e umidade 0 a 100%; descrição 10–500 caracteres após trim; foto obrigatória, tamanho limitado, conteúdo de imagem decodificável e tipo permitido. Limite de bytes também na ingestão MQTT/HTTP | Parcial no cliente: `app/domain/ocorrencia.ts:6`, chamada em `app/app/salas/[id]/ocorrencia.tsx:54`; testes em `app/domain/ocorrencia.test.ts:5`. Validação na fronteira do servidor e limite numérico de upload: **a conferir na implementação** | E07–E08: erros 4xx para entrada inválida, falso JPEG e arquivo acima do limite configurado, sem gravação parcial |
| C5 | Registrar `/demo/semear` apenas em Demo; em Production responder 404. Demonstrar que a mesma rota funciona em Demo para evitar falso positivo de rota inexistente | **a conferir na implementação**; condição prevista em `docs/PLANO.md:161`. O exemplo atual não tem essa rota em nenhum ambiente | E09: inicialização e resposta nos dois ambientes, consulta ao histórico sem inserção em Production |
| C6 | Segredos fora dos arquivos versionados e dos logs; chave distinta por dispositivo, revogável; configuração protegida no IIS e ACL mínima. Conciliar cadastro de dispositivos e origem dos segredos sem duplicar chave em configuração pública | Parcial: `.gitignore:6` e `:19` ignoram configuração local, secrets.h e env; appsettings atuais não contêm chave. Armazenamento efetivo, exemplo sem chave real, rotação e ausência no histórico: **a conferir na implementação** | E10: regras de exclusão, nomes de arquivos revisados e declaração da revisão do histórico pelo responsável; nunca mostrar valor da chave |
| C7 | GPO: RDP apenas grupo autorizado do AD, política de senha/bloqueio e firewall restrito à administração; auditoria de logon e da pasta de fotos (política + SACL), NTFS só permite escrita ao pool e administradores; logs protegidos | **a conferir na implementação**; `infra/` sem configuração. Previsto em `docs/PLANO.md:196` | E11: gpresult, auditpol, ACL/SACL, logon negado e eventos 4625/4663 correlacionados |
| C8 | Reduzir superfície de rede: HTTPS para usuários, SQL somente interno, RDP somente rede administrativa, MQTT de saída sem abrir listener no servidor. LED local sem internet e aviso de leitura desatualizada; não fabricar dados atuais | Só redirecionamento HTTPS no exemplo: `api/TermometroDaSala.Api/Program.cs:19`; certificado, firewall, LED e recuperação: **a conferir na implementação** | E12: nmap de origem comum e administrativa, regras de firewall, E13: perda de conexão controlada sem inundar broker |
| C9 | Nenhum dado de aluno: foto de sala/equipamento vazio, texto sem nomes, painel sem imagens e revisão antes de publicar. Minimizar metadados, excluir cópias locais de teste ao encerrar e revisar retenção antes de uso real | Orientação em `app/app/salas/[id]/ocorrencia.tsx:84` e `:91`; câmera solicita `exif: false` em `:36`. Isso não prova remoção dos metadados do arquivo nem impede rosto na imagem. Revisão, tratamento no servidor e painel: **a conferir na implementação** | E14: formulário, foto artificial sem pessoas, inspeção do arquivo salvo e do painel; nenhum print com pessoa identificável |
| C10 | Autenticar e autorizar ações, revogação e acesso a fotos na API; menor privilégio nas contas/pool/SQL. Até existir controle verificável, manter demonstração em rede isolada e sem dados reais | **a conferir na implementação**; identificação no AsyncStorage (`app/services/professor.ts:19`) não é login; middleware isolado (`api/TermometroDaSala.Api/Program.cs:21`) não é prova | E15: sessão válida autorizada e acessos anônimo/sem permissão negados; tentativa não modifica banco |

Não há obrigação de acrescentar autenticação AD ao app nesta entrega documental. Há obrigação de declarar a limitação e restringir o laboratório enquanto a autorização da API não estiver comprovada. Qualquer mudança de rota ou DTO requer ajustar os comandos do roteiro, os consumidores e o contrato na integração.

## 6. Riscos residuais e plano de tratamento

| Risco residual | Tratamento e responsável funcional | Prazo/gatilho | Condição de encerramento |
|---|---|---|---|
| Chave de demonstração visível no Wokwi (R7) | Coordenação/backend: aceitar apenas durante avaliação isolada com dados simulados; chave única por dispositivo, revogação após avaliação. Para instalação física, provisionar chave fora do fonte em NVS e proteger acesso físico | Revogar imediatamente após avaliação de 19/11/2026 ou antes se houver abuso | Mensagem assinada com a chave antiga é recusada; nenhuma chave de produção usada no Wokwi |
| Broker público sem SLA/controle de publicação (R8) | Dispositivo/backend: aceitar só no laboratório; LED autônomo, aviso de desatualização e POST /leituras como alternativa de demonstração. Migrar para broker autenticado com TLS e limites antes de uso operacional | Antes de qualquer piloto com decisões reais | Queda ensaiada sem dado antigo parecer atual; migração e disponibilidade reavaliadas |
| Foto pode conter pessoa apesar do aviso (R6) | Equipe/app: evitar coleta; revisar cada imagem e texto antes da entrega; remover ocorrência indevida e cópias, registrar incidente sem reproduzir dado. Não usar rosto real para testar | Antes de qualquer envio, print ou publicação | Conjunto entregue sem identificação de aluno e sem metadados desnecessários |
| Upload, RDP e autorização dependem da integração (R5/R9/R12) | Backend/infraestrutura: não expor serviço a rede não confiável; aplicar controles C3/C4/C7/C10 e coletar provas positivas e negativas | Antes de disponibilizar a API fora do laboratório | Evidências E06–E08/E11/E15 aprovadas; restrições verificadas de outra máquina |
| Exclusão não remove credencial antiga (R10) | Responsável pelo repositório: revisar histórico sem divulgar valores; se houve exposição, revogar primeiro e tratar cópias/histórico | Antes de publicar repositório e a cada suspeita | Chave exposta inutilizada; revisão documentada |

A coordenação mantém a decisão humana; alertas apoiam o protocolo escolar e não garantem proteção à saúde. A autonomia do LED e a revogação ainda precisam ser implementadas e comprovadas para sustentar a redução de impacto indicada na matriz.

## 7. Coleta, revisão e entrega

O roteiro [EVIDENCIAS.md](EVIDENCIAS.md) define E01–E15, comandos, resultado esperado e espaços para prints. Todos os comandos são propostas de coleta no laboratório; não foram executados contra rede ou servidor nesta análise. Um 404 em rota inexistente, um formulário em modo simulado ou um teste vazio não comprova segurança.

Antes da entrega, conferir novamente as linhas após integrar a implementação e substituir **a conferir na implementação** apenas mediante inspeção e evidência. Se um controle continuar ausente, manter o estado explícito e o risco correspondente; não descrevê-lo como aplicado.

Incorporar o roteiro preenchido como anexo deste relatório e exportar **um único PDF**, com capa, matriz, controles, riscos residuais e evidências legíveis. Fazer um envio por equipe na tarefa de Cybersecurity do Teams até **19/11**, conforme o briefing, página 28. Completar o RM e conferir a composição da equipe antes da exportação; somente Samuel Baecker foi informado para esta versão.

## 8. Referências internas

- `docs/briefing-fiap.txt:551`: Cybersecurity, página 28; critérios de ativos/ameaças (2,5), vulnerabilidades/riscos (3,0), controles (2,5) e evidências técnicas (2,0).
- `docs/PLANO.md:70`: contrato entre partes; `:109`: recusas; `:192`: núcleo de Cybersecurity; `:251`: armadilhas.
- Arquivos de implementação e linhas identificados na seção 5, inspecionados em 05/11/2026.
