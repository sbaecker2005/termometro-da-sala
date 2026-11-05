#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$Instance = '.\SQLEXPRESS',
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')][string]$Database = 'Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool'
)
$ErrorActionPreference = 'Stop'
# O instalador offline e executado manualmente; aqui usamos apenas autenticacao integrada local.
if ((Get-Service 'MSSQL$SQLEXPRESS').Status -ne 'Running') { throw 'Instale e inicie SQL Server Express SQLEXPRESS antes.' }
$connection = [System.Data.SqlClient.SqlConnection]::new("Server=$Instance;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True")
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = @"
IF DB_ID(N'$Database') IS NULL CREATE DATABASE [$Database];
IF SUSER_ID(N'IIS APPPOOL\$Pool') IS NULL CREATE LOGIN [IIS APPPOOL\$Pool] FROM WINDOWS;
USE [$Database];
IF USER_ID(N'IIS APPPOOL\$Pool') IS NULL CREATE USER [IIS APPPOOL\$Pool] FOR LOGIN [IIS APPPOOL\$Pool];
IF IS_ROLEMEMBER(N'db_datareader', N'IIS APPPOOL\$Pool') <> 1 ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\$Pool];
IF IS_ROLEMEMBER(N'db_datawriter', N'IIS APPPOOL\$Pool') <> 1 ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\$Pool];
SELECT DB_NAME() AS Banco;
"@
    [void]$command.ExecuteNonQuery()
} finally { $connection.Dispose() }
Write-Host 'Banco e acesso de dados criados. Migracoes devem ser aplicadas separadamente por administrador, nunca com db_owner no pool.'
