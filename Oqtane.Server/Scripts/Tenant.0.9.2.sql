/*  

Version 0.9.2 migration script

*/

ALTER TABLE [dbo].[Role]
ALTER COLUMN [Description] VARCHAR (256) NOT NULL
GO

ALTER TABLE [dbo].[Page] ADD
	[DefaultContainerType] [nvarchar](200) NULL
GO

UPDATE [dbo].[Page]
SET [DefaultContainerType] = ''
GO

