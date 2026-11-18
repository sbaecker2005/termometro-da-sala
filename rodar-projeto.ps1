#requires -Version 5.1
<#
    Roda o projeto inteiro numa maquina Windows, a partir da raiz do repositorio:
      - API (SOA) em http://localhost:5080, modo Demo, banco novo no SQL Server Express local;
      - cena de demonstracao com a previsao do modelo (ML) e o mapa termico;
      - desktop da coordenacao (C#);
      - painel web aberto no navegador;
      - com -App, tambem o app do professor (Mobile) no Expo, ligado a esta API.

    Requisitos: .NET SDK 8 e SQL Server Express (.\SQLEXPRESS). Para -App: Node.js e `npm --prefix app ci` uma vez.
    O ESP32 (Physical Computing) roda no Wokwi; a infraestrutura (Operating Systems) e a VM do roteiro em infra/.
    Uso:  powershell -ExecutionPolicy Bypass -File .\rodar-projeto.ps1 [-App]
#>
param([switch]$App)
$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'ferramentas/abrir-apresentacao.ps1')
Start-Process 'http://localhost:5080/'

if ($App) {
    $env:EXPO_PUBLIC_API_MOCK = '0'
    $env:EXPO_PUBLIC_API_URL = 'http://localhost:5080'
    Start-Process -FilePath (Get-Command npm.cmd).Source -WorkingDirectory (Join-Path $PSScriptRoot 'app') -ArgumentList 'start'
}
Write-Output 'Projeto no ar. Para encerrar a API, use o comando gravado em output/apresentacao/<banco>/PROCESSOS.txt.'
