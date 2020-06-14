/*  

Version 1.0.1 Master migration script

*/

CREATE UNIQUE NONCLUSTERED INDEX IX_Tenant ON [dbo].[Tenant]
	(
	[Name]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Alias ON [dbo].[Alias]
	(
	[Name]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_ModuleDefinition ON [dbo].[ModuleDefinition]
	(
	[ModuleDefinitionName]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Job ON [dbo].[Job]
	(
	[JobType]
	) ON [PRIMARY]
GO
