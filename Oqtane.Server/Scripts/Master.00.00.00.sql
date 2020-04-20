/*  

Create tables

*/
CREATE TABLE [dbo].[Tenant](
	[TenantId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DBConnectionString] [nvarchar](1024) NOT NULL,
	[DBSchema] [nvarchar](50) NOT NULL,
	[IsInitialized] [bit] NOT NULL,
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

CREATE TABLE [dbo].[ApplicationVersion](
	[ApplicationVersionId] [int] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](50) NOT NULL,
	[CreatedOn] [datetime] NOT NULL
  CONSTRAINT [PK_ApplicationVersion] PRIMARY KEY CLUSTERED 
  (
	[ApplicationVersionId] ASC
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

/*  

Create seed data

*/
SET IDENTITY_INSERT [dbo].[Tenant] ON 
GO
INSERT [dbo].[Tenant] ([TenantId], [Name], [DBConnectionString], [DBSchema], [IsInitialized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'Master', N'$ConnectionString$', N'', 0, '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Tenant] OFF
GO

SET IDENTITY_INSERT [dbo].[Alias] ON 
GO
INSERT [dbo].[Alias] ([AliasId], [Name], [TenantId], [SiteId], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'$Alias$', 1, 1, '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Alias] OFF
GO

SET IDENTITY_INSERT [dbo].[Job] ON 
GO
INSERT [dbo].[Job] ([JobId], [Name], [JobType], [Frequency], [Interval], [StartDate], [EndDate], [IsEnabled], [IsStarted], [IsExecuting], [NextExecution], [RetentionHistory],  [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'Notification Job', N'Oqtane.Infrastructure.NotificationJob, Oqtane.Server', N'm', 1, null, null, 0, 0, 0, null, 10, '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Job] OFF
GO

