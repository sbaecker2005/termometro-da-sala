# Evidências — preencher na VM

Nenhuma evidência foi coletada por este roteiro. Use somente dados simulados; recorte senhas, chaves, tokens e detalhes privados. Guarde prints em `infra/evidencias/` quando existirem; não crie prints vazios. Registre resultado real, inclusive falhas. Datas dos logs IIS são UTC.

As saídas de comando de E01 a E12 saem prontas do `10-coletar-evidencias.ps1` (README, passo 10), um arquivo por item em `infra/evidencias/AAAA-MM-DD/`. Em cada "Print/saída" abaixo, coloque o link desse arquivo e o print de tela que o comando não cobre.

Equipe/RMs: [preencher]  
Data/fuso do ensaio: [preencher]  
Versão Windows/avaliação e hipervisor: [preencher]  
IP do servidor/cliente: [preencher]  
Link repositório e vídeo (duração): [preencher]  
RDS completo exigido pelo professor? Resposta/data: [preencher]

Para cada item abaixo: cole a saída sem segredos, link do print, horário e interpretação (esperado/obtido).

## E01 — VM, hostname e rede (2 pontos com E02/E03)

```powershell
hostname
Get-ComputerInfo | Select-Object WindowsProductName,WindowsVersion,OsBuildNumber
ipconfig /all
Get-NetIPInterface -AddressFamily IPv4 | Select-Object InterfaceAlias,Dhcp
```

Print/saída: [colar]. Esperado: Windows Server 2022, SRV-TERMOMETRO, IP fixo, DNS da VM; registrar recursos da VM e disco no host.

## E02 — DNS e alcance a partir do cliente

```powershell
nslookup api.termometro.local
nslookup 192.168.56.10
Test-NetConnection api.termometro.local -Port 80
Test-NetConnection api.termometro.local -Port 443
```

Print/saída: [colar]. Substitua IP pelo real. Esperado: A e PTR corretos, `TcpTestSucceeded=True`, certificado válido/confiável no navegador.

## E03 — AD, usuários e grupos

```powershell
Get-ADDomain | Select-Object DNSRoot,NetBIOSName,DistinguishedName
Get-ADUser coordenador -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf
Get-ADUser admin.termometro -Properties Enabled,MemberOf | Select-Object SamAccountName,Enabled,MemberOf
Get-ADGroupMember Coordenacao
Get-ADGroupMember AdminsTermometro
Get-ADGroupMember -Identity S-1-5-32-555
```

Print/saída: [colar]. Esperado: termometro.local, contas habilitadas, associações corretas, `AdminsTermometro` (e não `Coordenacao`) em `Remote Desktop Users` e cliente no domínio.

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

Print/saída: [colar]. Esperado: ANCM V2, Microsoft.AspNetCore.App 8.x, binding, ApplicationPoolIdentity, Started/AlwaysRunning, idle 0 e preload True. Nomes no pool: `ConnectionStrings__Sala`, `Fotos__Pasta`, `ChaveCoordenacao`, `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_HTTPS_PORT`, `Banco__Migrar` (e `Mqtt__Desligado` se tiver sido gravado). Não capture valores. Print de Application Initialization e Rapid-Fail Protection: [colar].

## E05 — pastas e NTFS

```powershell
icacls C:\Sites\Termometro
icacls C:\Dados\Termometro\Fotos
Get-SmbShare | Select-Object Name,Path
```

Print/saída: [colar]. Esperado: pool RX no site e M em fotos; SYSTEM/Administrators F; sem permissão de escrita de grupos escolares, sem SMB/diretório virtual para fotos. Teste como coordenador negado e upload real pelo pool permitido: [colar]. ACL de filhos preexistentes conferida: [preencher].

## E06 — SQL e publicação

```powershell
Get-Service 'MSSQL$SQLEXPRESS'
Invoke-WebRequest https://api.termometro.local/ -UseBasicParsing | Select-Object StatusCode
Invoke-WebRequest https://api.termometro.local/ambientes -UseBasicParsing | Select-Object StatusCode
```

Print/saída: [colar]. As duas URLs precisam de HTTP 200: o painel em `/` e a lista em `/ambientes`. Registre a versão publicada e um registro que continua no banco depois de reciclar o pool. Via ferramenta SQL com autenticação integrada administrativa, sem exibir string privada nem a coluna Segredo:

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

Esperado: ONLINE, login/usuário do pool, reader/writer sem db_owner, e as migrações `20261105025544_Inicial`, `20261105033613_Enums`, `20261105050515_SegredosPorDispositivo`. Print/saída: [colar].

## E07 — GPO, RDP e auditoria (1 ponto com E12)

```powershell
Get-GPO -All | Select-Object DisplayName,GpoStatus
Get-ADDefaultDomainPasswordPolicy
gpresult /r
auditpol /get /subcategory:*
Get-Acl C:\Dados\Termometro\Fotos -Audit | Select-Object -ExpandProperty Audit
Get-NetFirewallRule -Name TermometroRDP | Get-NetFirewallAddressFilter
```

Print/saída: [colar]. Print GPMC com link/escopo, direito RDP exclusivo para AdminsTermometro (e, no `gpresult /h`, a GPO que venceu esse direito no DC), NLA, inatividade 600 s e auditoria avançada sucesso/falha: [colar]. RDP autorizado e coordenador recusado; Security 4624/4625 e falha de acesso à foto 4656; 4663 com `WriteData` no envio da foto pela API e com `ReadData` na leitura por `GET /ocorrencias/{id}/foto`: [colar]. Confirme ausência de outra regra de firewall que amplie RDP: [preencher].

## E08 — DFS e acesso remoto

```powershell
Get-DfsnRoot
Get-DfsnFolder -Path '\\termometro.local\Escola\*'
Get-DfsnFolderTarget -Path '\\termometro.local\Escola\Manuais'
```

Print/saída e leitura de `\\termometro.local\Escola\Manuais` como coordenador: [colar]. Esperado: um destino, leitura permitida, gravação de usuário negada. Fotos não fazem parte do DFS. RDP administrativo restrito ou, se exigido, coleção RDS em VM membro e sessão real: [colar e identificar qual foi demonstrado].

## E09 — monitoramento (2 pontos)

```powershell
logman query Termometro-Alertas
logman query Termometro-Recursos
schtasks /query /tn Termometro-Monitoramento-Boot /v /fo LIST
```

Prints dos três contadores com caminhos localizados, limites, ação `Termometro-Recursos` no disparo e estado Running: [colar]. BLG/histórico: [link]. Evento de alerta de cada recurso no ensaio separado (contador, limite, valor, provedor e hora): [colar]. Tarefa `Termometro-Monitoramento-Boot`, reinicialização e os dois conjuntos em Running sem intervenção: [colar]. Limites temporários removidos e operacionais restaurados: [preencher].

## E10 — recuperação do pool (1 ponto com E11)

Print/saída do 09 parte Pool: [colar]. PID antes: [preencher]; depois: [preencher]; horários: [preencher]. Evento WAS e HTTP 200 posterior em `/ambientes` e em `/`: [colar]. Confirmação de que não houve pedido externo nem início manual durante a espera: [preencher]. Rapid-Fail Protection e número de crashes no intervalo: [preencher].

## E11 — recuperação W3SVC

```powershell
sc.exe qfailure W3SVC
sc.exe qfailureflag W3SVC
Get-CimInstance Win32_Service -Filter "Name='W3SVC'" | Select-Object Name,State,ProcessId,ServiceType
Get-Service WAS,W3SVC
```

Print de isolamento do PID e saída do 09 parte Servico: [colar]. PID antes/depois e tempo real: [preencher]. SCM falha/recuperação + HTTP 200 em `/` e `/ambientes` sem intervenção: [colar]. Se isolamento ou recuperação falhar, registrar aqui e corrigir antes da entrega: [preencher].

## E12 — correlação de logs

```powershell
Get-WinEvent -FilterHashtable @{LogName='System'; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 60 | Select-Object TimeCreated,ProviderName,Id,Message
Get-WinEvent -LogName Security -FilterXPath "*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= 900000]] and EventData[Data[@Name='LogonType']='10']]" -MaxEvents 30 | Select-Object TimeCreated,Id,Message
Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4625; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 30 | Select-Object TimeCreated,Id,Message
Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4656,4663; StartTime=(Get-Date).AddMinutes(-15)} -MaxEvents 30 | Select-Object TimeCreated,Id,Message
```

Selecione só eventos do ensaio antes de publicar saídas. Prints System, Application, Security e trecho de `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log` com URL/sc-status e hora UTC: [colar]. Mostre causa, PID/serviço e requisição após recuperação; não prometa IDs que o Windows não gerou.

## E13 — funcionamento completo (2 pontos)

Print/trecho do vídeo: [colar]. Painel em `/` com sala, nível e previsão; `GET /ambientes`; leitura por MQTT (NAT na 1883) ou por `POST /leituras` com `Mqtt__Desligado=true`; ocorrência ou ação ainda presente depois de reciclar o pool; foto só por `GET /ocorrencias/{id}/foto`. O HMAC `segredo-de-teste-sala-0N` é de demonstração: a troca é `POST /dispositivos/{id}/revogar` e um dispositivo novo, sem exibir segredo nem chave. Origem dos dados simulados/públicos: [preencher].

Não marque E13 só com o processo no ar. As duas URLs, a migração e uma leitura gravada fazem parte desta prova.

## Conferência final

- [ ] Ficha, bairro, link, equipe e RMs preenchidos.
- [ ] E01–E13 com provas reais e sem segredos.
- [ ] Sintaxe dos dez scripts aprovada, ensaio completo na VM e `10-coletar-evidencias.ps1` rodado no servidor e no cliente (E02).
- [ ] RDS mínimo confirmado com professor, ou implantação completa evidenciada.
- [ ] Vídeo acessível, duração até 7 min e fluxo real completo.
- [ ] Um integrante enviou o formulário de entrega (link do repositório e do vídeo) até 19/11 às 23h55.
