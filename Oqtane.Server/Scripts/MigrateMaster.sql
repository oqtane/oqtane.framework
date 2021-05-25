IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.SchemaVersions') AND OBJECTPROPERTY(id, N'IsTable') = 1)
    BEGIN
        IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.__EFMigrationsHistory') AND OBJECTPROPERTY(id, N'IsTable') = 1)
            BEGIN
                CREATE TABLE __EFMigrationsHistory
                (
                    MigrationId    nvarchar(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
                    ProductVersion nvarchar(32)  NOT NULL,
                    AppliedVersion nvarchar(10) NOT NULL,
                    AppliedDate datetime DEFAULT GETDATE()
                )
            END
        INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion, AppliedVersion)
            VALUES ('Master.01.00.00.00', '5.0.0', '{{Version}}')
        INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion, AppliedVersion)
            SELECT REPLACE(REPLACE(ScriptName, 'Oqtane.Scripts.', ''), '.sql', '') As MigrationId, 
                   ProductVersion = '5.0.0',
                   AppliedVersion = '{{Version}}'
                FROM SchemaVersions
                WHERE ScriptName LIKE 'Oqtane.Scripts.Master.01%'
    END