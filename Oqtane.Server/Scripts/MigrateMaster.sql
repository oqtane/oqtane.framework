IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.SchemaVersions') AND OBJECTPROPERTY(id, N'IsTable') = 1)
    BEGIN
        IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.__EFMigrationsHistory') AND OBJECTPROPERTY(id, N'IsTable') = 1)
            BEGIN
                CREATE TABLE __EFMigrationsHistory
                (
                    MigrationId    nvarchar(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
                    ProductVersion nvarchar(32)  NOT NULL
                )
            END
        INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion)
            VALUES ('Master.01.00.00.00', '5.0.0')
        INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion)
            SELECT REPLACE(REPLACE(ScriptName, 'Oqtane.Scripts.', ''), '.sql', '') As MigrationId, ProductVersion = '5.0.0'
                FROM SchemaVersions
                WHERE ScriptName LIKE 'Oqtane.Scripts.Master.01%'
    END