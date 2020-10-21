/*  

Version 2.0.0 Tenant migration script

*/

ALTER TABLE  [dbo].[Page] 
ALTER COLUMN [Path] [nvarchar](256) NOT NULL
GO

ALTER TABLE [dbo].[Profile] ADD
	[Options] [nvarchar](2000) NULL
GO

UPDATE [dbo].[Profile] SET Options = ''
GO