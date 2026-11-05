# Vídeo de Operating Systems — até 7 minutos

Meta: **6 min 50 s**, com 10 s de margem. Prepare janelas, comandos, dados simulados e evidências antes de gravar; não mostre senhas, a chave da coordenação nem o segredo HMAC. Não grave instalação ou reinicialização da VM. Todos os integrantes devem saber explicar o ambiente. O painel da prova é `/`; a lista da API é `/ambientes`.

| Tempo | Mostrar e explicar | Evidência |
|---|---|---|
| 0:00–0:25 | Nome, equipe/RMs, ficha, calor na sala e papel da coordenação. Descrever a VM isolada | README preenchido |
| 0:25–1:10 | Windows Server 2022; hostname, IP fixo, DNS e domínio; resolução e conexão pelo cliente | `hostname`, `ipconfig /all`, `Get-ADDomain`, `nslookup`, `Test-NetConnection` |
| 1:10–1:50 | Usuários/grupos, senha/bloqueio, RDP restrito e auditoria. Mostrar uma permissão concedida e uma negação já ensaiadas | AD, GPMC, 4624/4625, tentativa negada ao coordenador |
| 1:50–2:35 | IIS: runtime, site, binding HTTPS, pool sempre ligado; variáveis do pool só pelos nomes; pasta privada com NTFS; SQL local e as três migrações | ANCM/.NET 8, AlwaysRunning/preload/idle 0, ACL, `Banco__Migrar=false`, serviço SQL |
| 2:35–2:55 | File Server e DFS; acesso remoto mínimo e sua limitação | Abrir `\\termometro.local\Escola\Manuais` pelo cliente; RDP restrito. Se houver RDS completo, mostrar coleção |
| 2:55–3:40 | Três alertas operacionais no perfmon, histórico e evento de ensaio; explicar que limite foi reduzido no ensaio separado | CPU >80%, disco <15%, memória <512 MB; evento com contador/valor/horário, BLG |
| 3:40–4:25 | Matar o worker específico; mostrar outro PID automaticamente e, só então, `/ambientes` e o painel `/` em HTTP 200 | Script 09 parte Pool, appcmd, evento WAS; sem abrir navegador durante a espera |
| 4:25–5:20 | Mostrar política de restart do W3SVC e processo isolado; matar e observar PID novo/Running; de novo `/` e `/ambientes` | `sc.exe qfailure`, 09 parte Servico, eventos SCM |
| 5:20–6:25 | Fluxo: painel com sala, leitura e previsão; ocorrência ou ação; dado ainda lá depois de reciclar o pool; foto só pelo id. Leitura pelo MQTT se houver NAT na 1883, ou `POST /leituras` no Insomnia se `Mqtt__Desligado=true`. Diga que o HMAC da semente é de demonstração e que a troca é revogar e criar outro dispositivo | Painel `/`, `/ambientes`, SQL e auditoria 4663/4656. Sem mostrar a chave nem o segredo |
| 6:25–6:50 | Correlacionar falha/retorno no System e logs IIS, mostrar evidências e link de entrega | Data/hora, URL/sc-status, EVIDENCIAS preenchido |

Se a recuperação demorar, use cortes identificados com horários/PIDs visíveis e intervalo real declarado. Não substitua falha real por reinício manual nem esconda erro. A recuperação só está mostrada quando o PID muda sozinho e, depois, `/` e `/ambientes` respondem. Publique o vídeo em modo acessível ao professor, confira duração e permissão e cole o link em EVIDENCIAS.md. Envie link do repositório no Teams por um único integrante.
