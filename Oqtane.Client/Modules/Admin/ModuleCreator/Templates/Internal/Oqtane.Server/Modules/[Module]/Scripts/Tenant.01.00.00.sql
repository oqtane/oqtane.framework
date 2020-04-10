/*  
Create [Module] table
*/

CREATE TABLE [dbo].[[Module]](
	[[Module]Id] [int] IDENTITY(1,1) NOT NULL,
	[ModuleId] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_[Module]] PRIMARY KEY CLUSTERED 
  (
	[[Module]Id] ASC
  )
)
GO

/*  
Create foreign key relationships
*/
ALTER TABLE [dbo].[[Module]]  WITH CHECK ADD  CONSTRAINT [FK_[Module]_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].Module ([ModuleId])
ON DELETE CASCADE
GO