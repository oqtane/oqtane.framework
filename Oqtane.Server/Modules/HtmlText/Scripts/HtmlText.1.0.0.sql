CREATE TABLE [dbo].[HtmlText](
	[HtmlTextId] [int] IDENTITY(1,1) NOT NULL,
	[ModuleId] [int] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_HtmlText] PRIMARY KEY CLUSTERED 
  (
	[HtmlTextId] ASC
  )
)
GO

ALTER TABLE [dbo].[HtmlText] WITH CHECK ADD CONSTRAINT [FK_HtmlText_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
ON DELETE CASCADE
GO
