/*  

Version 2.0.0 Tenant migration script

*/

CREATE TABLE [dbo].[Language](
	[LanguageId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
    [Code] [nvarchar](10) NOT NULL,
    [IsCurrent] [bit] NOT NULL,
    [SiteId] [int],
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Language] PRIMARY KEY CLUSTERED 
  (
	[LanguageId] ASC
  )
)
GO

ALTER TABLE [dbo].[Language]  WITH CHECK ADD  CONSTRAINT [FK_Language_Tenant] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Tenant] ([TenantId])
ON DELETE CASCADE
GO