# Roteiro de gravação — fala por fala

> Vídeo gravado (6 min 39 s): seguiu este roteiro com diferenças. Não mostrou `icacls` nem as permissões NTFS, não mostrou o teste do W3SVC (só fechou dizendo que a política de recuperação está configurada, com limitação documentada) e a previsão apareceu como "sem dado de rua". O que o vídeo comprova, minuto a minuto, está em [EVIDENCIAS.md](EVIDENCIAS.md).

Alvo 6:30. Limite 7:00. Fale devagar; o que está escrito já cabe no tempo.

**Antes de apertar o rec**

- [ ] Uma janela PowerShell elevada, limpa (sem o histórico de hoje).
- [ ] Abertas e posicionadas: IIS Manager, GPMC, perfmon, DFS Management, Visualizador de Eventos, Edge.
- [ ] Edge **fechado** na hora do teste de falha.
- [ ] `C:\GS\enviar-leitura.ps1` salvo.
- [ ] `cd C:\termometro-da-sala-main\infra`.
- [ ] `$env:Path += ';C:\Windows\System32\inetsrv'` (senão `appcmd` não é reconhecido).

---

## 0:00 – 0:20 — Abertura

**Tela:** o mural em `https://api.termometro.local`.

**Falar:**
> Boa noite. Eu sou o Guilherme, RM 559765, e esta é a parte de Sistemas Operacionais do Termômetro da Sala. O problema é simples: salas de aula que esquentam e ninguém percebe a tempo. A solução roda num Windows Server 2022 em máquina virtual isolada, no VMware, com rede NAT — nada exposto na rede de casa. Vou mostrar a infraestrutura, o monitoramento e um teste de falha real.

---

## 0:20 – 1:30 — Rede, domínio e identidade

**Tela:** PowerShell.

**Digitar:**
```powershell
hostname
Get-NetIPAddress -InterfaceAlias Ethernet0 -AddressFamily IPv4 | Select-Object IPAddress,PrefixLength
Get-ADDomain | Select-Object DNSRoot,NetBIOSName,DomainMode
nslookup api.termometro.local
nslookup 192.168.16.10
```

**Falar:**
> O servidor é o SRV-TERMOMETRO, endereço fixo 192.168.16.10, barra 24. Ele é controlador do domínio termometro.local e também o DNS. Repare que o nome resolve para o endereço e o endereço resolve de volta para o nome — a zona reversa está criada, com o PTR.

**Digitar:**
```powershell
Get-ADGroupMember Coordenacao      | Select-Object SamAccountName
Get-ADGroupMember AdminsTermometro | Select-Object SamAccountName
```

**Falar:**
> A separação de contas é por grupo: Coordenação para quem usa o sistema, AdminsTermometro para quem administra, e uma conta de serviço, a svc.sql, que não é pessoa e não entra em lugar nenhum.

**Tela:** GPMC, já na OU Domain Controllers, aba Linked Group Policy Objects.

**Falar:**
> A GPO Termometro-Seguranca está ligada na unidade organizacional dos controladores de domínio, e não só no domínio. Isso importa: política ligada no domínio perde para a política da OU dos controladores, e o direito não chegaria no servidor.

**Tela:** editor da GPO em User Rights Assignment.

**Falar:**
> Aqui estão os três direitos. Acesso remoto permitido só para AdminsTermometro e administradores. Logon local negado para a svc.sql. E acesso remoto negado para a svc.sql e para convidados.

**Tela:** as duas capturas de RDP, lado a lado, ou a pasta de prints.

**Falar:**
> Testado do host, pela VMnet8: admin.termometro entra; coordenador é recusado. E o detalhe que vale a pena notar — a senha do coordenador estava certa. A autenticação passou, a autorização é que negou. Por isso não existe evento de senha errada, e sim uma recusa no Serviço de Área de Trabalho Remota.

---

## 1:30 – 2:20 — Aplicação, certificado e banco

**Tela:** IIS Manager, diálogo de Bindings aberto.

**Falar:**
> A aplicação está publicada no IIS, em HTTPS, com certificado emitido para api.termometro.local e confiável no servidor — sem aviso de segurança no navegador.

**Tela:** Advanced Settings do TermometroPool.

**Falar:**
> O pool fica sempre ligado: Start Mode AlwaysRunning e tempo ocioso zero. É um mural de TV, não pode dormir.

**Digitar:**
```powershell
dotnet --list-runtimes | Select-String AspNetCore
Get-Service MSSQL`$SQLEXPRESS | Select-Object Name,Status,StartType
sqlcmd -S .\SQLEXPRESS -d TermometroDaSala -Q "SELECT MigrationId FROM __EFMigrationsHistory"
```

**Falar:**
> O runtime do ASP.NET Core está instalado, o SQL Server Express está rodando e o banco tem as cinco migrações aplicadas: Inicial, Enums, SegredosPorDispositivo, AlertaFrio e MapaTermico. A aplicação não migra sozinha em produção — a variável Banco__Migrar está em falso, e o script idempotente é aplicado à parte.

**Falar (sem precisar de tela):**
> O SQL roda em conta virtual, a NT Service barra MSSQL cifrão SQLEXPRESS. É o padrão de menor privilégio: conta por serviço, sem senha para gerenciar. Conta de domínio só seria necessária para acessar recurso de rede, e aqui a API conecta localmente.

---

## 2:20 – 3:05 — Monitoramento

**Tela:** perfmon, Data Collector Sets, User Defined.

**Falar:**
> São dois conjuntos. O Termometro-Recursos grava processador, espaço livre em disco e memória disponível a cada 15 segundos, em formato binário. Repare no número do arquivo: já está no terceiro, ou seja, vem coletando desde antes de hoje.

**Digitar:**
```powershell
logman query "Termometro-Alertas"
```

**Falar:**
> E o Termometro-Alertas dispara em três situações: processador acima de 80 por cento, disco com menos de 15 por cento livre, e memória disponível abaixo de 512 megabytes.

**Tela:** Agendador de Tarefas com a tarefa Termometro.

**Falar:**
> A coleta sobe sozinha no boot, por tarefa agendada. Se a máquina reiniciar, o monitoramento volta sem ninguém tocar.

---

## 3:05 – 4:00 — Teste de falha, parte 1

**Fechar o Edge antes.** Requisição externa inicia o worker e estraga a prova.

**Digitar:**
```powershell
appcmd list wp
.\09-teste-de-falha.ps1 -Parte Pool -ProbeUri https://api.termometro.local/ambientes -LaboratorioConfirmado
```

Antes do take: `$env:Path += ';C:\Windows\System32\inetsrv'` para o `appcmd` funcionar sem caminho completo. O script exige **um único** worker do pool — confira com `appcmd list wp` antes, com o Edge fechado.

**Falar, enquanto roda:**
> Vou matar o processo que atende a aplicação. Não vou parar o serviço — parada planejada não é falha. Estou derrubando o worker do pool, à força.
>
> O PID era este. O navegador está fechado, ninguém está pedindo nada. Agora espero.

**Quando o PID novo aparecer:**
> Subiu outro processo, PID diferente, sem intervenção. E agora a aplicação responde.

**Digitar:**
```powershell
Invoke-WebRequest https://api.termometro.local/ambientes -UseBasicParsing | Select-Object StatusCode
```

**Falar:**
> Duzentos. Isso é recuperação automática: o processo morreu, o WAS levantou outro, e o serviço voltou sozinho.

---

## 4:00 – 4:40 — Recuperação do W3SVC e a limitação

**Digitar:**
```powershell
sc.exe qfailure W3SVC
```

**Falar:**
> A política de recuperação do serviço do IIS está configurada: três reinícios, cinco segundos entre eles, contador zerado em um dia.
>
> Aqui eu preciso ser honesto sobre um limite. A entrega pede derrubar o processo do serviço. Nesta build o W3SVC roda dentro do grupo de processos compartilhado do IIS, junto com outros serviços. Tentei isolar com sc config type igual a own; o serviço parou de iniciar. Revertí para compartilhado e o serviço voltou. Eu não substituí o teste por um Stop-Service, porque parada planejada não prova recuperação.

**Tela:** log System filtrado, janela de 16/11.

**Falar:**
> Mas a política funciona, e tem prova. Durante aquela tentativa, o Gerenciador de Controle de Serviços reaplicou o reinício de cinco segundos mais de cem vezes seguidas, registrado nos eventos 7031 e 7024. É a política de recuperação disparando de verdade — só que num cenário que eu não provoquei de propósito.

---

## 4:40 – 5:50 — Fluxo completo

**Tela:** Edge no mural.

**Falar:**
> Este é o mural, só leitura, pensado para a TV do corredor. Atualiza a cada 30 segundos, e o nível está escrito, não só colorido — quem não distingue cor continua lendo a informação.

**Digitar:**
```powershell
.\enviar-leitura.ps1 -Sala 2 -Temperatura 34.8 -Umidade 66
```

**Falar:**
> Vou mandar uma leitura agora, como se fosse o ESP32. A leitura é assinada com HMAC SHA-256, com segredo por dispositivo. Sem assinatura válida a API recusa, e recusa também leitura repetida ou atrasada.

**Atualizar o mural.**

**Falar:**
> A Sala 2 mudou de nível. O segredo aqui é de demonstração, está no repositório, declarado como tal. Em campo, trocar chave é revogar o dispositivo e cadastrar outro.
>
> Na coluna de previsão aparece "sem dado da rua". Isso é o comportamento previsto fora do modo de demonstração: nada alimenta a tabela de leituras da rua neste servidor, então o painel mantém a última leitura válida da sala e declara a ausência, em vez de mostrar previsão velha ou inventada.

**Tela:** `https://api.termometro.local/ambientes`.

**Falar:**
> E esta é a API por trás, com os ambientes e suas dimensões.

---

## 5:50 – 6:30 — Arquivos, auditoria e fecho

**Tela:** Explorer em `\\termometro.local\Escola\Manuais`.

**Falar:**
> Os manuais ficam num espaço de nomes DFS baseado em domínio. O usuário acessa pelo nome do domínio, não pelo nome do servidor — se amanhã o compartilhamento mudar de máquina, o caminho continua o mesmo.

**Tela:** Visualizador de Eventos, Custom View.

**Falar:**
> E tudo isso é auditado. Logon com sucesso e falha, e acesso a arquivo com sucesso e falha, ligados por GPO. Esta visão personalizada junta a queda e a volta do serviço, e o horário bate com o log do IIS.

**Falar (fecho):**
> Resumindo: domínio com DNS direto e reverso, contas separadas por grupo com direitos de acesso remoto controlados por GPO, aplicação publicada em HTTPS com certificado confiável, banco com migração controlada, monitoramento com três alertas e coleta que sobe no boot, recuperação automática comprovada no pool, política de recuperação configurada no serviço com a limitação documentada, arquivos em DFS e auditoria ligada. Obrigado.

---

## Se algo der errado na hora

- Pool demorou a voltar: não corte. Mostre o relógio e diga o intervalo real.
- Comando errou: apague a linha e refaça na frente da câmera. Erro corrigido é melhor que corte suspeito.
- Estourou 7 minutos: corte do bloco 5:50, não do teste de falha.
