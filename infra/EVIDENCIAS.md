# Evidências — Operating Systems

Este arquivo registra o que o ensaio de 16/11/2026 e o vídeo de demonstração (6 min 39 s) comprovam. Cada item traz **Mostrado no vídeo** (com o minuto) e **Arquivo de saída / print no repositório**. Onde a segunda linha diz "não coletado", o comando está listado para rodar na VM com `10-coletar-evidencias.ps1` (README, passo 10), que grava um arquivo por item em `infra/evidencias/AAAA-MM-DD/`. Essa pasta ainda não existe, porque nenhuma saída foi coletada. Só dados simulados; recortar senhas, chaves, tokens e detalhes privados. Datas dos logs IIS são UTC.

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
**Arquivo de saída / print no repositório:** não coletado (`Get-Website`, `Get-WebBinding`, `dotnet --list-runtimes`, `processModel`).
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
**Arquivo de saída / print no repositório:** não coletado (`Get-Service`, HTTP 200 em `/` e `/ambientes`, consulta ao `__EFMigrationsHistory`).
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
**Arquivo de saída / print no repositório:** não coletado (`Get-GPO`, `gpresult`, `auditpol`, eventos 4624, 4625, 4656 e 4663, regras de firewall do RDP).

## E08 — DFS e acesso remoto

```powershell
Get-DfsnRoot
Get-DfsnFolder -Path '\\termometro.local\Escola\*'
Get-DfsnFolderTarget -Path '\\termometro.local\Escola\Manuais'
```

**Mostrado no vídeo (05:50 a 06:01):** citado na fala: os manuais ficam no namespace DFS baseado em domínio e o usuário acessa pelo nome do domínio, não pelo do servidor. RDP administrativo restrito demonstrado em 01:50 a 02:16; coleção RDS não demonstrada.
**Arquivo de saída / print no repositório:** não coletado (`Get-DfsnRoot`, `Get-DfsnFolderTarget`, leitura de `\\termometro.local\Escola\Manuais` como coordenador). Fotos não fazem parte do DFS.

## E09 — monitoramento (2 pontos)

```powershell
logman query Termometro-Alertas
logman query Termometro-Recursos
schtasks /query /tn Termometro-Monitoramento-Boot /v /fo LIST
```

**Mostrado no vídeo (03:26 a 04:12):** `Termometro-Recursos` gravando processador, espaço livre em disco e memória disponível a cada 15 s em arquivo BLG, já no terceiro arquivo (a coleta começou antes do dia da gravação); `Termometro-Alertas` com os três alertas (processador acima de 80%, disco abaixo de 15% livre, memória abaixo de 512 MB); tarefa agendada que sobe os conjuntos no boot. O vídeo mostra só a configuração; nenhum alerta disparando.
**Arquivo de saída / print no repositório:** não coletado (`logman query`, `schtasks`, BLG, evento de disparo de cada recurso com contador, limite, valor e hora). Como nenhum disparo foi provocado, não há limite temporário a restaurar.

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
**Arquivo de saída / print no repositório:** não coletado.

Cobertura esperada de E13: painel em `/` com sala, nível e previsão; `GET /ambientes`; leitura por MQTT (NAT na 1883) ou por `POST /leituras` com `Mqtt__Desligado=true`; ocorrência ou ação ainda presente depois de reciclar o pool; foto só por `GET /ocorrencias/{id}/foto`. O HMAC `segredo-de-teste-sala-0N` é de demonstração: a troca é `POST /dispositivos/{id}/revogar` e um dispositivo novo, sem exibir segredo nem chave. Origem dos dados: leitura simulada enviada durante a gravação, assinada com o segredo de demonstração; nenhum dado público de rua foi carregado na tabela de leituras do servidor.

Não marque E13 só com o processo no ar. As duas URLs, a migração e uma leitura gravada fazem parte desta prova.

## Conferência final

- [ ] Ficha, bairro (a ficha vale para todas as disciplinas), link, equipe e RMs preenchidos.
- [ ] E01–E13 com arquivos de saída e prints no repositório, sem segredos (hoje há só o registro do ensaio e o vídeo).
- [ ] Sintaxe dos dez scripts aprovada, ensaio completo na VM e `10-coletar-evidencias.ps1` rodado no servidor e no cliente (E02).
- [ ] RDS mínimo confirmado com professor, ou implantação completa evidenciada.
- [ ] Vídeo publicado com acesso liberado ao professor e testado em aba anônima (duração 6 min 39 s).
- [ ] Um integrante enviou o link do repositório e o do vídeo na tarefa do Teams da disciplina até 19/11 às 23h55.
