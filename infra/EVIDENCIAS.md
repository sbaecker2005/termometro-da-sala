# Evidências — Operating Systems

Este arquivo registra o que o ensaio de 16/11/2026 e o vídeo de demonstração (6 min 39 s) comprovam. Cada item traz **Mostrado no vídeo** (com o minuto) e **Arquivo de saída / print no repositório**. Parte das provas já está no repositório: 15 prints e 1 arquivo de texto da VM `SRV-TERMOMETRO` em [`evidencias/vm-2026-11/`](evidencias/vm-2026-11/README.md) (E04, E06, E07, E08, E09 e E13). Onde a segunda linha diz "não coletado", o item continua sem print ou saída, e o comando está listado para rodar na VM com `10-coletar-evidencias.ps1` (README, passo 10), que grava um arquivo por item em `infra/evidencias/AAAA-MM-DD/` (essa pasta ainda não existe). Só dados simulados; recortar senhas, chaves, tokens e detalhes privados. Datas dos logs IIS são UTC.

Os minutos do vídeo vêm da transcrição da fala e podem variar alguns segundos. Link do vídeo: ainda não publicado.

Equipe/RMs: Guilherme Caseiro — RM 559765; Samuel Baecker — RM 559269; Lucas Villar — RM 560005  
Data/fuso do ensaio: 16/11/2026, fuso America/Sao_Paulo (UTC−03:00), ajustado na VM com `Set-TimeZone -Id 'E. South America Standard Time'`  
Versão Windows/avaliação e hipervisor: Windows Server 2022 Standard **Avaliação** (180 dias), Desktop Experience, build 20348; **VMware Workstation Pro 26H1**; VM com 2 vCPU, 6 GB RAM, disco NVMe 80 GB expansível, firmware UEFI sem Secure Boot  
IP do servidor/cliente: servidor `SRV-TERMOMETRO` em `192.168.16.10/24` (adaptador `Ethernet0`, gateway `192.168.16.2`, DNS apontando para si mesmo); cliente de laboratório = host do VMware pela VMnet8, `192.168.16.1`  
Link repositório e vídeo (duração): https://github.com/sbaecker2005/termometro-da-sala ; vídeo com 6 min 39 s (limite de 7 min), link ainda não publicado  
RDS completo exigido pelo professor? Sem resposta registrada. Foi implementado e demonstrado o RDP administrativo restrito a `AdminsTermometro`; a coleção RDS completa não foi implantada

Cada item abaixo traz os comandos da coleta, o que o vídeo mostrou e o que ficou sem arquivo no repositório.

## E01 — VM, hostname e rede (2 pontos com E02/E03)

```powershell
hostname
Get-ComputerInfo | Select-Object WindowsProductName,WindowsVersion,OsBuildNumber
ipconfig /all
Get-NetIPInterface -AddressFamily IPv4 | Select-Object InterfaceAlias,Dhcp
```

**Mostrado no vídeo (00:31 a 00:49):** servidor `SRV-TERMOMETRO` com IP fixo `192.168.16.10/24`, controlador do domínio e DNS. Os recursos da VM (2 vCPU, 6 GB, disco de 80 GB) estão no cabeçalho deste arquivo.
**Arquivo de saída / print no repositório:** não coletado (`ipconfig /all`, `hostname`, `Get-ComputerInfo`).

## E02 — DNS e alcance a partir do cliente

```powershell
nslookup api.termometro.local
nslookup 192.168.16.10
Test-NetConnection api.termometro.local -Port 80
Test-NetConnection api.termometro.local -Port 443
```

**Mostrado no vídeo:** o nome resolve para o endereço e o endereço de volta para o nome, com a zona reversa e o PTR criados (00:49 a 00:54); HTTPS com certificado emitido para `api.termometro.local` e confiável, sem aviso do navegador (02:16 a 02:26).
**Arquivo de saída / print no repositório:** não coletado (`nslookup` direto e reverso, `Test-NetConnection` nas portas 80 e 443).

## E03 — AD, usuários e grupos

```powershell
Get-ADDomain | Select-Object DNSRoot,NetBIOSName,DistinguishedName
Get-ADUser coordenador -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf
Get-ADUser admin.termometro -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf
Get-ADGroupMember Coordenacao
Get-ADGroupMember AdminsTermometro
Get-ADGroupMember -Identity S-1-5-32-555
```

**Mostrado no vídeo (00:44 a 01:07):** domínio `termometro.local`; grupos `Coordenacao` (quem usa o sistema) e `AdminsTermometro` (quem administra); conta de serviço `svc.sql`, que não é pessoa e não faz logon interativo.
**Cliente:** o cliente de laboratório é o host do VMware pela VMnet8 (`192.168.16.1`); o acesso remoto foi testado a partir dele. Ingresso de um cliente no domínio: não demonstrado.
**Arquivo de saída / print no repositório:** não coletado (`Get-ADDomain`, `Get-ADUser`, `Get-ADGroupMember`).

## E04 — IIS e dependências (2 pontos com E05/E06)

```powershell
Get-WindowsFeature Web-Server,Web-AppInit,FS-FileServer,FS-DFS-Namespace
Import-Module WebAdministration
Get-Website | Select-Object Name,Id,State,PhysicalPath,ApplicationPool,Bindings
Get-WebBinding -Name Termometro
Get-WebGlobalModule | Where-Object Name -eq AspNetCoreModuleV2
dotnet --list-runtimes
Get-WebAppPoolState TermometroPool
Get-Item IIS:\AppPools\TermometroPool | Select-Object Name,startMode,managedRuntimeVersion
Get-ItemProperty IIS:\AppPools\TermometroPool -Name processModel
Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/sites/site[@name='Termometro']/application[@path='/']" -Name preloadEnabled
(Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/applicationPools/add[@name='TermometroPool']" -Name environmentVariables).Collection.name
```

**Mostrado no vídeo (02:16 a 02:49):** site em HTTPS; pool com `AlwaysRunning` e tempo de inatividade 0 (02:26 a 02:33); runtime do ASP.NET Core instalado (02:33 a 02:44). O nome e o caminho físico do site não foram citados na fala.
**Prints no repositório:**
- [E04-iis-binding.png](evidencias/vm-2026-11/E04-iis-binding.png): Site Bindings com `http` na porta 80 e `https` na porta 443, ambos com host `api.termometro.l…` (nome cortado na coluna) e IP `*`. Interpretação: o site está ligado por nome nas portas 80 e 443 (o print mostra a configuração, não respostas); o nome completo aparece nas URLs de E06 e E13.
- [E04-iis-pool.png](evidencias/vm-2026-11/E04-iis-pool.png): Advanced Settings de `TermometroPool`: .NET CLR Version `No Managed Code`, Managed Pipeline Mode `Integrated`, Start Mode `AlwaysRunning`, Identity `ApplicationPoolIdentity`, Idle Time-out `0`. Interpretação: pool próprio, sem runtime gerenciado (compatível com ASP.NET Core pelo ANCM, que este print não mostra), identidade virtual por pool, partida automática e sem descarte por inatividade.

**Ainda sem prova:** nome e caminho físico do site, estado `Started` do pool, ANCM V2 e runtime `dotnet`, Application Initialization (`preloadEnabled`), Rapid-Fail Protection, nomes das variáveis do pool e `Get-WindowsFeature` (`Get-Website`, `Get-WebBinding`, `dotnet --list-runtimes`, `processModel`).
Esperado na coleta: ANCM V2, Microsoft.AspNetCore.App 8.x, binding, ApplicationPoolIdentity, Started/AlwaysRunning, idle 0 e preload True. Nomes no pool: `ConnectionStrings__Sala`, `Fotos__Pasta`, `ChaveCoordenacao`, `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_HTTPS_PORT`, `Banco__Migrar` (e `Mqtt__Desligado` se tiver sido gravado). Não capture valores. Print de Application Initialization e Rapid-Fail Protection: não coletado.

## E05 — pastas e NTFS

```powershell
icacls C:\Sites\Termometro
icacls C:\Dados\Termometro\Fotos
Get-SmbShare | Select-Object Name,Path
```

**Mostrado no vídeo:** nada sobre pasta do site, pasta de fotos nem permissões NTFS; a fala não cita `icacls`.
**Arquivo de saída / print no repositório:** não coletado (`icacls`, `Get-SmbShare`, teste de acesso negado ao coordenador, upload de foto pelo pool, ACL dos filhos preexistentes).
Configuração prevista em `04-iis.ps1` e `08-publicar-api.ps1`: pool com leitura e execução no site e Modify nas fotos; SYSTEM e Administrators com Full; sem SMB nem diretório virtual para fotos.

## E06 — SQL e publicação

```powershell
Get-Service 'MSSQL$SQLEXPRESS'
Invoke-WebRequest https://api.termometro.local/ -UseBasicParsing | Select-Object StatusCode
Invoke-WebRequest https://api.termometro.local/ambientes -UseBasicParsing | Select-Object StatusCode
```

**Mostrado no vídeo:** SQL Server Express em execução e banco com as cinco migrações aplicadas (02:44 a 02:54), com `Banco__Migrar=false` e o script idempotente aplicado à parte (02:54 a 03:06); SQL rodando em conta virtual `NT SERVICE\MSSQL$SQLEXPRESS` (03:06 a 03:21), com a API conectando localmente; `GET /ambientes` (05:45).
**Prints no repositório:**
- [E06-certificado.png](evidencias/vm-2026-11/E06-certificado.png): Edge em `https://api.termometro.local` com o Certificate Viewer aberto: emitido para `api.termometro.local` e emitido por `api.termometro.local` (autoassinado), válido de 16/11/2026 às 20:52:37 a 16/11/2027 às 21:12:37, com a impressão digital SHA-256. Interpretação: HTTPS com certificado próprio do nome do site. Como titular e emissor são iguais, a confiança depende de o certificado estar instalado como raiz confiável no cliente, o que este print não mostra (a barra de endereço aparece com cadeado e a página abre sem tela de aviso).
- [E06-painel.png](evidencias/vm-2026-11/E06-painel.png): mural em `https://api.termometro.local`: Sala 1 Atenção 28.5 °C, Sala 2 Atenção 31.4 °C, Sala 3 Cuidado extremo 35.3 °C, Sala 4 Cuidado extremo 40.5 °C; a coluna "Nível previsto" repete o nível com "sem dado da rua: mantém a última leitura válida da sala"; texto "Atualiza a cada 30 segundos". Interpretação: a publicação no IIS responde em `/` com dados das salas. Os valores acompanham os de `/ambientes` (ver E13).

**Ainda sem prova:** `Get-Service 'MSSQL$SQLEXPRESS'`, HTTP 200 por comando, consulta ao `__EFMigrationsHistory`, usuário e papéis do pool no banco, registro que sobrevive à reciclagem do pool.
As duas URLs precisam de HTTP 200: o painel em `/` e a lista em `/ambientes`. Registre a versão publicada e um registro que continua no banco depois de reciclar o pool. Via ferramenta SQL com autenticação integrada administrativa, sem exibir string privada nem a coluna Segredo:

```sql
SELECT name, state_desc FROM sys.databases WHERE name = N'Termometro';
USE Termometro;
SELECT name, type_desc FROM sys.database_principals WHERE name = N'IIS APPPOOL\TermometroPool';
SELECT r.name AS papel, u.name AS usuario
FROM sys.database_role_members m
JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
WHERE u.name = N'IIS APPPOOL\TermometroPool';
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
```

Esperado: ONLINE, login/usuário do pool, reader/writer sem db_owner, e as migrações `20261105025544_Inicial`, `20261105033613_Enums`, `20261105050515_SegredosPorDispositivo`, `20261107033211_AlertaFrio`, `20261107091447_MapaTermico`.

## E07 — GPO, RDP e auditoria (1 ponto com E12)

```powershell
Get-GPO -All | Select-Object DisplayName,GpoStatus
Get-ADDefaultDomainPasswordPolicy
gpresult /r
auditpol /get /subcategory:*
Get-Acl C:\Dados\Termometro\Fotos -Audit | Select-Object -ExpandProperty Audit
Get-NetFirewallRule -Name TermometroRDP | Get-NetFirewallAddressFilter
```

**Mostrado no vídeo:** a GPO `Termometro-Seguranca` ligada na unidade organizacional Domain Controllers, e não na raiz do domínio, porque uma política ligada no domínio perde para a política dos controladores (01:16 a 01:32); os três direitos: acesso remoto permitido só a `AdminsTermometro` e administradores, logon local negado a `svc.sql`, acesso remoto negado a `svc.sql` e convidados (01:32 a 01:50); teste do host pela VMnet8: `admin.termometro` entra e `coordenador` é recusado por autorização, com a senha correta e sem evento de senha errada (01:50 a 02:16); auditoria de logon e de arquivo, sucesso e falha, ligada por GPO (06:01 a 06:05).
**Prints e saída no repositório:**
- [E07-gpo-link.png](evidencias/vm-2026-11/E07-gpo-link.png): GPMC, `termometro.local`, aba Linked Group Policy Objects: ordem 1 `Default Domain Policy` e ordem 2 `Termometro-Segura…` (nome cortado), Enforced `No`, Link Enabled `Yes`, GPO Status `Enabled`. Interpretação: a GPO de segurança está ligada e habilitada **no nó do domínio**. O vídeo cita a ligação na unidade Domain Controllers (01:16 a 01:32); este print mostra o domínio, então o local da ligação precisa ser conferido antes de ser afirmado.
- [E07-direitos.png](evidencias/vm-2026-11/E07-direitos.png): editor da GPO `Termometro-Seguranca`, User Rights Assignment: *Allow log on through Remote Desktop Services* = `Administrators,TERMOMETRO\A…` (cortado); *Deny log on locally* = `TERMOMETRO\svc.sql`; *Deny log on through Remote Desktop Services* = `Guests,TERMOMETRO\svc.sql`. As outras linhas visíveis estão Not Defined. Interpretação: RDP só para administradores e o grupo cortado (que o vídeo diz ser `AdminsTermometro`), e conta de serviço sem logon local nem remoto.
- [E07-auditoria.png](evidencias/vm-2026-11/E07-auditoria.png): Advanced Audit Policy Configuration > Audit Policies > Logon/Logoff: *Audit Logon* = `Success and Failure`; as outras subcategorias da tela (Logoff, Account Lockout, Special Logon etc.) estão `Not Configured`. Interpretação: gera 4624 (sucesso) e 4625 (falha). A auditoria de acesso a arquivos (Object Access) não aparece neste print.
- [E07-rdp-permitido.png](evidencias/vm-2026-11/E07-rdp-permitido.png): sessão RDP em `192.168.16.10`; no PowerShell, `whoami` = `termometro\admin.termometro`, `hostname` = `SRV-TERMOMETRO`, `qwinsta` com `rdp-tcp#0` da conta `admin.termometro` `Active` (e `console` do `Administrator` também ativo). Interpretação: a conta administrativa entra por RDP.
- [E07-rdp-negado.png](evidencias/vm-2026-11/E07-rdp-negado.png): cliente RDP com "The connection was denied because the user account is not authorised for remote log-in." O print não mostra a conta usada nem o destino; a atribuição à conta `coordenador` vem do vídeo (01:50 a 02:16), não da imagem.
- [E07-logons.txt](evidencias/vm-2026-11/E07-logons.txt): evento 4624 de 17/11/2026 21:09:48, Logon Type `10` (RDP), conta `TERMOMETRO\admin.termometro`, origem `192.168.16.1`, workstation `SRV-TERMOMETRO`, pacote Negotiate. Interpretação: o log de segurança registra um logon RDP da conta administrativa vindo do host do VMware pela VMnet8. O horário é o impresso na saída (fuso não consta do arquivo) e nenhum print o liga ao instante da sessão de `E07-rdp-permitido.png`.

**Ainda sem prova:** evento 4625 da recusa (o RDP negado não tem evento em arquivo), 4656 e 4663 de arquivo (auditoria de objeto não aparece), `Get-GPO`, `gpresult`, `auditpol`, política de senha, regra de firewall do RDP.

## E08 — DFS e acesso remoto

```powershell
Get-DfsnRoot
Get-DfsnFolder -Path '\\termometro.local\Escola\*'
Get-DfsnFolderTarget -Path '\\termometro.local\Escola\Manuais'
```

**Mostrado no vídeo (05:50 a 06:01):** citado na fala: os manuais ficam no namespace DFS baseado em domínio e o usuário acessa pelo nome do domínio, não pelo do servidor. RDP administrativo restrito demonstrado em 01:50 a 02:16; coleção RDS não demonstrada.
**Print no repositório:** [E08-dfs.png](evidencias/vm-2026-11/E08-dfs.png): DFS Management com o namespace `\\termometro.local\Escola` (Domain-based in Windows Server 2008 mode) e 1 entrada, a pasta `Manuais`. Interpretação: namespace baseado em domínio, endereçado por `\\termometro.local\Escola`, e não pelo nome do servidor.
**Ainda sem prova:** destino da pasta (`Get-DfsnFolderTarget`), aba Namespace Servers e leitura de `\\termometro.local\Escola\Manuais` como coordenador. Fotos não fazem parte do DFS.

## E09 — monitoramento (2 pontos)

```powershell
logman query Termometro-Alertas
logman query Termometro-Recursos
schtasks /query /tn Termometro-Monitoramento-Boot /v /fo LIST
```

**Mostrado no vídeo (03:26 a 04:12):** `Termometro-Recursos` gravando processador, espaço livre em disco e memória disponível a cada 15 s em arquivo BLG, já no terceiro arquivo (a coleta começou antes do dia da gravação); `Termometro-Alertas` com os três alertas (processador acima de 80%, disco abaixo de 15% livre, memória abaixo de 512 MB); tarefa agendada que sobe os conjuntos no boot. O vídeo mostra só a configuração; nenhum alerta disparando.
**Prints no repositório:**
- [E09-coletor-contadores.png](evidencias/vm-2026-11/E09-coletor-contadores.png): propriedades de `Termometro-Recursos`: contadores `\Processor(_Total)\% Processor Time`, `\LogicalDisk(C:)\% Free Space` e `\Memory\Available MBytes`; Log format `Binary`; Sample interval `15` segundos; arquivo `Termometro-Recursos_000003.blg` visível ao fundo. Interpretação: coleta dos três contadores a cada 15 s, já no terceiro arquivo.
- [E09-alertas-limites.png](evidencias/vm-2026-11/E09-alertas-limites.png): `logman query "Termometro-Alertas"`: Status `Running`, Root Path `C:\PerfLogs\Termometro\`, Run as `SYSTEM`, tipo `Alert`, intervalo 15 s, Event Log `On`, limites `Processor Time>80`, `% Free Space<15` e `Available MBytes<512`. Interpretação: os três alertas pedidos, com gravação no log de eventos.
- [E09-alertas.png](evidencias/vm-2026-11/E09-alertas.png): Performance Monitor, Data Collector Sets > User Defined, com `Termometro-Alertas` (tipo Alert) e `Termometro-Recursos`. Interpretação: os dois conjuntos existem; a coluna Output do alerta está vazia.
- [E09-boot.png](evidencias/vm-2026-11/E09-boot.png): Task Scheduler, tarefa `Termometro-…` (nome cortado) com status `Ready` e gatilho `At system startup`. Interpretação: tarefa agendada para o boot; o print não mostra a ação nem o conjunto que ela inicia.

**Ainda sem prova:** alerta disparando (evento com contador, limite, valor e hora), `logman query Termometro-Recursos`, `schtasks /query /v` com nome completo e ação, conteúdo do BLG. Como nenhum disparo foi provocado, não há limite temporário a restaurar.

## E10 — recuperação do pool (1 ponto com E11)

Executado em 16/11/2026 com `.\09-teste-de-falha.ps1 -Parte Pool -ProbeUri https://api.termometro.local/ambientes -LaboratorioConfirmado`.

| Campo | Valor |
|---|---|
| PID do worker antes | 7336 |
| PID do worker depois | 7332 |
| Estado do pool ao fim | `Started` |
| HTTP antes e depois | 200 em `/ambientes` e em `/` (exigido pelo próprio script, que falha se não vier 200) |
| Horário | 16/11/2026, por volta das 22h40 (America/Sao_Paulo); minuto exato não registrado |

O script derruba apenas o processo `w3wp.exe` do `TermometroPool`, espera o WAS criar outro e só então repete as duas requisições; não houve `Start-Service` nem reinício manual durante a espera. O navegador foi fechado antes do ensaio, de modo que nenhuma requisição externa pudesse iniciar o worker e invalidar a prova de inicialização automática.

Houve uma execução anterior no mesmo dia (PID 5068 → 7336) feita com o navegador ainda aberto no painel, que atualiza a cada 30 s; ela **não** é usada como prova, justamente por não garantir a ausência de requisição externa.

**Mostrado no vídeo (04:12 a 04:45):** o worker do pool é morto à força, sem parar o serviço; o PID muda sozinho, com o navegador fechado, e a aplicação volta a responder HTTP 200. Os valores dos PIDs do vídeo não constam da transcrição; os PIDs 7336 e 7332 acima são da execução de 16/11.
**Arquivo de saída / print no repositório:** não coletado (saída do script, evento WAS, Rapid-Fail Protection, número de falhas no intervalo).

## E11 — recuperação W3SVC

```powershell
sc.exe qfailure W3SVC
sc.exe qfailureflag W3SVC
Get-CimInstance Win32_Service -Filter "Name='W3SVC'" | Select-Object Name,State,ProcessId,ServiceType
Get-Service WAS,W3SVC
```

**Resultado: não executado. Limitação registrada, conforme [TESTE-DE-FALHA.md](TESTE-DE-FALHA.md).**

Nesta build do Windows Server 2022, o `W3SVC` roda dentro do grupo compartilhado de serviços do IIS (`svchost`/`iissvcs`). O script 09 recusa a parte `Servico` nessa condição — com razão, porque matar aquele PID derrubaria todo o grupo e não provaria recuperação do W3SVC.

Seguindo o procedimento documentado, tentou-se o isolamento:

```powershell
sc.exe config W3SVC type= own
Restart-Computer
```

Depois do reinício o serviço **não voltou a subir**. `Start-Service W3SVC` falhou e o log System registrou, entre 22:25:48 e 22:34:39 de 16/11/2026:

- **7001** — "The World Wide Web Publishing Service service depends on the Windows Process Activation Service service which failed to start because of the following error: The service did not respond to the start or control request in a timely fashion."
- **7024** — término com erro "The dependency service or group failed to start."
- **7031** — término inesperado, repetido **137 vezes**, cada um seguido de "The following corrective action will be taken in 5000 milliseconds: Restart the service."

Ou seja: isolado do grupo, o W3SVC não consegue iniciar junto do WAS, e o SCM reaplicou a ação de reinício a cada 5 s por cerca de nove minutos até esgotar o ciclo.

A reversão prevista no procedimento resolveu:

```powershell
sc.exe config W3SVC type= share
Restart-Computer
```

Às 22:35:36 o log registra "The W3SVC service entered the running state", e `https://api.termometro.local/ambientes` voltou a responder 200.

**Interpretação.** A parte 2 não foi demonstrada porque o isolamento exigido por ela não é viável nesta build. Não se substituiu o teste por `Stop-Service` nem por `iisreset`: ambos são paradas planejadas e não comprovam recuperação por falha, como o próprio procedimento adverte. A prova de recuperação da entrega é a parte 1 (E10), que derruba o worker do pool e demonstra o WAS criando outro sem intervenção.

Como efeito colateral útil, os 137 eventos 7031 acima são prova direta de que a **política de recuperação do serviço configurada no passo 7 funciona**: o SCM executou "Restart the service" com atraso de 5000 ms, repetidamente, exatamente como `sc.exe qfailure W3SVC` declara. A origem foi a tentativa de isolamento, não uma falha da aplicação.

**Mostrado no vídeo (06:29 a 06:35):** só o fechamento da fala: política de recuperação configurada no serviço, com limitação documentada. O teste do W3SVC não foi feito no vídeo, e nenhuma saída de `sc.exe qfailure` aparece nele. A recuperação comprovada em vídeo é a do pool (E10).
**Consulta ao professor** sobre a exigência da parte 2: sem resposta registrada.

**Arquivo de saída / print no repositório:** não coletado (`sc.exe qfailure W3SVC`, `sc.exe qfailureflag W3SVC`, eventos 7031, 7024 e 7001 da janela acima). Os números e horários acima vêm da leitura do log System feita no ensaio.

## E12 — correlação de logs

```powershell
Get-WinEvent -FilterHashtable @{LogName='System'; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 60 | Select-Object TimeCreated,ProviderName,Id,Message
Get-WinEvent -LogName Security -FilterXPath "*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= 900000]] and EventData[Data[@Name='LogonType']='10']]" -MaxEvents 30 | Select-Object TimeCreated,Id,Message
Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4625; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 30 | Select-Object TimeCreated,Id,Message
Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4656,4663; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 30 | Select-Object TimeCreated,Id,Message
```

Selecione só eventos do ensaio antes de publicar saídas. **Mostrado no vídeo (06:01 a 06:11):** a visão personalizada do Visualizador de Eventos (`visao-termometro.xml`), que junta a queda e a volta do serviço e a auditoria com o log do IIS. Em 01:50 a 02:16, o RDP aceito e o recusado.
**Arquivo de saída / print no repositório:** não coletado (eventos 4624, 4625, 4656 e 4663, visão personalizada, trecho do `u_ex*.log` com `sc-status` 200).
Na coleta: System, Application, Security e trecho de `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log` com URL/sc-status e hora UTC. Mostre causa, PID/serviço e requisição após recuperação; não prometa IDs que o Windows não gerou.

**Janela a explicar ao revisar o log System.** Entre 22:25:48 e 22:34:39 de 16/11/2026 há uma sequência de 137 eventos 7031 do W3SVC ("terminated unexpectedly... It has done this N time(s)"), com 7024 e 7001 intercalados. Não é instabilidade do servidor nem da aplicação: é a tentativa de isolar o W3SVC descrita em E11, revertida às 22:35:36. Quem recortar esse trecho sem a explicação lê um servidor quebrado; quem o apresentar com a explicação mostra a política de recuperação do SCM funcionando.

## E13 — funcionamento completo (2 pontos)

**Mostrado no vídeo (04:49 a 05:50):** painel pensado para a TV do corredor, atualizado a cada 30 s, com o nível escrito além da cor; leitura enviada com HMAC-SHA256 e segredo por dispositivo, que a API recusa sem assinatura válida e quando repetida ou atrasada; a sala 2 mudou de nível; `GET /ambientes` com as salas e suas dimensões. A coluna de previsão apareceu como "sem dado de rua" (05:28): fora do modo de demonstração, sem leituras externas na tabela, a API não inventa previsão. A ocorrência continuando depois de reciclar o pool não foi mostrada.
**Prints no repositório:**
- [E06-painel.png](evidencias/vm-2026-11/E06-painel.png): painel em `/` com as quatro salas, nível, índice e nível previsto (detalhes em E06).
- [E13-ambientes.png](evidencias/vm-2026-11/E13-ambientes.png): Edge em `https://api.termometro.local/ambientes` com o JSON das quatro salas (`sala-01` a `sala-04`): índices 28.5, 31.4, 35.3 e 40.5; `nivelAtual` `atencao` nas salas 1 e 2 e `cuidado` nas salas 3 e 4 (o mural escreve "Cuidado extremo"); `ultimaLeituraEm` `2026-11-17T23:23:18Z` nas quatro; `semLeitura` `false`; `dispositivoId` `esp32-sala-01` a `esp32-sala-04`. Interpretação: a API responde por HTTPS com as quatro salas e leituras gravadas, e os valores batem com o mural.

**Ainda sem prova:** leitura entrando por MQTT ou `POST /leituras` (os prints mostram o resultado, não o envio), ocorrência que sobrevive à reciclagem do pool, foto por `GET /ocorrencias/{id}/foto`, app no celular, LED sem broker, HTTP 200 por comando e migrações no banco.

Cobertura esperada de E13: painel em `/` com sala, nível e previsão; `GET /ambientes`; leitura por MQTT (NAT na 1883) ou por `POST /leituras` com `Mqtt__Desligado=true`; ocorrência ou ação ainda presente depois de reciclar o pool; foto só por `GET /ocorrencias/{id}/foto`. O HMAC `segredo-de-teste-sala-0N` é de demonstração: a troca é `POST /dispositivos/{id}/revogar` e um dispositivo novo, sem exibir segredo nem chave. Origem dos dados: leitura simulada enviada durante a gravação, assinada com o segredo de demonstração; nenhum dado público de rua foi carregado na tabela de leituras do servidor.

Não marque E13 só com o processo no ar. As duas URLs, a migração e uma leitura gravada fazem parte desta prova.

## Conferência final

- [ ] Ficha, bairro (a ficha vale para todas as disciplinas), link, equipe e RMs preenchidos.
- [ ] E01–E13 com arquivos de saída e prints no repositório, sem segredos. Já há prints de E04, E06, E07, E08, E09 e E13 em `evidencias/vm-2026-11/`; faltam E01, E02, E03, E05, E10 a E12 e as saídas listadas em "Ainda sem prova" de cada item.
- [ ] Sintaxe dos dez scripts aprovada, ensaio completo na VM e `10-coletar-evidencias.ps1` rodado no servidor e no cliente (E02).
- [ ] RDS mínimo confirmado com professor, ou implantação completa evidenciada.
- [ ] Vídeo publicado com acesso liberado ao professor e testado em aba anônima (duração 6 min 39 s).
- [ ] Um integrante enviou o link do repositório e o do vídeo na tarefa do Teams da disciplina até 19/11 às 23h55.
