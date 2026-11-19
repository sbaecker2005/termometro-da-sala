BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    ALTER TABLE [Dispositivos] ADD [PosX] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    ALTER TABLE [Dispositivos] ADD [PosY] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    ALTER TABLE [Dispositivos] ADD [Rotulo] nvarchar(80) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    ALTER TABLE [Ambientes] ADD [LarguraM] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    ALTER TABLE [Ambientes] ADD [ProfundidadeM] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    CREATE TABLE [Lugares] (
        [Id] bigint NOT NULL IDENTITY,
        [AmbienteId] nvarchar(32) NOT NULL,
        [Nome] nvarchar(80) NOT NULL,
        [X] float NOT NULL,
        [Y] float NOT NULL,
        [Vulneravel] bit NOT NULL,
        CONSTRAINT [PK_Lugares] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lugares_Ambientes_AmbienteId] FOREIGN KEY ([AmbienteId]) REFERENCES [Ambientes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lugares_AmbienteId_Nome] ON [Lugares] ([AmbienteId], [Nome]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107091447_MapaTermico'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107091447_MapaTermico', N'8.0.11');
END;
GO

COMMIT;
GO

