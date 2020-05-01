/*  

Create tables

*/
CREATE TABLE [dbo].[Tenant](
	[TenantId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DBConnectionString] [nvarchar](1024) NOT NULL,
	[Version] [nvarchar](50) NULL,
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


CREATE TABLE [dbo].[ModuleDefinition](
	[ModuleDefinitionId] [int] IDENTITY(1,1) NOT NULL,
	[ModuleDefinitionName] [nvarchar](200) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Description] [nvarchar](2000) NULL,
	[Categories] [nvarchar](200) NULL,
	[Version] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_ModuleDefinition] PRIMARY KEY CLUSTERED 
  (
	[ModuleDefinitionId] ASC
  )
)
GO

CREATE TABLE [dbo].[Job] (
	[JobId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[JobType] [nvarchar](200) NOT NULL,
	[Frequency] [char](1) NOT NULL,
	[Interval] [int] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[IsEnabled] [bit] NOT NULL,
	[IsStarted] [bit] NOT NULL,
	[IsExecuting] [bit] NOT NULL,
	[NextExecution] [datetime] NULL,
	[RetentionHistory] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
    CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED 
    (
	  [JobId] ASC
    )
)
GO

CREATE TABLE [dbo].[JobLog] (
	[JobLogId] [int] IDENTITY(1,1) NOT NULL,
	[JobId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[FinishDate] [datetime] NULL,
	[Succeeded] [bit] NULL,
	[Notes] [nvarchar](max) NULL,
    CONSTRAINT [PK_JobLog] PRIMARY KEY CLUSTERED 
    (
	  [JobLogId] ASC
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

ALTER TABLE [dbo].[JobLog]  WITH NOCHECK ADD CONSTRAINT [FK_JobLog_Job] FOREIGN KEY([JobId])
REFERENCES [dbo].[Job] ([JobId])
ON DELETE CASCADE
GO


