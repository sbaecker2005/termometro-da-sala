# Evidências da VM SRV-TERMOMETRO

Prints e uma saída de texto enviados pela equipe a partir da VM `SRV-TERMOMETRO` (domínio `termometro.local`, IP `192.168.16.10`). Cada item é interpretado em [../../EVIDENCIAS.md](../../EVIDENCIAS.md), que também lista o que ainda falta. Nada aqui foi renomeado ou editado.

| Arquivo | Item | O que prova |
|---|---|---|
| [E04-iis-binding.png](E04-iis-binding.png) | E04 | Bindings do site: `http` na porta 80 e `https` na 443, host `api.termometro.l…`, IP `*`. |
| [E04-iis-pool.png](E04-iis-pool.png) | E04 | `TermometroPool`: No Managed Code, Integrated, AlwaysRunning, ApplicationPoolIdentity, Idle Time-out 0. |
| [E06-certificado.png](E06-certificado.png) | E06 | Certificado autoassinado de `api.termometro.local`, válido de 16/11/2026 a 16/11/2027, com SHA-256. |
| [E06-painel.png](E06-painel.png) | E06, E13 | Mural em `https://api.termometro.local` com as quatro salas e "sem dado da rua" na previsão. |
| [E07-auditoria.png](E07-auditoria.png) | E07 | Advanced Audit Policy: Audit Logon em Success and Failure; demais subcategorias da tela Not Configured. |
| [E07-direitos.png](E07-direitos.png) | E07 | GPO `Termometro-Seguranca`: RDP permitido a administradores e grupo cortado; `svc.sql` negado em logon local e RDP. |
| [E07-gpo-link.png](E07-gpo-link.png) | E07 | GPO de segurança ligada e habilitada no nó `termometro.local`. |
| [E07-logons.txt](E07-logons.txt) | E07 | Evento 4624, 17/11/2026 21:09:48, Logon Type 10, `TERMOMETRO\admin.termometro`, origem `192.168.16.1`. |
| [E07-rdp-negado.png](E07-rdp-negado.png) | E07 | RDP recusado por conta não autorizada (a conta não aparece no print). |
| [E07-rdp-permitido.png](E07-rdp-permitido.png) | E07 | Sessão RDP de `admin.termometro` em `SRV-TERMOMETRO` (`whoami`, `hostname`, `qwinsta`). |
| [E08-dfs.png](E08-dfs.png) | E08 | Namespace DFS `\\termometro.local\Escola` com a pasta `Manuais`. |
| [E09-alertas-limites.png](E09-alertas-limites.png) | E09 | `logman query` do alerta: Running, 15 s, limites 80 % CPU, 15 % disco livre, 512 MB de memória. |
| [E09-alertas.png](E09-alertas.png) | E09 | Data Collector Sets com `Termometro-Alertas` e `Termometro-Recursos`. |
| [E09-boot.png](E09-boot.png) | E09 | Tarefa `Termometro-…` Ready com gatilho At system startup. |
| [E09-coletor-contadores.png](E09-coletor-contadores.png) | E09 | Contadores de CPU, disco e memória, formato Binary, 15 s, arquivo `..._000003.blg`. |
| [E13-ambientes.png](E13-ambientes.png) | E13 | JSON de `/ambientes` com as quatro salas, índices e `ultimaLeituraEm` 2026-11-17T23:23:18Z. |

## Não coberto por estes arquivos

nmap, eventos 4663/4656 de NTFS, `icacls`, `ipconfig`/`nslookup`/`Test-NetConnection`, eventos 7031/7024/7001, alerta disparando, `u_ex*.log`, app no celular, LED sem broker e reciclagem do pool.
