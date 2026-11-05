#Requires -RunAsAdministrator
[CmdletBinding()]
param(
    [string]$Instance = '.\SQLEXPRESS',
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')][string]$Database = 'Termometro',
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Pool = 'TermometroPool',
    [string]$MigrationScript
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
    if ($MigrationScript) {
        if (-not (Test-Path -LiteralPath $MigrationScript -PathType Leaf)) { throw "Script de migracao ausente: $MigrationScript" }
        $sql = Get-Content -LiteralPath $MigrationScript -Raw -Encoding UTF8
        if ([string]::IsNullOrWhiteSpace($sql)) { throw 'Script de migracao vazio.' }
        # shortcut: so reconhece GO sozinho na linha, gere de novo se o EF emitir GO com contagem.
        $lotes = [regex]::Split($sql, '(?m)^\s*GO\s*$')
        $connection.ChangeDatabase($Database)
        $migracao = $connection.CreateCommand()
        $migracao.CommandTimeout = 180
        $n = 0
        foreach ($lote in $lotes) {
            if ([string]::IsNullOrWhiteSpace($lote)) { continue }
            $n++
            $migracao.CommandText = $lote
            try { [void]$migracao.ExecuteNonQuery() }
            catch { throw "Falha no lote $n da migracao. $($_.Exception.GetBaseException().Message)" }
        }
        $lista = $connection.CreateCommand()
        $lista.CommandText = 'SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;'
        $reader = $lista.ExecuteReader()
        $ids = New-Object System.Collections.Generic.List[string]
        while ($reader.Read()) { [void]$ids.Add($reader.GetString(0)) }
        $reader.Close()
        # shortcut: confere so Inicial, Enums e SegredosPorDispositivo, inclua o id quando houver outra migracao.
        foreach ($id in '20261105025544_Inicial','20261105033613_Enums','20261105050515_SegredosPorDispositivo') {
            if ($ids -notcontains $id) { throw "Historico sem $id. Veja o erro 1785 no README da API." }
        }
        Write-Host ("Migracoes: " + ($ids -join ', '))
    }
} finally { $connection.Dispose() }
if ($MigrationScript) {
    Write-Host "Banco [$Database], acesso do pool e migracao idempotente aplicados por esta sessao de administrador. Banco__Migrar=false no pool; nao de db_owner ao processo web."
} else {
    Write-Host "Banco [$Database] e acesso do pool criados, sem migracao. Gere o SQL na maquina com SDK (README, passo 5) e rode de novo com -MigrationScript. Nao de db_owner ao pool."
}
