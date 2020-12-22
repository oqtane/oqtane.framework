/*  

Version 2.0.1 Tenant migration script

*/

UPDATE [dbo].[Page] SET Icon = IIF(Icon <> '', 'oi oi-' + Icon, '');
GO

