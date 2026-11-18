# Vídeo de Operating Systems — até 7 minutos

Meta: **6 min 30 s**. Prepare janelas, comandos e dados simulados antes de gravar; não mostre senhas, a chave da coordenação nem o segredo HMAC. Não grave instalação nem reinicialização da VM. Todos os integrantes devem saber explicar o ambiente. O painel é `/` e a lista da API é `/ambientes`, os dois em `https://api.termometro.local`.

Ambiente real do ensaio: VMware Workstation Pro 26H1; servidor `SRV-TERMOMETRO` em `192.168.16.10/24`, gateway `192.168.16.2`; cliente de laboratório = host pela VMnet8, `192.168.16.1`.

## Antes de apertar o rec

- [ ] Uma leitura já gravada, para o painel não abrir com quatro "sem leitura".
- [ ] Janelas abertas e posicionadas: PowerShell elevado, IIS Manager, GPMC, perfmon, DFS Management, navegador, Visualizador de Eventos com a Custom View.
- [ ] Navegador **fechado** no momento do teste de falha (requisição externa inicia o worker e invalida a prova).
- [ ] `Set-ADUser admin.termometro -ChangePasswordAtLogon $false` já feito, senão o RDP recusa por troca de senha e não pela GPO.
- [ ] Windows Update adiado; nada de reinício no meio da gravação.

| Tempo | Mostrar e explicar | Evidência |
|---|---|---|
| 0:00–0:20 | Nome, equipe e RMs, problema do calor na sala e papel da coordenação. VM isolada em VMware, rede NAT, sem bridge na rede doméstica | README: Ficha e "Item da pontuação → onde está" |
| 0:20–1:30 | Rede e identidade: hostname, IP fixo `192.168.16.10`, DNS e domínio vistos do cliente; zona reversa com PTR; grupos, política de senha e bloqueio; RDP aceito para `admin.termometro` e recusado para `coordenador` | `hostname`, `ipconfig /all`, `nslookup` (A e PTR), `Test-NetConnection` 80/443, `Get-ADDomain`, `Get-ADGroupMember` (inclusive `-Identity S-1-5-32-555`), GPMC com o direito de RDP, 4624/4625 |
| 1:30–2:20 | IIS: Hosting Bundle, site, binding HTTPS com certificado confiável, pool sempre ligado, variáveis do pool **só pelos nomes**; pasta de fotos com NTFS; SQL local com as cinco migrações: Inicial, Enums, SegredosPorDispositivo, AlertaFrio e MapaTermico | ANCM V2 e `dotnet --list-runtimes`, AlwaysRunning/preload/idle 0, `icacls`, `Banco__Migrar=false`. Dizer que o SQL roda em **conta virtual** (`NT Service\MSSQL$SQLEXPRESS`), que é o padrão de menor privilégio recomendado e sem senha a gerenciar — conta de domínio só seria necessária para acessar recurso de rede, e a API conecta local em `.\SQLEXPRESS`. `svc.sql` existe como identidade de serviço com logon interativo e RDP negados |
| 2:20–3:05 | Os três alertas no perfmon, o histórico e um evento do ensaio; explicar que o limite foi reduzido só no coletor de ensaio | CPU > 80%, disco < 15% livre, memória < 512 MB; evento com contador, valor e hora; BLG |
| 3:05–4:00 | **Teste de falha, parte 1.** Matar só o worker do pool; outro PID aparece sozinho e, depois, `/ambientes` e `/` respondem 200. Dizer em voz alta os dois PIDs e a hora | `09-teste-de-falha.ps1 -Parte Pool`, `appcmd list wp`, evento WAS; navegador fechado durante a espera |
| 4:00–4:40 | **Recuperação do W3SVC: política configurada e limitação registrada.** Mostrar `sc.exe qfailure W3SVC` (3 reinícios, 5000 ms, contador zerado em 1 dia). Explicar que nesta build o W3SVC roda no grupo compartilhado do IIS e que `type= own` o impede de iniciar, então a parte 2 **não foi executada** — e que não se trocou o teste por `Stop-Service`, que é parada planejada. Mostrar no log System a janela de 16/11 em que o SCM reaplicou o reinício de 5 s repetidamente: é prova direta de que a política dispara | `sc.exe qfailure`/`qfailureflag`, eventos 7031/7024/7001, EVIDENCIAS.md item E11 |
| 4:40–5:50 | Fluxo completo: painel com sala, nível e índice (mural só de leitura, atualiza a cada 30 s); ocorrência pelo app ou ação pelo desktop; o dado continua depois de reciclar o pool; foto só pelo id. Leitura por MQTT (NAT na 1883) ou `POST /leituras`. Dizer que "sem dado da rua" é o comportamento previsto fora do modo Demo: nada alimenta `LeiturasRua` no servidor, então o mural mantém a última leitura válida e **declara** a ausência em vez de exibir previsão velha ou inventada. O HMAC da semente é de demonstração; a troca é revogar e cadastrar outro dispositivo | Painel `/`, `/ambientes`, SQL, 4663 (`WriteData` no envio e `ReadData` na leitura da foto) e 4656 na negação |
| 5:50–6:30 | DFS: `\\termometro.local\Escola\Manuais` aberto como coordenador. Custom View "Termometro - acompanhamento" com a queda e a volta, e a mesma hora no `u_ex*.log` do IIS (UTC) | `Get-DfsnFolder`, `visao-termometro.xml`, `sc-status` 200 depois da volta, EVIDENCIAS.md preenchido |

## Como falar da limitação sem perder ponto

Trinta segundos, nesta ordem: o que a entrega pede, o que foi tentado, o que o sistema respondeu, o que foi feito em vez disso, e por que não se forçou um teste falso. Um examinador desconta prova inventada; raramente desconta limitação medida, documentada e corretamente interpretada.

Se a recuperação do pool demorar, corte com horários e PIDs visíveis e diga o intervalo real. A recuperação só conta quando o PID muda sozinho e `/` e `/ambientes` respondem depois; reinício manual não substitui a falha. Publique o vídeo com acesso para o professor, confira duração e permissão e cole o link em EVIDENCIAS.md e no formulário de entrega.
