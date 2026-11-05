# Vídeo de Operating Systems — até 7 minutos

Meta: **6 min 30 s**. Prepare janelas, comandos e dados simulados antes de gravar; não mostre senhas, a chave da coordenação nem o segredo HMAC. Não grave instalação nem reinicialização da VM. Todos os integrantes devem saber explicar o ambiente. O painel é `/` e a lista da API é `/ambientes`, os dois em `https://api.termometro.local`.

| Tempo | Mostrar e explicar | Evidência |
|---|---|---|
| 0:00–0:20 | Nome, equipe e RMs, problema do calor na sala e papel da coordenação. VM isolada | README: Ficha e "Item da pontuação → onde está" |
| 0:20–1:30 | Rede e identidade: hostname, IP fixo, DNS e domínio vistos do cliente; grupos, senha e bloqueio; RDP aceito para `admin.termometro` e recusado para `coordenador` (já ensaiados) | `hostname`, `ipconfig /all`, `nslookup` (A e PTR), `Test-NetConnection` 80/443, `Get-ADDomain`, `Get-ADGroupMember`, GPMC, 4624/4625 |
| 1:30–2:20 | IIS: Hosting Bundle, site, binding HTTPS com certificado confiável, pool sempre ligado, variáveis do pool só pelos nomes; pasta de fotos com NTFS; SQL local com as três migrações | ANCM V2 e `dotnet --list-runtimes`, AlwaysRunning/preload/idle 0, `icacls`, `Banco__Migrar=false`, serviço SQL |
| 2:20–3:05 | Os três alertas no perfmon, o histórico e um evento do ensaio; explicar que o limite foi reduzido só no coletor de ensaio | CPU > 80%, disco < 15% livre, memória < 512 MB; evento 2031 com contador, valor e hora; BLG |
| 3:05–3:50 | Matar só o worker do pool; outro PID aparece sozinho e, depois, `/ambientes` e `/` respondem 200 | 09 parte Pool, `appcmd list wp`, WAS 5009; navegador fechado durante a espera |
| 3:50–4:40 | Política de restart do W3SVC e processo isolado; matar, ver PID novo em Running e de novo `/` e `/ambientes` | `sc.exe qfailure`, 09 parte Servico, SCM 7031/7034/7036 |
| 4:40–5:50 | Fluxo: painel com sala, leitura e previsão; ocorrência ou ação; o dado continua depois de reciclar o pool; foto só pelo id. Leitura pelo MQTT (NAT na 1883) ou `POST /leituras` no Insomnia. O HMAC da semente é de demonstração e a troca é revogar e cadastrar outro dispositivo | Painel `/`, `/ambientes`, SQL, 4663/4656 na pasta de fotos |
| 5:50–6:30 | Custom View "Termometro - acompanhamento" com a queda e a volta, e a mesma hora no `u_ex*.log` do IIS (UTC) | `visao-termometro.xml`, `sc-status` 200 depois da volta, EVIDENCIAS.md preenchido |

Se a recuperação demorar, corte com horários e PIDs visíveis e diga o intervalo real. A recuperação só conta quando o PID muda sozinho e `/` e `/ambientes` respondem depois; reinício manual não substitui a falha. Publique o vídeo com acesso para o professor, confira duração e permissão e cole o link em EVIDENCIAS.md e no formulário de entrega.
