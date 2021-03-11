/*  

Version 2.0.1 Tenant migration script

*/

DELETE FROM [dbo].[Page]
WHERE Path = 'admin/tenants';
GO

ALTER TABLE [dbo].[Site] ADD
	[AdminContainerType] [nvarchar](200) NULL
GO

UPDATE [dbo].[Site] SET AdminContainerType = ''
GO

