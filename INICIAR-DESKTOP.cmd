@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0ferramentas\preparar-windows.ps1"
if errorlevel 1 (
  echo Nao foi possivel confirmar o .NET 8. Leia a mensagem acima.
  pause
  exit /b 1
)
start "" "%~dp0executavel-desktop\TermometroDaSala.Desktop.exe"
