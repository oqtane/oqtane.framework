/*  

Version 0.9.1 migration script

*/

ALTER TABLE [dbo].[Module] ADD
	[AllPages] [bit] NULL
GO

UPDATE [dbo].[Module]
SET [AllPages] = 0
GO

