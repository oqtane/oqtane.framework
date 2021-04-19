/*  

Version 2.0.2 Tenant migration script

*/

ALTER TABLE [dbo].[Site] ADD
	[SiteGuid] [char](36) NULL
GO

