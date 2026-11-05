# Vídeo de Operating Systems — até 7 minutos

Meta: **6 min 50 s**, com 10 s de margem. Prepare janelas, comandos, dados simulados e evidências antes de gravar; não mostre senhas nem telas de configuração privada. Não grave instalação ou reinicialização da VM. Todos os integrantes devem saber explicar o ambiente. Não apresente template como solução pronta.

| Tempo | Mostrar e explicar | Evidência |
|---|---|---|
| 0:00–0:25 | Nome, equipe/RMs, ficha, calor na sala e papel da coordenação. Descrever a VM isolada | README preenchido |
| 0:25–1:10 | Windows Server 2022; hostname, IP fixo, DNS e domínio; resolução e conexão pelo cliente | `hostname`, `ipconfig /all`, `Get-ADDomain`, `nslookup`, `Test-NetConnection` |
| 1:10–1:50 | Usuários/grupos, senha/bloqueio, RDP restrito e auditoria. Mostrar uma permissão concedida e uma negação já ensaiadas | AD, GPMC, 4624/4625, tentativa negada ao coordenador |
| 1:50–2:35 | IIS: runtime, site, binding HTTPS, pool sempre ligado; pasta privada com NTFS; SQL local | ANCM/.NET 8, AlwaysRunning/preload/idle 0, ACL, serviço SQL |
| 2:35–2:55 | File Server e DFS; acesso remoto mínimo e sua limitação | Abrir `\\termometro.local\Escola\Manuais` pelo cliente; RDP restrito. Se houver RDS completo, mostrar coleção |
| 2:55–3:40 | Três alertas operacionais no perfmon, histórico e evento de ensaio; explicar que limite foi reduzido no ensaio separado | CPU >80%, disco <15%, memória <512 MB; evento com contador/valor/horário, BLG |
| 3:40–4:25 | Matar o worker específico; mostrar outro PID automaticamente e HTTP 200; Rapid-Fail Protection | Script 09 parte Pool, appcmd, evento WAS; sem abrir navegador durante espera |
| 4:25–5:20 | Mostrar política de restart do W3SVC e processo isolado; matar e observar PID novo/Running e retorno HTTP | `sc.exe qfailure`, 09 parte Servico, eventos SCM |
| 5:20–6:25 | Fluxo real: painel com sala/leitura/previsão; ocorrência/ação pela aplicação, persistência e foto privada; explicar dados simulados | Painel/API + SQL em uso + auditoria 4663/4656. Conferir retomada MQTT se implementada |
| 6:25–6:50 | Correlacionar falha/retorno no System e logs IIS, mostrar evidências e link de entrega | Data/hora, URL/sc-status, EVIDENCIAS preenchido |

Se a recuperação demorar, use cortes identificados com horários/PIDs visíveis e intervalo real declarado. Não substitua falha real por reinício manual nem esconda erro. Se faltar aplicação/painel, a gravação final fica pendente: o template serve apenas para ensaio do hosting. Publique o vídeo em modo acessível ao professor, confira duração e permissão e cole o link em EVIDENCIAS.md. Envie link do repositório no Teams por um único integrante.
