# Termômetro da Sala — Windows Server

## Ficha da CitizenTech

| Campo | Conteúdo |
|---|---|
| Nome da CitizenTech | Termômetro da Sala |
| Cidade e bairro | São Paulo, [bairro da escola entrevistada] |
| Cidadão beneficiado | Professor(a) e turma de uma sala sem climatização. Quem opera o sistema é a coordenação pedagógica |
| Problema, em uma frase | Nas tardes quentes, a sala passa do limite seguro sem que ninguém meça, e a escola só reage quando alguém passa mal |
| Como funciona | Um sensor na porta da sala mede o calor e acende verde, amarelo ou vermelho. O sistema prevê o nível da sala para daqui a 2 h. A coordenação vê todas as salas, decide e registra a ação tomada. O professor acompanha pelo app e registra ocorrências com foto |
| O que cada parte faz | ESP32 (Wokwi) mede e envia a leitura assinada; API (ASP.NET Core + SQL Server) valida, grava e prevê; modelo (Python, dados do INMET) prevê o índice de calor da rua; app (React Native + Expo) para o professor; desktop (Windows Forms) para a coordenação; Windows Server hospeda a API e o painel web no IIS |
| Link do repositório | https://github.com/sbaecker2005/termometro-da-sala |
| Indicador de impacto | Aulas no nível "Perigo" sem ação registrada (meta: zero) e minutos entre o alerta e a ação |
| ODS conectados | 3 (meta 3.d), 4 (meta 4.a), 11 (meta 11.b) e 13 (meta 13.1) |

Equipe (nome completo e RM de todos): [preencher].

## Item da pontuação → onde está

| Item da pontuação (briefing p. 34) | Pontos | Configuração | Prova em [EVIDENCIAS.md](EVIDENCIAS.md) |
|---|---:|---|---|
| Rede e identidade: IP fixo, hostname, DNS e domínio no AD | 2 | Passos 1–3; `01-rede.ps1`, `02-ad-dns.ps1`, `03-usuarios-grupos.ps1` | E01–E03: `hostname`, `ipconfig /all`, `nslookup` (A e PTR), `Test-NetConnection` 80/443, `Get-ADDomain`, `Get-ADGroupMember` |
| Aplicação no IIS: site, pool, dependências, pasta e NTFS | 2 | Passos 4, 5 e 8; `04-iis.ps1`, `05-sql.ps1`, `08-publicar-api.ps1`; HTTPS com certificado confiável na própria VM | E04–E06 |
| Monitoramento: alertas de CPU, disco e memória no Monitor de Desempenho | 2 | Passo 7; `07-monitoramento.ps1`, coletor de histórico e ensaio | E09 |
| Tolerância a falhas: recuperação do serviço e do pool, teste de falha | 1 | Passos 7 e 9; `07-monitoramento.ps1` (restart do W3SVC), pool sempre ligado, `09-teste-de-falha.ps1`, [TESTE-DE-FALHA.md](TESTE-DE-FALHA.md) | E10–E11 |
| Logs de auditoria e acompanhamento: Visualizador de Eventos e logs do IIS | 1 | Passos 6 e 7a; `06-gpo.ps1` (auditoria e SACL), [visao-termometro.xml](visao-termometro.xml), `u_ex*.log` do IIS | E07, E12 |
| Funcionamento completo da proposta | 2 | Passos 5 e 8; painel `/` e `GET /ambientes` servidos pelo IIS da VM, leitura nova por MQTT ou `POST /leituras` | E13 e vídeo ([ROTEIRO-VIDEO.md](ROTEIRO-VIDEO.md)) |
| Serviços do conceito: GPO, File Server, RDS e DFS | — | Passos 4, 6 e 6a | E07–E08 |

## Resumo do ambiente

| Campo | Valor no roteiro |
|---|---|
| Servidor | Windows Server 2022 (avaliação, Desktop Experience), VM isolada em Hyper-V ou VirtualBox |
| Hostname e IP fixo | `SRV-TERMOMETRO`, `192.168.56.10/24` (exemplo do roteiro; o IP real fica em E01) |
| Domínio e DNS | Floresta `termometro.local`; registro A `api.termometro.local` apontando para a VM; zona reversa com PTR |
| Grupos | `Coordenacao` (`coordenador`) e `AdminsTermometro` (`admin.termometro`, único grupo com RDP: membro de `Remote Desktop Users` e dono do direito na GPO) |
| IIS | Site `Termometro` em `C:\Sites\Termometro`, pool `TermometroPool` (No Managed Code, AlwaysRunning, idle 0, preload), Hosting Bundle .NET 8, HTTPS 443 em `https://api.termometro.local/` |
| Pasta privada | `C:\Dados\Termometro\Fotos`: entre as identidades da aplicação, só o pool tem Modify; SACL de auditoria (leitura e gravação); foto sai só por `GET /ocorrencias/{id}/foto` |
| Banco | SQL Server Express `.\SQLEXPRESS`, banco `Termometro`, login do pool só com `db_datareader` e `db_datawriter` |
| Alertas (a cada 15 s) | CPU acima de 80%, disco C: abaixo de 15% livre, memória abaixo de 512 MB disponíveis (`Termometro-Alertas`) |
| Recuperação | W3SVC: 3 reinícios com 5 s, contador zerado após 1 dia; pool: WAS sobe outro worker; Rapid-Fail Protection ligado |
| Eventos acompanhados | WAS 5002/5009/5011, SCM 7031/7034/7036, AspNetCore Module V2, Diagnosis-PLA 2031, Security 4624/4625/4656/4663 |

## Escopo

Roteiro para laboratório isolado, baseado no briefing, páginas 32–34. Os scripts preparam serviços; as etapas gráficas de GPO, DFS, certificados e coletores também são obrigatórias. Execute em Windows PowerShell 5.1 **como administrador dentro da VM**, na sequência abaixo. Nunca execute estes scripts no computador pessoal.

A API em `api/` grava pela connection string `ConnectionStrings:Sala` (variável `ConnectionStrings__Sala`) e guarda a foto em `Fotos:Pasta` (`Fotos__Pasta`). A coordenação envia `ChaveCoordenacao` no cabeçalho `X-Chave-Coordenacao`. O painel está em `wwwroot` e abre em `/`; `GET /ambientes` lista as salas. `Banco:Migrar` existe e o padrão é `true` (a API migraria sozinha na subida). Os scripts 04 e 08 gravam `Banco__Migrar=false` no pool: em produção quem migra é o administrador, no passo 5.

O segredo HMAC da semente (`segredo-de-teste-sala-01` a `sala-04`) é só de demonstração. Ele já está nas migrações e não serve como segredo real. Para trocar: `POST /dispositivos/{id}/revogar` com a chave da coordenação e um dispositivo novo. A API não tem rota para cadastrar sensor; a linha nova em `Dispositivos` entra pelo administrador, com segredo que não volta para o git.

Uma VM reúne AD, DNS, arquivos, IIS e SQL para caber no laboratório. As consequências disso estão em [Limitações](#limitações).

## 0. Criar a VM e preparar os arquivos

1. No Hyper-V: VM geração 2, Windows Server 2022 avaliação **Desktop Experience**, 2 vCPU, 6 GB RAM, disco expansível 80 GB. Guarde o VHDX, ISO e checkpoints em `D:` no host. No VirtualBox: equivalente, com rede host-only e, se necessário, uma segunda placa NAT temporária para instalar dependências. Após instalar, mantenha apenas a interface de laboratório; não use rede em bridge na rede doméstica/escola.
2. Reserve endereço livre na rede virtual, por exemplo `192.168.56.10/24`. O gateway deve existir nessa rede; não copie um gateway fictício. Hyper-V NAT e VirtualBox NAT Network podem fornecer um gateway real. Cliente e host precisam alcançar a rede da VM. Não abra portas no roteador.
3. Instale atualizações e transfira, de fontes oficiais, o **Hosting Bundle .NET 8 x64**, instalador offline SQL Server Express 2022 e, na máquina de desenvolvimento, SDK .NET 8. Não é necessário SDK no servidor. Use arquivos locais verificados por assinatura digital; os scripts não baixam instaladores.
4. Tire checkpoint antes da promoção AD e outro antes dos testes destrutivos. Checkpoint é para o laboratório, não substitui backup. Copie o repositório para `C:\GS\termometro` dentro da VM e abra PowerShell elevado em `C:\GS\termometro\infra`.
5. Use senhas próprias digitadas no prompt seguro; não grave transcrições de sessões com credenciais, nem inclua senhas, chaves, connection strings secretas ou arquivos privados nas evidências. Os caminhos `C:` abaixo são **da VM**, cujo disco fica em `D:` no host.

## 1. IP fixo e hostname

Pelo console da VM (a rede pode cair):

```powershell
Get-NetAdapter
.\01-rede.ps1 -InterfaceAlias 'Ethernet' -IPAddress 192.168.56.10 -PrefixLength 24 -Gateway 192.168.56.1
Restart-Computer
ipconfig /all
hostname
```

Troque os exemplos pela rede real. O script se recusa a remover outro IP fixo/gateway. O DNS passa a apontar para o próprio servidor, que ainda não responderá até a etapa 2. Se usar duas placas durante a instalação, remova a NAT antes de promover AD para não registrar endereço errado.

## 2. AD DS, domínio e DNS

```powershell
.\02-ad-dns.ps1 -ServerIP 192.168.56.10
Restart-Computer
# Entrar como TERMOMETRO\Administrator (nome pode variar com idioma).
.\02-ad-dns.ps1 -ServerIP 192.168.56.10
Get-ADDomain
nslookup api.termometro.local 192.168.56.10
```

Na primeira passagem, informe a senha DSRM e promova a floresta `termometro.local`; na segunda, crie o A `api` apontando para a VM. No console DNS, crie zona reversa da sub-rede e PTR do servidor para a prova de diagnóstico; verifique `nslookup 192.168.56.10`. Encaminhadores DNS são opcionais para resolução externa, não substituem o DNS do AD. No cliente Windows Pro/Enterprise, configure DNS `192.168.56.10`, ingresse no domínio e reinicie. Não há DNS público para esse domínio.

## 3. Usuários e grupos

```powershell
.\03-usuarios-grupos.ps1
Get-ADUser coordenador -Properties Enabled,MemberOf
Get-ADGroupMember Coordenacao
Get-ADGroupMember AdminsTermometro
Get-ADGroupMember -Identity S-1-5-32-555
```

`Coordenacao` recebe `coordenador`; `AdminsTermometro` recebe `admin.termometro`. Ambos devem trocar a senha no primeiro logon. Faça essa troca no console/cliente antes do primeiro RDP com NLA. O script também põe `AdminsTermometro` no grupo interno `Remote Desktop Users` (SID `S-1-5-32-555`, o mesmo em qualquer idioma), que dá a permissão de conexão no listener RDP; sem ele o RDP é recusado mesmo com o direito da GPO do passo 6. `coordenador` fica fora desse grupo. O grupo de RDP não é Domain Admins: para instalar serviços, use a conta administrativa separada. A conta da coordenação não ganha acesso direto às fotos; o acesso de usuário deve passar pela aplicação.

## 4. File Server, IIS, pool e Hosting Bundle

```powershell
.\04-iis.ps1
```

O script instala File Server e IIS com Application Initialization. Reserva `C:\Sites\Termometro` para publicação e `C:\Dados\Termometro\Fotos` para fotos privadas. Use pastas dedicadas vazias: a execução reaplica a ACL da raiz, removendo permissões extras. Não aponte para diretórios de outros sistemas. Verifique filhos preexistentes com ACL explícita; eles não são saneados automaticamente.

Permissões NTFS esperadas: pool `IIS AppPool\TermometroPool` com leitura/execução no site e Modify em fotos; SYSTEM e Administrators com FullControl para manutenção/recuperação. **Somente o pool escreve entre as identidades da aplicação**; usuários e grupos da escola não escrevem. Preserve a exceção administrativa. Não crie diretório virtual, compartilhamento SMB nem conteúdo em `wwwroot` apontando para fotos. O script grava `Fotos__Pasta` com o caminho dessa pasta (absoluto). A API usa o caminho como está quando ele é absoluto.

A chave não entra no git nem na linha de comando. Defina `ChaveCoordenacao` na sessão ou digite no prompt seguro. Sem NAT de saída para `broker.hivemq.com` na porta 1883, desligue o assinante:

```powershell
.\04-iis.ps1 -MqttDesligado
```

Com saída pela NAT, rode `.\04-iis.ps1` sem o switch. O mesmo switch vale no 08: sem ele, o 08 não muda `Mqtt__Desligado`. Para religar o MQTT depois de ter desligado, `$env:Mqtt__Desligado='false'` e rode o 04 ou o 08 de novo. Outra connection string (sem senha no arquivo) entra por `ConnectionStrings__Sala` na sessão; o padrão é `Server=.\SQLEXPRESS;Database=Termometro;Integrated Security=True;TrustServerCertificate=True`, o banco que o script 05 cria. Isso substitui o `TermometroDaSala` do `appsettings.json`.

Instale agora o Hosting Bundle .NET 8 x64. Se já foi instalado antes do IIS, execute Repair depois de instalar IIS. Reinicie a VM. Confira:

```powershell
Import-Module WebAdministration
Get-WebGlobalModule | Where-Object Name -eq AspNetCoreModuleV2
dotnet --list-runtimes
Get-WebAppPoolState TermometroPool
Get-Item IIS:\AppPools\TermometroPool | Select-Object name,startMode,managedRuntimeVersion
Get-ItemProperty IIS:\AppPools\TermometroPool -Name processModel
Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/sites/site[@name='Termometro']/application[@path='/']" -Name preloadEnabled
icacls C:\Sites\Termometro
icacls C:\Dados\Termometro\Fotos
(Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "system.applicationHost/applicationPools/add[@name='TermometroPool']" -Name environmentVariables).Collection.name
```

A lista traz os nomes, não os valores. Esperado: `ConnectionStrings__Sala`, `Fotos__Pasta`, `ChaveCoordenacao`, `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_HTTPS_PORT`, `Banco__Migrar`. `Mqtt__Desligado` só aparece quando o assinante foi desligado ou religado por variável. Não capture o valor da chave.

No IIS Manager, confirme `AlwaysRunning`, idle timeout `00:00:00`, preload `True`, `.NET CLR Version: No Managed Code`, identidade `ApplicationPoolIdentity` e Rapid-Fail Protection habilitado. Anonymous Authentication usa a identidade do pool (não IUSR). O site tem binding HTTP 80 com host `api.termometro.local`; o site padrão pode permanecer, pois o host é distinto. Reexecução preserva bindings de site existente: confirme host/porta manualmente.

### HTTPS no laboratório

A API chama `UseHttpsRedirection`, então o HTTPS vem antes da prova final. Crie o certificado no servidor com o nome do site, exporte só a parte pública e importe essa parte em `Cert:\LocalMachine\Root` **da própria VM**:

```powershell
$cert = New-SelfSignedCertificate -DnsName api.termometro.local -CertStoreLocation Cert:\LocalMachine\My
Export-Certificate -Cert $cert -FilePath C:\GS\api-termometro.cer | Out-Null
Import-Certificate -FilePath C:\GS\api-termometro.cer -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
Get-ChildItem Cert:\LocalMachine\Root | Where-Object Subject -eq 'CN=api.termometro.local' | Select-Object Subject,NotAfter,Thumbprint
```

O `.cer` do `Export-Certificate` leva só a chave pública. Sem a importação em `Root`, o Windows não confia no certificado autoassinado e o `Invoke-WebRequest https://` dos scripts 08 e 09 para com erro de confiança antes de testar a aplicação. Na repetição, reutilize o certificado que já está em `Cert:\LocalMachine\My` em vez de criar outro. No cliente de laboratório, importe o mesmo `.cer` em Trusted Root Certification Authorities (`certlm.msc` ou o mesmo `Import-Certificate`) para o navegador abrir o painel sem aviso.

No IIS Manager: site `Termometro` → Bindings → HTTPS 443, hostname `api.termometro.local`, SNI marcado e o certificado criado acima. A prova HTTPS em `/ambientes` só funciona depois do banco (passo 5) e da publicação (passo 8); ela está no passo 8.

O pool já recebe `ASPNETCORE_ENVIRONMENT=Production` e `ASPNETCORE_HTTPS_PORT=443` pelos scripts 04 e 08 (junto com a connection string, a pasta de fotos, a chave e `Banco__Migrar=false`). Não grave isso no `web.config` publicado: a próxima cópia apagaria, e a chave iria parar no backup. Não use Development nem Demo no servidor: Development abre o Swagger; Demo abre `POST /demo/semear`. Em Production essa rota responde 404.

```powershell
New-NetFirewallRule -Name TermometroHTTPS -DisplayName 'Termometro HTTPS laboratorio' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 443 -Profile Domain,Private -RemoteAddress LocalSubnet
Test-NetConnection api.termometro.local -Port 443
```

Na repetição, confira a regra existente em vez de recriá-la. O certificado local serve apenas para o laboratório; nunca exporte sua chave privada para evidências. HTTP permanece disponível para a inicialização interna do IIS, com redirecionamento da aplicação para HTTPS.

## 5. SQL Server Express

Antes da instalação no DC, crie em Active Directory Users and Computers uma conta de domínio dedicada `svc.sql`, sem grupos administrativos, com senha própria digitada no formulário seguro. Não use essa conta para usuários da aplicação. Na GPO, negue a ela logon local e por RDP; não negue logon como serviço. Planeje a troca da senha junto da configuração do serviço para não interromper o banco quando expirar.

Execute o instalador offline na VM: New standalone installation → Database Engine Services → instância nomeada `SQLEXPRESS` → serviço automático. Na VM que também é DC, configure a conta do mecanismo como `TERMOMETRO\svc.sql` (não conta local/virtual), digitando a senha apenas no instalador; deixe o instalador conceder os direitos necessários ao serviço. Escolha **Windows Authentication**, adicione a conta administrativa atual como SQL administrator. Não habilite modo misto, SQL Browser nem portas externas. Não exponha SQL na rede; API e banco usam a mesma VM. Reinicie se o instalador pedir.

```powershell
Get-Service 'MSSQL$SQLEXPRESS'
.\05-sql.ps1
```

O script usa ADO.NET disponível no PowerShell 5.1; não exige `sqlcmd`/SSMS. Cria banco `Termometro` e login Windows `IIS APPPOOL\TermometroPool` com `db_datareader`/`db_datawriter`. Não dá `db_owner` ao processo web. A conexão integrada local usa `TrustServerCertificate=True` pelo certificado do laboratório. A API em produção não migra sozinha: `Banco__Migrar=false` no pool. Sem isso o padrão (`true`) chama `Database.Migrate()` na subida, com uma conta que não tem direito de DDL.

Na máquina com SDK .NET 8, a partir de `api/` (o comando não abre o SQL; a variável só satisfaz a factory):

```powershell
dotnet tool restore
$env:ConnectionStrings__Sala = 'Server=localhost;Database=Termometro;Integrated Security=True;TrustServerCertificate=True'
dotnet ef migrations script --idempotent --project TermometroDaSala.Api --output $env:TEMP\termometro-migracao.sql
```

Copie o arquivo para a VM e não o versione: ele repete o segredo HMAC de demonstração da semente. Na VM, como administrador:

```powershell
.\05-sql.ps1 -MigrationScript $env:TEMP\termometro-migracao.sql
```

O script separa os lotes no `GO` e aplica no banco `Termometro` com a conta da sessão, não com o pool. A saída deve listar `20261105025544_Inicial`, `20261105033613_Enums` e `20261105050515_SegredosPorDispositivo`. Pode rodar de novo: o SQL é idempotente. O banco é criado num comando e o usuário do pool em outro, já conectado ao banco `Termometro` (`ChangeDatabase`): `USE` no mesmo lote do `CREATE DATABASE` falharia com o erro 911. Sem `-MigrationScript`, o 05 só cria o banco e o login.

## 6. GPO, RDP e auditoria

```powershell
.\06-gpo.ps1
gpmc.msc
```

O script cria/vincula `Termometro-Seguranca` à raiz do domínio isolado, configura bloqueio de tela em 600 s, NLA, RDP e preferência de auditoria avançada; aplica política de senha do domínio (12 caracteres, complexidade, histórico 24, expiração 90 dias, bloqueio após 5 erros por 15 min) e SACL de Modify na pasta de fotos para sucesso/falha (Modify inclui ReadData, então leitura, gravação e exclusão geram evento). Antes de abrir o firewall, conclua na GPMC:

1. Edite `Termometro-Seguranca` → Computer Configuration → Policies → Windows Settings → Security Settings → Local Policies → User Rights Assignment. Em **Allow log on through Remote Desktop Services**, defina **somente** `TERMOMETRO\AdminsTermometro`. Em **Deny log on through Remote Desktop Services**, inclua Guests. Direitos de negação têm prioridade: verifique que nenhuma outra GPO nega o grupo permitido. Em um DC, o direito explícito na GPO é essencial. São duas peças: este direito e a permissão de conexão de `Remote Desktop Users`, onde o `03-usuarios-grupos.ps1` pôs `AdminsTermometro`. A VM é DC: a Default Domain Controllers Policy, vinculada à OU Domain Controllers, vence a GPO da raiz. Em `gpresult /scope computer /h C:\GS\gpresult.html`, confira que o direito vem de `Termometro-Seguranca`; se vier da Default Domain Controllers Policy, defina nela o mesmo valor.
2. Na mesma GPO, Advanced Audit Policy Configuration → Audit Policies → Logon/Logoff → Audit Logon: Success e Failure. Object Access → Audit File System: Success e Failure. Security Options → Audit: Force audit policy subcategory settings…: Enabled. O script habilita as duas subcategorias localmente para o ensaio; a configuração na GPO é necessária para persistência após atualização.
3. Confirme na política de domínio efetiva os valores de senha/bloqueio. Caso uma GPO existente os sobreponha, ajuste a política de senha na **Default Domain Policy** pelos mesmos valores; não duplique políticas conflitantes. O bloqueio de tela por inatividade também vale para sessões administrativas, mesmo sem proteção de tela do usuário.
4. Windows Defender Firewall with Advanced Security → Inbound Rules: permita TCP 3389 somente do IP do cliente de laboratório, perfil Domain/Private. Não habilite um conjunto de regras amplo sem verificar escopo. Regra de exemplo abaixo; substitua o IP. Desative eventuais regras permissivas de RDP preexistentes pelo console e confira também regras UDP. Não desative o firewall.

```powershell
New-NetFirewallRule -Name TermometroRDP -DisplayName 'Termometro RDP cliente laboratorio' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 3389 -RemoteAddress 192.168.56.20 -Profile Domain,Private
gpupdate /force
Get-GPO -All
Get-ADDefaultDomainPasswordPolicy
Get-ADGroupMember -Identity S-1-5-32-555
gpresult /r
auditpol /get /subcategory:*
```

Faça logoff/logon para regras de usuário e grupos. Do cliente autorizado, `admin.termometro` deve entrar via RDP e `coordenador` deve ser recusado, mesmo com senha correta (não tem o direito nem está em `Remote Desktop Users`). Não provoque cinco erros na conta de administração. Teste senha inválida uma vez com usuário de laboratório e registre 4625; sucesso gera 4624 (logon remoto tipo 10).

SACL (aba Security → Advanced → Auditing) não concede acesso: complementa a DACL do passo 4. Registre uma ocorrência com foto pela API e abra a foto por `GET /ocorrencias/{id}/foto`: o pool grava e lê o arquivo, e Security deve mostrar 4663 com `WriteData` na gravação e `ReadData` na leitura (e 4660 se a foto for apagada). Para negação, tente gravar um arquivo como `coordenador`: deve falhar e gerar falha de acesso a objeto (4656). Correlacione usuário, caminho, operação e horário. Não use foto de pessoa nem dado real.

## 6a. DFS e RDS: mínimo demonstrável

O briefing cita DFS e RDS no conceito (p. 32), sem pontos separados nas pp. 33–34. **DFS entra neste roteiro** com um namespace e um destino. O acesso remoto demonstrado é o **RDP administrativo restrito** do passo 6; o alcance de RDS está em [Limitações](#limitações).

Para DFS, crie `C:\Dados\Termometro\Manuais` e coloque uma cópia pública deste roteiro, sem configurações privadas. Server Manager → File and Storage Services → Shares: compartilhe como `Manuais`, acesso de leitura para `TERMOMETRO\Coordenacao`, Full Control para Administrators; NTFS com os mesmos leitores, SYSTEM/Administrators FullControl. Nenhum acesso de usuário à pasta de fotos.

DFS Management (`dfsmgmt.msc`) → Namespaces → New Namespace → servidor `SRV-TERMOMETRO` → nome `Escola` → namespace baseado em domínio. Dentro dele, New Folder `Manuais`, destino `\\SRV-TERMOMETRO\Manuais`. Use permissões de leitura para Coordenacao e administrativas para administradores também na raiz. No cliente, abra `\\termometro.local\Escola\Manuais` como coordenador e registre a leitura.

## 7. Monitoramento e recuperação do W3SVC

Abra `perfmon.msc`. Em Performance Monitor → Add Counters, selecione os equivalentes **no idioma da VM**:

| Recurso | Contador (nomes em inglês como referência) | Alerta a cada 15 s |
|---|---|---|
| CPU | `\Processor(_Total)\% Processor Time` | acima de 80% |
| Disco do sistema | `\LogicalDisk(C:)\% Free Space` | abaixo de 15% livre |
| Memória | `\Memory\Available MBytes` | abaixo de 512 MB livres |

Copie os caminhos reais (podem ser localizados) para os parâmetros. Não use disco `_Total` se quiser provar o volume da aplicação.

```powershell
.\07-monitoramento.ps1 -CpuCounter '\Processor(_Total)\% Processor Time' -DiskCounter '\LogicalDisk(C:)\% Free Space' -MemoryCounter '\Memory\Available MBytes'
logman query Termometro-Alertas
sc.exe qfailure W3SVC
sc.exe qfailureflag W3SVC
```

O script cria um conjunto de alertas e configura recuperação do W3SVC (três ações restart com atraso de 5 s; reset após 86400 s). Não altera/deleta coletor existente: para repetir, confira-o na interface. Em Data Collector Sets → User Defined → Termometro-Alertas, confirme os três limites e a opção **Log an entry in the application event log**. Configure início por agendamento/tarefa na inicialização e sem duração máxima, inicie e reinicie a VM uma vez para provar persistência. Guarde o XML/configuração da tarefa e estado Running. O script sozinho não cria essa tarefa.

Para ter **histórico**, crie também um conjunto `Termometro-Recursos`, manual, tipo Performance Counter, com os mesmos três contadores, amostra 15 s, formato BLG e diretório `C:\PerfLogs\Termometro\Recursos`. Inicie e confira no Reports/Performance Monitor o arquivo de amostras; alerta não substitui histórico.

Ensaio sem sobrecarregar o host: crie no perfmon um coletor separado `Termometro-Ensaio` com os mesmos contadores, limites temporários CPU > 0%, disco livre < 100%, memória disponível < total de RAM em MB. Inicie por 30–60 s e confirme eventos de limite nos logs Application e em Applications and Services Logs → Microsoft → Windows → Diagnosis-PLA (habilite canal Operational quando disponível). Registre evento/provedor, contador, valor e horário. Depois pare/remova apenas o coletor de ensaio e mostre os limites operacionais originais e `Termometro-Alertas` Running. Não encha disco nem force falta de memória para produzir print.

## 7a. Acompanhamento pelo Visualizador de Eventos

[visao-termometro.xml](visao-termometro.xml) é um modo de exibição personalizado que junta, numa tela só, o que a infraestrutura registra sobre a aplicação:

| Origem | Eventos | O que mostra |
|---|---|---|
| System, `Microsoft-Windows-WAS` | 5009, 5011, 5002 | worker do pool terminou sem aviso, falha de comunicação, pool desligado pelo Rapid-Fail |
| System, `Service Control Manager` | 7031, 7034, 7036 | W3SVC terminou sem aviso e voltou a Running |
| Application, `IIS AspNetCore Module V2` | todos | erro na subida ou no processo da API |
| Application e `Diagnosis-PLA/Operational` | 2031 | disparo dos alertas de CPU, disco e memória do passo 7 |
| Security | 4624 (só RDP, tipo 10), 4625, 4656, 4663 | logon permitido e recusado, acesso à pasta de fotos (SACL do passo 6) |

Na VM, como administrador: `eventvwr.msc` → Action → **Import Custom View** → `C:\GS\termometro\infra\visao-termometro.xml`. A visão aparece em Custom Views → **Termometro - acompanhamento**. O 7036 registra qualquer serviço; no teste de falha, procure a mensagem com o nome do W3SVC. Os logs do IIS ficam em `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log`, em UTC: compare `sc-status` e horário com os eventos da visão (E12).

## 8. Publicação da API e painel

Na máquina de desenvolvimento com SDK .NET 8 (fora do servidor):

```powershell
dotnet publish .\api\TermometroDaSala.Api\TermometroDaSala.Api.csproj -c Release -o D:\GS\publish
```

Transfira a pasta publicada inteira para `C:\GS\publish` da VM; não copie apenas a DLL. Ela precisa conter `web.config`, DLL, dependências, `modelo\modelo.json` e o painel em `wwwroot\index.html`. Não leve arquivos de desenvolvimento com credenciais. Rode o passo 5 antes desta publicação.

```powershell
.\08-publicar-api.ps1 -PublishPath C:\GS\publish -ProbeUri https://api.termometro.local/ambientes
# Sem NAT de saida na porta 1883, acrescente -MqttDesligado (igual ao 04).
```

O script reaplica as variáveis do pool (mesma regra do 04), exige `wwwroot\index.html`, para só o pool dedicado, salva backup, copia sem apagar arquivos e religa o pool em `finally`. Depois exige HTTP 200 em `/ambientes` e no painel `/`. Falha de HTTP é erro, não sucesso parcial. A volta para a versão anterior é manual: ver [Limitações](#limitações). O backup herda proteção administrativa; não o compartilhe.

### MQTT ou Insomnia

As duas formas de leitura valem; use uma na prova.

1. **NAT de saída.** A VM alcança `broker.hivemq.com` na porta TCP 1883. Não abra essa porta para dentro e não publique o broker. O assinante sobe com o pool (`Mqtt__Desligado` ausente ou `false`). No vídeo, uma leitura nova aparece em `/` depois que o worker volta.
2. **Sem internet.** `.\04-iis.ps1 -MqttDesligado` e o mesmo switch no 08 (`Mqtt__Desligado=true`). A leitura entra por `POST /leituras` no Insomnia (`api/insomnia.json`), com o HMAC de demonstração. Desligue o simulador para não disputar o relógio. O painel em `/` lê `GET /ambientes` do mesmo site.

Confira também à mão, na VM e sem erro de certificado:

```powershell
(Invoke-WebRequest https://api.termometro.local/ambientes -UseBasicParsing).StatusCode
(Invoke-WebRequest https://api.termometro.local/ -UseBasicParsing).StatusCode
```

Abra `/` e `/ambientes`. O fluxo da proposta é sala, leitura, previsão, ocorrência ou ação, e o dado continua depois de reciclar o pool. A foto só sai por `GET /ocorrencias/{id}/foto`; não há URL direta para `C:\Dados\Termometro\Fotos`.

## 9. Teste de falha e entrega

Siga [TESTE-DE-FALHA.md](TESTE-DE-FALHA.md): mate somente o worker do pool e depois o processo isolado do W3SVC. `Stop-Service` é parada planejada e **não comprova recuperação por falha**. Registre PIDs antes/depois, retorno HTTP e eventos sem reiniciar manualmente os serviços durante a medição.

Preencha [EVIDENCIAS.md](EVIDENCIAS.md) e grave [ROTEIRO-VIDEO.md](ROTEIRO-VIDEO.md), até 7 minutos. A entrega é feita por formulário até 19/11 às 23h55, por um integrante: link do repositório, link do vídeo e os dados do [Resumo do ambiente](#resumo-do-ambiente).

## Conferência antes de executar

O parser verifica sintaxe, sem instalar funções nem executar scripts:

```powershell
$errorsFound = @()
$scripts = @(Get-ChildItem .\infra\*.ps1)
if ($scripts.Count -ne 9) { throw 'Esperados exatamente nove scripts' }
$scripts | ForEach-Object {
    $tokens = $null; $parseErrors = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$tokens, [ref]$parseErrors)
    $errorsFound += $parseErrors
}
if ($errorsFound.Count) { $errorsFound; throw 'Erro de sintaxe' }
'9 scripts: sintaxe valida'
```

O parser confere a sintaxe. O funcionamento na VM é comprovado pelas saídas e prints de [EVIDENCIAS.md](EVIDENCIAS.md) e pelo vídeo.

## Limitações

- **Uma VM para tudo.** AD, DNS, arquivos, IIS e SQL ficam na mesma VM para caber no laboratório. É um ponto único de falha e não é arquitetura de produção. SQL no controlador de domínio tem restrições de conta de serviço: o roteiro usa a conta de domínio dedicada `svc.sql` (passo 5). Se o instalador recusar, o SQL vai para uma segunda VM membro, e a identidade do pool e o acesso ao banco precisam ser revistos (o login virtual local do 05 não serve para SQL remoto).
- **Acesso remoto.** O demonstrado é RDP administrativo restrito a `AdminsTermometro`, que não é uma implantação de Remote Desktop Session Host. RDS completo, se o professor exigir, vai em **outra VM membro do domínio**: Server Manager → Add Roles and Features → Remote Desktop Services installation → Quick Start → Session-based desktop deployment (Connection Broker, Web Access e Session Host com coleção), coleção restrita a `AdminsTermometro`, certificados e licenciamento conforme a avaliação da Microsoft e uma conexão real pela coleção. Session Host não vai no DC.
- **DFS sem replicação.** Um namespace e um destino; não há disponibilidade entre servidores. DFS Replication exigiria segundo servidor, segundo destino e grupo de replicação.
- **Passos manuais.** Direito de logon por RDP e auditoria na GPMC (a entrada em `Remote Desktop Users` é feita pelo 03), zona reversa e PTR, início do `Termometro-Alertas` no boot e o coletor de histórico `Termometro-Recursos` são feitos pela interface (passos 2, 6 e 7). Os nomes dos contadores mudam com o idioma da VM.
- **Publicação sem rollback automático.** Se o 08 falhar: preserve os logs, pare o pool, mova os binários novos para quarentena (cópia simples não remove arquivos adicionados) e restaure o backup revisado, sem tocar no banco nem nas fotos. O 08 também não apaga arquivos que saíram da publicação: compare as pastas e limpe com backup.
- **Certificado autoassinado.** Serve só para o laboratório; a chave privada nunca sai da VM nem entra em evidência.
- **Recuperação sem promessa de zero perda.** Uma falha pode interromper requisições em andamento e perder a leitura MQTT em voo; depois da volta, a prova mostra uma leitura nova.
