/*  

migrate to new naming convention for scripts

*/

UPDATE [dbo].[SchemaVersions] SET ScriptName = 'Oqtane.Scripts.Tenant.00.09.00.00.sql' WHERE ScriptName = 'Oqtane.Scripts.Tenant.0.9.0.sql'
GO
UPDATE [dbo].[SchemaVersions] SET ScriptName = 'Oqtane.Scripts.Tenant.00.09.01.00.sql' WHERE ScriptName = 'Oqtane.Scripts.Tenant.0.9.1.sql'
GO
UPDATE [dbo].[SchemaVersions] SET ScriptName = 'Oqtane.Scripts.Tenant.00.09.02.00.sql' WHERE ScriptName = 'Oqtane.Scripts.Tenant.0.9.2.sql'
GO
UPDATE [dbo].[SchemaVersions] SET ScriptName = 'Oqtane.Scripts.Tenant.01.00.01.00.sql' WHERE ScriptName = 'Oqtane.Scripts.Tenant.1.0.1.sql'
GO
