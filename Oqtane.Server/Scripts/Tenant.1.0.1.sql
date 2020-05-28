/*  

Version 1.0.1 Tenant migration script

*/

CREATE UNIQUE NONCLUSTERED INDEX IX_Site ON [dbo].[Site]
	(
	[TenantId],
	[Name]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Role ON [dbo].[Role]
	(
	[SiteId],
	[Name]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Profile ON [dbo].[Profile]
	(
	[SiteId],
	[Name]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_File ON [dbo].[File]
	(
	[FolderId],
	[Name]
	) ON [PRIMARY]
GO
