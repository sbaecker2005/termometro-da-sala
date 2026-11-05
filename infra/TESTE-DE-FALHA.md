# Teste de falha — pool e W3SVC

## Preparação

Use somente a VM isolada e com checkpoint. Termine o README e publique a aplicação. O 09 testa por HTTPS, então a parte pública do certificado precisa estar em `Cert:\LocalMachine\Root` **da própria VM**; o `04-iis.ps1` faz essa importação (README, passo 4, "HTTPS no laboratório"). Sem isso o `Invoke-WebRequest` para com erro de confiança antes de matar o processo. Confira antes:

```powershell
Get-ChildItem Cert:\LocalMachine\Root | Where-Object Subject -eq 'CN=api.termometro.local'
(Invoke-WebRequest https://api.termometro.local/ambientes -UseBasicParsing).StatusCode
```

A prova pede as duas URLs: `GET /ambientes` e o painel em `/` (`wwwroot`). O script 09, com `-ProbeUri` em `/ambientes`, chama as duas antes da falha e de novo só depois do PID novo. Feche o navegador, monitor HTTP e auto-refresh antes de matar processos: uma requisição externa poderia iniciar o worker e invalidar a prova de inicialização automática.

No PowerShell elevado da VM:

```powershell
Import-Module WebAdministration
Get-Service WAS,W3SVC
Get-WebAppPoolState TermometroPool
& "$env:windir\System32\inetsrv\appcmd.exe" list wp
sc.exe qfailure W3SVC
sc.exe qfailureflag W3SVC
```

Confirme AlwaysRunning, preload, idle timeout zero, Application Initialization e Hosting Bundle. Para executar em produção seria necessário outro procedimento; este ensaio é exclusivamente de laboratório.

## Parte 1 — matar w3wp.exe e provar recuperação do pool

1. Registre hora, PID e nome do pool pelo `appcmd list wp`. Uma requisição antes do teste confirma que a aplicação está saudável.
2. Execute **uma vez**:

```powershell
.\09-teste-de-falha.ps1 -Parte Pool -ProbeUri https://api.termometro.local/ambientes -LaboratorioConfirmado
```

3. O script localiza apenas o worker cujo `-ap` corresponde a `TermometroPool`, mata esse PID e espera até 90 s por **outro PID sem enviar requisição durante a espera**. Só depois verifica HTTP 200 em `/ambientes` e em `/`. Não use `Stop-Process -Name w3wp`: atingiria outros sites.
4. Guarde PID anterior/novo, estado Started, hora e resposta HTTP. Abra System no Visualizador de Eventos: filtre provedores WAS/W3SVC/W3SVC-WP conforme disponíveis, procurando término inesperado e reinicialização (por exemplo, WAS 5009). ID exato pode variar: registre o evento real com mensagem, provedor e horário. Confirme requisição posterior no log do IIS e valide novamente a aplicação.

**Rapid-Fail Protection:** mantenha habilitado. O padrão desabilita o pool ao atingir cinco falhas em cinco minutos; confira o valor efetivo. Não faça cinco tentativas para gravar o vídeo: um crash basta. Se repetir ensaio, espere mais que o intervalo configurado. Se o pool desligar, preserve eventos, corrija a causa e só então inicie manualmente; essa intervenção não conta como recuperação automática.

## Parte 2 — falha inesperada do W3SVC

`Stop-Service W3SVC` ou `iisreset` são ações administrativas planejadas; não demonstram as ações de recuperação do Service Control Manager. A prova usa término inesperado do processo do serviço. **W3SVC pode compartilhar svchost com WAS/outros serviços**: o script recusa essa condição para não matar todo o grupo.

Preparação **fora da medição**, pelo console da VM:

```powershell
Get-CimInstance Win32_Service -Filter "Name='W3SVC'" | Select-Object Name,State,ProcessId,ServiceType
# Isolar W3SVC somente no laboratorio e reiniciar antes de medir.
sc.exe config W3SVC type= own
Restart-Computer
```

Após reiniciar, verifique isolamento e configure recuperação (também feito pelo script 07):

```powershell
$svc = Get-CimInstance Win32_Service -Filter "Name='W3SVC'"
Get-CimInstance Win32_Service | Where-Object { $_.ProcessId -eq $svc.ProcessId -and $_.State -eq 'Running' } | Select-Object Name,ProcessId
sc.exe failure W3SVC reset= 86400 actions= restart/5000/restart/5000/restart/5000
sc.exe failureflag W3SVC 1
sc.exe qfailure W3SVC
sc.exe qfailureflag W3SVC
```

Deve haver somente W3SVC naquele PID. Se a versão do Windows não permitir isolamento, **pare e registre a limitação**; não mate svchost compartilhado nem trate uma parada planejada como falha. Resolva com o professor em uma VM descartável compatível antes da gravação.

Execute uma vez:

```powershell
.\09-teste-de-falha.ps1 -Parte Servico -ProbeUri https://api.termometro.local/ambientes -LaboratorioConfirmado
```

O script mata somente o PID isolado, espera o SCM devolver W3SVC a Running com PID novo e depois exige HTTP 200 em `/ambientes` e em `/`. Não use Start-Service nem reabra o site durante a espera. No log **System**, filtre **Service Control Manager**, normalmente eventos 7031/7034 (falha) e 7036 (mudança de estado), registrando mensagem e horário reais. Mostre a ação de reinício de 5000 ms; o tempo de recuperação completo pode superar 5 s.

Depois da gravação, pode manter o isolamento na VM de laboratório. Para reverter, fora do ensaio: `sc.exe config W3SVC type= share` e reinicie a VM. Não remova a política de recuperação.

## Onde tirar a prova e como decidir sucesso

| Prova | Onde / esperado |
|---|---|
| Worker antes/depois | appcmd e saída do 09: PID diferente, pool Started, sem início manual |
| Serviço antes/depois | Win32_Service e saída do 09: PID diferente, Running, recuperação SCM |
| Configuração | IIS Manager e `sc.exe qfailure`, incluindo AlwaysRunning/preload/idle/recuperação |
| Falha e recuperação | Visualizador de Eventos → Custom Views → Termometro - acompanhamento (`visao-termometro.xml`) ou Windows Logs → System; horário e mensagem reais |
| Erro da aplicação | Windows Logs → Application, provedor IIS AspNetCore Module V2/.NET Runtime se houver |
| HTTP após a falha | Saída do 09 e `C:\inetpub\logs\LogFiles\W3SVC<ID>\u_ex*.log`: `/ambientes` e `/`, sc-status 200 |
| Auditoria de usuário/foto | Security: 4624/4625 e acesso a objeto 4663/4656 na pasta de fotos |
| Integridade funcional | O painel em `/` e o dado em `/ambientes` continuam; nenhuma foto ou banco foi apagado |

Logs W3C usam UTC; correlacione com a hora exibida pelo Event Viewer e fuso da VM. Desabilite stdout detalhado do ANCM após diagnóstico; se habilitado temporariamente, use pasta dedicada protegida e não exponha conteúdo sensível em evidências. Uma falha pode interromper requisições em andamento e perder a leitura MQTT que ainda não chegou; recuperação não promete zero perda. Com o assinante ligado, mostre uma leitura com horário novo depois da volta.

Se não recuperar, o 09 termina com erro; guarde a saída, o log e a configuração antes de corrigir. Novo PID sem HTTP 200 nas duas URLs, ou início manual, não aprovam a recuperação. Uma falha pode perder a leitura MQTT que estava em voo; com o assinante desligado, repita o `POST /leituras` depois que `/` voltar.
