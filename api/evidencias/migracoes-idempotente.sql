IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Ambientes] (
        [Id] nvarchar(32) NOT NULL,
        [Nome] nvarchar(80) NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [Andar] int NOT NULL,
        [FaceSol] nvarchar(20) NOT NULL,
        [TemVentilador] bit NOT NULL,
        CONSTRAINT [PK_Ambientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [LeiturasRua] (
        [Id] bigint NOT NULL IDENTITY,
        [MedidoEm] datetimeoffset NOT NULL,
        [TemperaturaC] float NOT NULL,
        [UmidadePct] int NOT NULL,
        [IndiceCalor] float NOT NULL,
        CONSTRAINT [PK_LeiturasRua] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [NiveisAcao] (
        [Nivel] nvarchar(20) NOT NULL,
        [Nome] nvarchar(max) NOT NULL,
        [AcaoSugerida] nvarchar(max) NOT NULL,
        [IndiceMin] float NOT NULL,
        CONSTRAINT [PK_NiveisAcao] PRIMARY KEY ([Nivel])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Acoes] (
        [Id] bigint NOT NULL IDENTITY,
        [AmbienteId] nvarchar(32) NOT NULL,
        [Nivel] nvarchar(20) NOT NULL,
        [Descricao] nvarchar(500) NOT NULL,
        [RegistradaEm] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Acoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Acoes_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Dispositivos] (
        [Id] nvarchar(64) NOT NULL,
        [AmbienteId] nvarchar(32) NOT NULL,
        [Segredo] nvarchar(128) NOT NULL,
        [Ativo] bit NOT NULL,
        [UltimoMedidoEm] datetimeoffset NULL,
        CONSTRAINT [PK_Dispositivos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Dispositivos_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Ocorrencias] (
        [Id] uniqueidentifier NOT NULL,
        [AmbienteId] nvarchar(32) NOT NULL,
        [Descricao] nvarchar(500) NOT NULL,
        [CaminhoFoto] nvarchar(80) NOT NULL,
        [CriadaEm] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Ocorrencias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ocorrencias_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Previsoes] (
        [Id] bigint NOT NULL IDENTITY,
        [AmbienteId] nvarchar(32) NOT NULL,
        [PrevistoPara] datetimeoffset NOT NULL,
        [IndiceRuaPrevisto] float NOT NULL,
        [DeltaSala] float NULL,
        [IndiceSalaPrevisto] float NULL,
        [NivelPrevisto] nvarchar(20) NOT NULL,
        [SemLeiturasRecentes] bit NOT NULL,
        [CalculadaEm] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Previsoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Previsoes_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE TABLE [Leituras] (
        [Id] bigint NOT NULL IDENTITY,
        [AmbienteId] nvarchar(32) NOT NULL,
        [DispositivoId] nvarchar(64) NOT NULL,
        [TemperaturaC] float NOT NULL,
        [UmidadePct] int NOT NULL,
        [IndiceCalor] float NOT NULL,
        [Nivel] nvarchar(20) NOT NULL,
        [MedidoEm] datetimeoffset NOT NULL,
        [Origem] nvarchar(20) NOT NULL,
        [Suspeita] bit NOT NULL,
        CONSTRAINT [PK_Leituras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Leituras_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Leituras_Dispositivos_DispositivoId] FOREIGN KEY ([DispositivoId]) REFERENCES [Dispositivos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Andar', N'FaceSol', N'Nome', N'TemVentilador', N'Tipo') AND [object_id] = OBJECT_ID(N'[Ambientes]'))
        SET IDENTITY_INSERT [Ambientes] ON;
    EXEC(N'INSERT INTO [Ambientes] ([Id], [Andar], [FaceSol], [Nome], [TemVentilador], [Tipo])
    VALUES (N''sala-01'', 0, N''leste'', N''Sala 1'', CAST(1 AS bit), N''sala''),
    (N''sala-02'', 0, N''norte'', N''Sala 2'', CAST(1 AS bit), N''sala''),
    (N''sala-03'', 1, N''oeste'', N''Sala 3'', CAST(1 AS bit), N''sala''),
    (N''sala-04'', 2, N''sul'', N''Sala 4'', CAST(0 AS bit), N''sala'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Andar', N'FaceSol', N'Nome', N'TemVentilador', N'Tipo') AND [object_id] = OBJECT_ID(N'[Ambientes]'))
        SET IDENTITY_INSERT [Ambientes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Nivel', N'AcaoSugerida', N'IndiceMin', N'Nome') AND [object_id] = OBJECT_ID(N'[NiveisAcao]'))
        SET IDENTITY_INSERT [NiveisAcao] ON;
    EXEC(N'INSERT INTO [NiveisAcao] ([Nivel], [AcaoSugerida], [IndiceMin], [Nome])
    VALUES (N''atencao'', N''liberar água, abrir janelas e ligar ventiladores'', 27.0E0, N''Atenção''),
    (N''cuidado'', N''pausa, troca de sala ou adiar a Educação Física'', 32.0E0, N''Cuidado extremo''),
    (N''normal'', N''nenhuma'', 0.0E0, N''Normal''),
    (N''perigo'', N''tirar a turma da sala'', 41.0E0, N''Perigo'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Nivel', N'AcaoSugerida', N'IndiceMin', N'Nome') AND [object_id] = OBJECT_ID(N'[NiveisAcao]'))
        SET IDENTITY_INSERT [NiveisAcao] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AmbienteId', N'Ativo', N'Segredo', N'UltimoMedidoEm') AND [object_id] = OBJECT_ID(N'[Dispositivos]'))
        SET IDENTITY_INSERT [Dispositivos] ON;
    EXEC(N'INSERT INTO [Dispositivos] ([Id], [AmbienteId], [Ativo], [Segredo], [UltimoMedidoEm])
    VALUES (N''esp32-sala-01'', N''sala-01'', CAST(1 AS bit), N''segredo-de-teste-sala-03'', NULL),
    (N''esp32-sala-02'', N''sala-02'', CAST(1 AS bit), N''segredo-de-teste-sala-03'', NULL),
    (N''esp32-sala-03'', N''sala-03'', CAST(1 AS bit), N''segredo-de-teste-sala-03'', NULL),
    (N''esp32-sala-04'', N''sala-04'', CAST(1 AS bit), N''segredo-de-teste-sala-03'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AmbienteId', N'Ativo', N'Segredo', N'UltimoMedidoEm') AND [object_id] = OBJECT_ID(N'[Dispositivos]'))
        SET IDENTITY_INSERT [Dispositivos] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Acoes_AmbienteId] ON [Acoes] ([AmbienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Dispositivos_AmbienteId] ON [Dispositivos] ([AmbienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Leituras_AmbienteId_MedidoEm] ON [Leituras] ([AmbienteId], [MedidoEm]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Leituras_DispositivoId] ON [Leituras] ([DispositivoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_LeiturasRua_MedidoEm] ON [LeiturasRua] ([MedidoEm]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ocorrencias_AmbienteId] ON [Ocorrencias] ([AmbienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    CREATE INDEX [IX_Previsoes_AmbienteId] ON [Previsoes] ([AmbienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105025544_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105025544_Inicial', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Previsoes]') AND [c].[name] = N'NivelPrevisto');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Previsoes] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Previsoes] ALTER COLUMN [NivelPrevisto] nvarchar(7) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NiveisAcao]') AND [c].[name] = N'Nivel');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [NiveisAcao] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [NiveisAcao] ALTER COLUMN [Nivel] nvarchar(7) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Leituras]') AND [c].[name] = N'Origem');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Leituras] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Leituras] ALTER COLUMN [Origem] nvarchar(11) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Leituras]') AND [c].[name] = N'Nivel');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Leituras] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Leituras] ALTER COLUMN [Nivel] nvarchar(7) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ambientes]') AND [c].[name] = N'Tipo');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Ambientes] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Ambientes] ALTER COLUMN [Tipo] nvarchar(4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ambientes]') AND [c].[name] = N'FaceSol');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Ambientes] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Ambientes] ALTER COLUMN [FaceSol] nvarchar(5) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Acoes]') AND [c].[name] = N'Nivel');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Acoes] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Acoes] ALTER COLUMN [Nivel] nvarchar(7) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105033613_Enums'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105033613_Enums', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105050515_SegredosPorDispositivo'
)
BEGIN
    EXEC(N'UPDATE [Dispositivos] SET [Segredo] = N''segredo-de-teste-sala-01''
    WHERE [Id] = N''esp32-sala-01'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105050515_SegredosPorDispositivo'
)
BEGIN
    EXEC(N'UPDATE [Dispositivos] SET [Segredo] = N''segredo-de-teste-sala-02''
    WHERE [Id] = N''esp32-sala-02'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105050515_SegredosPorDispositivo'
)
BEGIN
    EXEC(N'UPDATE [Dispositivos] SET [Segredo] = N''segredo-de-teste-sala-04''
    WHERE [Id] = N''esp32-sala-04'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105050515_SegredosPorDispositivo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105050515_SegredosPorDispositivo', N'8.0.11');
END;
GO

COMMIT;
GO

