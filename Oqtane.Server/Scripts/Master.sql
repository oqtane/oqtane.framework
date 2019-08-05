/*  

Create tables

*/
CREATE TABLE [dbo].[Alias](
	[AliasId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[TenantId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Alias] PRIMARY KEY CLUSTERED 
  (
	[AliasId] ASC
  )
)
GO

CREATE TABLE [dbo].[Tenant](
	[TenantId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DBConnectionString] [nvarchar](1024) NOT NULL,
	[DBSchema] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Tenant] PRIMARY KEY CLUSTERED 
  (
	[TenantId] ASC
  )
)
GO


/*  

Create foreign key relationships

*/
ALTER TABLE [dbo].[Alias]  WITH CHECK ADD  CONSTRAINT [FK_Alias_Tenant] FOREIGN KEY([TenantId])
REFERENCES [dbo].[Tenant] ([TenantId])
ON DELETE CASCADE
GO

/*  

Create seed data

*/
SET IDENTITY_INSERT [dbo].[Tenant] ON 
GO
INSERT [dbo].[Tenant] ([TenantId], [Name], [DBConnectionString], [DBSchema], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'Tenant1', N'{ConnectionString}', N'', '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Tenant] OFF
GO

SET IDENTITY_INSERT [dbo].[Alias] ON 
GO
INSERT [dbo].[Alias] ([AliasId], [Name], [TenantId], [SiteId], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'{Alias}', 1, 1, '', getdate(), '', getdate())
GO
INSERT [dbo].[Alias] ([AliasId], [Name], [TenantId], [SiteId], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, N'{Alias}/site2', 1, 2, '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Alias] OFF
GO

