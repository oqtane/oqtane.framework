/*  

Create tables

*/

CREATE TABLE [dbo].[Site](
	[SiteId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Logo] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Site] PRIMARY KEY CLUSTERED 
  (
	[SiteId] ASC
  )
)
GO

CREATE TABLE [dbo].[Page](
	[PageId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Path] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ThemeType] [nvarchar](200) NULL,
	[Icon] [nvarchar](50) NOT NULL,
	[Panes] [nvarchar](50) NOT NULL,
	[ParentId] [int] NULL,
	[Order] [int] NOT NULL,
	[IsNavigation] [bit] NOT NULL,
	[LayoutType] [nvarchar](200) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Page] PRIMARY KEY CLUSTERED 
  (
	[PageId] ASC
  )
)
GO

CREATE TABLE [dbo].[Module](
	[ModuleId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[ModuleDefinitionName] [nvarchar](200) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Module] PRIMARY KEY CLUSTERED 
  (
	[ModuleId] ASC
  )
)
GO

CREATE TABLE [dbo].[PageModule](
	[PageModuleId] [int] IDENTITY(1,1) NOT NULL,
	[PageId] [int] NOT NULL,
	[ModuleId] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Pane] [nvarchar](50) NOT NULL,
	[Order] [int] NOT NULL,
	[ContainerType] [nvarchar](200) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_PageModule] PRIMARY KEY CLUSTERED 
  (
	[PageModuleId] ASC
  )
)
GO

CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](256) NOT NULL,
	[DisplayName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[IsHost] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
  (
	[UserId] ASC
  )
)
GO

CREATE TABLE [dbo].[SiteUser](
	[SiteUserId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_SiteUser] PRIMARY KEY CLUSTERED 
  (
	[SiteUserId] ASC
  )
)
GO

CREATE TABLE [dbo].[Role](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[IsAutoAssigned] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
  (
	[RoleId] ASC
  )
)
GO

CREATE TABLE [dbo].[UserRole](
	[UserRoleId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[EffectiveDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED 
  (
	[UserRoleId] ASC
  )
)
GO

CREATE TABLE [dbo].[Permission](
	[PermissionId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[EntityName] [nvarchar](50) NOT NULL,
	[EntityId] [int] NOT NULL,
	[PermissionName] [nvarchar](50) NOT NULL,
	[RoleId] [int] NULL,
	[UserId] [int] NULL,
	[IsAuthorized] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Permission] PRIMARY KEY CLUSTERED 
  (
	[PermissionId] ASC
  )
)
GO

CREATE TABLE [dbo].[Setting](
	[SettingId] [int] IDENTITY(1,1) NOT NULL,
	[EntityName] [nvarchar](50) NOT NULL,
	[EntityId] [int] NOT NULL,
	[SettingName] [nvarchar](50) NOT NULL,
	[SettingValue] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED 
  (
	[SettingId] ASC
  )
)
GO

CREATE TABLE [dbo].[Profile](
	[ProfileId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[ViewOrder] [int] NOT NULL,
	[MaxLength] [int] NOT NULL,
	[DefaultValue] [nvarchar](2000) NULL,
	[IsRequired] [bit] NOT NULL,
	[IsPrivate] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
  CONSTRAINT [PK_Profile] PRIMARY KEY CLUSTERED 
  (
	[ProfileId] ASC
  )
)

GO

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

/*  

Create foreign key relationships

*/
ALTER TABLE [dbo].[Module] WITH CHECK ADD CONSTRAINT [FK_Module_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Page] WITH CHECK ADD CONSTRAINT [FK_Page_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PageModule] WITH CHECK ADD CONSTRAINT [FK_PageModule_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
GO

ALTER TABLE [dbo].[PageModule] WITH CHECK ADD CONSTRAINT [FK_PageModule_Page] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([PageId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[SiteUser] WITH CHECK ADD CONSTRAINT [FK_SiteUser_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[SiteUser] WITH CHECK ADD CONSTRAINT [FK_SiteUser_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO

ALTER TABLE [dbo].[HtmlText] WITH CHECK ADD CONSTRAINT [FK_HtmlText_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Role]  WITH CHECK ADD CONSTRAINT [FK_Role_Site] FOREIGN KEY ([SiteId]) 
REFERENCES [dbo].[Site] ([SiteId]) 
ON DELETE CASCADE 
GO

ALTER TABLE [dbo].[UserRole] WITH CHECK ADD CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) 
REFERENCES [dbo].[User] ([UserId]) 
ON DELETE CASCADE 
GO

ALTER TABLE [dbo].[UserRole] WITH CHECK ADD CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) 
REFERENCES [dbo].[Role] ([RoleId]) 
GO

ALTER TABLE [dbo].[Permission] WITH CHECK ADD CONSTRAINT [FK_Permission_Site] FOREIGN KEY ([SiteId]) 
REFERENCES [dbo].[Site] ([SiteId]) 
ON DELETE CASCADE 
GO

ALTER TABLE [dbo].[Permission] WITH CHECK ADD CONSTRAINT [FK_Permission_User] FOREIGN KEY ([UserId]) 
REFERENCES [dbo].[User] ([UserId]) 	
GO

ALTER TABLE [dbo].[Permission] WITH CHECK ADD CONSTRAINT [FK_Permission_Role] FOREIGN KEY ([RoleId]) 
REFERENCES [dbo].[Role] ([RoleId]) 
GO

ALTER TABLE [dbo].[Profile] WITH NOCHECK ADD CONSTRAINT [FK_Profile_Sites] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

/*  

Create indexes

*/

CREATE UNIQUE NONCLUSTERED INDEX IX_Setting ON dbo.Setting
	(
	EntityName,
	EntityId,
	SettingName
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_User ON dbo.[User]
	(
	Username
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_SiteUser ON dbo.SiteUser
	(
	SiteId,
	UserId
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Permission ON dbo.Permission
	(
	SiteId,
	EntityName,
	EntityId,
	PermissionName,
	RoleId,
	UserId
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Page ON dbo.Page
	(
	SiteId,
	[Path]
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_UserRole ON dbo.UserRole
	(
	RoleId,
	UserId
	) ON [PRIMARY]
GO

/*  

Create seed data

*/

SET IDENTITY_INSERT [dbo].[Site] ON 
GO
INSERT [dbo].[Site] ([SiteId], [Name], [Logo], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, N'Site1', N'oqtane.png', '', getdate(), '', getdate())
GO
INSERT [dbo].[Site] ([SiteId], [Name], [Logo], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, N'Site2', N'oqtane.png', '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Site] OFF
GO

SET IDENTITY_INSERT [dbo].[Role] ON 
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (-1, null, N'All Users', N'All Users', 0, '', getdate(), '', getdate())
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (0, null, N'Super Users', N'Super Users', 0, '', getdate(), '', getdate())
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, 1, N'Administrators', N'Site Administrators', 0, '', getdate(), '', getdate())
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, 1, N'Registered Users', N'Registered Users', 1, '', getdate(), '', getdate())
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (3, 2, N'Administrators', N'Site Administrators', 0, '', getdate(), '', getdate())
GO
INSERT [dbo].[Role] ([RoleId], [SiteId], [Name], [Description], [IsAutoAssigned], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (4, 2, N'Registered Users', N'Registered Users', 1, '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[Role] OFF 
GO

SET IDENTITY_INSERT [dbo].[Page] ON 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, 1, N'Page1', N'', N'Oqtane.Client.Themes.Theme1.Theme1, Oqtane.Client', N'oi-home', N'Left;Right', NULL, 1, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 1, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 1, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 1, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, 1, N'Page2', N'page2', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'oi-plus', N'Top;Bottom', NULL, 3, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 2, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 2, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (3, 1, N'Page3', N'page3', N'Oqtane.Client.Themes.Theme3.Theme3, Oqtane.Client', N'oi-list-rich', N'Left;Right', NULL, 3, 1, N'Oqtane.Client.Themes.Theme3.HorizontalLayout, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 3, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 3, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 3, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (4, 1, N'Admin', N'admin', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'oi-home', N'Top;Bottom', NULL, 7, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 4, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 4, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (5, 1, N'Page Management', N'admin/pages', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 1, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 5, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 5, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (6, 1, N'Login', N'login', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', NULL, 1, 0, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 6, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 6, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 6, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (7, 1, N'Register', N'register', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', NULL, 1, 0, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 7, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 7, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 7, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (8, 1, N'Site Management', N'admin/sites', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 0, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 8, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 8, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (9, 1, N'User Management', N'admin/users', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 2, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 9, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 9, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (10, 1, N'Module Management', N'admin/modules', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 3, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 10, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 10, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (11, 1, N'Theme Management', N'admin/themes', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 4, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 11, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 11, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (12, 2, N'Page1', N'', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'oi-home', N'Top;Bottom', NULL, 1, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 12, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 12, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 12, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (13, 2, N'Page2', N'page2', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'oi-home', N'Top;Bottom', NULL, 1, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 13, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 13, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 13, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (14, 2, N'Login', N'login', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', NULL, 1, 0, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 14, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 14, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 14, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (15, 2, N'Register', N'register', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', NULL, 1, 0, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 15, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 15, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Page', 15, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Page] ([PageId], [SiteId], [Name], [Path], [ThemeType], [Icon], [Panes], [ParentId], [Order], [IsNavigation], [LayoutType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (16, 1, N'Role Management', N'admin/roles', N'Oqtane.Client.Themes.Theme2.Theme2, Oqtane.Client', N'', N'Top;Bottom', 4, 5, 1, N'', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 16, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Page', 16, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
SET IDENTITY_INSERT [dbo].[Page] OFF 
GO

SET IDENTITY_INSERT [dbo].[Module] ON 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, 1, N'Oqtane.Client.Modules.Weather, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 1, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 1, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 1, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, 1, N'Oqtane.Client.Modules.Counter, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 2, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 2, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 2, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (3, 1, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 3, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 3, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 3, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (4, 1, N'Oqtane.Client.Modules.Weather, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 4, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 4, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 4, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (5, 1, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 5, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 5, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 5, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (6, 1, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 6, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 6, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 6, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (7, 1, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 7, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 7, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (8, 1, N'Oqtane.Client.Modules.Admin.Pages, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 8, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 8, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (9, 1, N'Oqtane.Client.Modules.Admin.Login, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 9, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 9, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 9, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (10, 1, N'Oqtane.Client.Modules.Admin.Register, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 10, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 10, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 10, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (11, 1, N'Oqtane.Client.Modules.Admin.Admin, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 11, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 11, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (12, 1, N'Oqtane.Client.Modules.Admin.Sites, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 12, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 12, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (13, 1, N'Oqtane.Client.Modules.Admin.Users, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 13, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 13, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (14, 1, N'Oqtane.Client.Modules.Admin.ModuleDefinitions, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 14, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 14, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (15, 1, N'Oqtane.Client.Modules.Admin.Themes, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 15, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 15, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (16, 2, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 16, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 16, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 16, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (17, 2, N'Oqtane.Client.Modules.HtmlText, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 17, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 17, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 17, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (18, 2, N'Oqtane.Client.Modules.Admin.Login, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 18, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 18, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 18, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (19, 2, N'Oqtane.Client.Modules.Admin.Register, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 19, 'View', -1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 19, 'View', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (2, 'Module', 19, 'Edit', 3, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Module] ([ModuleId], [SiteId], [ModuleDefinitionName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (20, 1, N'Oqtane.Client.Modules.Admin.Roles, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 20, 'View', 1, null, 1, '', getdate(), '', getdate()) 
GO
INSERT [dbo].[Permission] ([SiteId], [EntityName], [EntityId], [PermissionName], [RoleId], [UserId], [IsAuthorized], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) VALUES (1, 'Module', 20, 'Edit', 1, null, 1, '', getdate(), '', getdate()) 
GO
SET IDENTITY_INSERT [dbo].[Module] OFF 
GO

SET IDENTITY_INSERT [dbo].[PageModule] ON 
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, 1, 1, N'Weather', N'Right', 0, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, 1, 2, N'Counter', N'Left', 1, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (3, 1, 3, N'Lorem ipsum', N'Left', 0, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (4, 2, 4, N'Weather', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (5, 2, 5, N'Enim sed', N'Top', 1, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (6, 3, 6, N'Id consectetur', N'Left', 0, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (7, 3, 7, N'Ornare arcu', N'Right', 0, N'Oqtane.Client.Themes.Theme1.Container1, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (8, 5, 8, N'Page Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (9, 6, 9, N'Login', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (10, 7, 10, N'Register', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (11, 4, 11, N'Administration', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (12, 8, 12, N'Site Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (13, 9, 13, N'User Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (14, 10, 14, N'Module Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (15, 11, 15, N'Theme Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (16, 12, 16, N'Id consectetur', N'Top', 1, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (17, 13, 17, N'Lorem ipsum', N'Top', 1, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (18, 14, 18, N'Login', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (19, 15, 19, N'Register', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
INSERT [dbo].[PageModule] ([PageModuleId], [PageId], [ModuleId], [Title], [Pane], [Order], [ContainerType], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (20, 16, 20, N'Role Management', N'Top', 0, N'Oqtane.Client.Themes.Theme2.Container2, Oqtane.Client', '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[PageModule] OFF 
GO

SET IDENTITY_INSERT [dbo].[HtmlText] ON 
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (1, 3, N'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. <br /><br /><a href="http://localhost:44357/site2/">Go To Site2</a>', '', getdate(), '', getdate())
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (2, 5, N'Enim sed faucibus turpis in eu mi bibendum neque egestas. Quis hendrerit dolor magna eget est lorem. Dui faucibus in ornare quam viverra orci sagittis. Integer eget aliquet nibh praesent tristique magna sit. Nunc aliquet bibendum enim facilisis gravida neque convallis a cras. Tortor id aliquet lectus proin. Diam volutpat commodo sed egestas egestas fringilla. Posuere sollicitudin aliquam ultrices sagittis orci. Viverra mauris in aliquam sem fringilla ut morbi tincidunt. Eget gravida cum sociis natoque penatibus et. Sagittis orci a scelerisque purus semper. Eget velit aliquet sagittis id consectetur purus. Volutpat blandit aliquam etiam erat. Et tortor consequat id porta nibh venenatis cras. Volutpat odio facilisis mauris sit amet. Varius duis at consectetur lorem.', '', getdate(), '', getdate())
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (3, 6, N'Id consectetur purus ut faucibus pulvinar elementum integer. Bibendum neque egestas congue quisque egestas diam in arcu. Eget nullam non nisi est sit amet facilisis. Sit amet consectetur adipiscing elit pellentesque. Id aliquet risus feugiat in. Enim blandit volutpat maecenas volutpat blandit aliquam etiam erat. Commodo odio aenean sed adipiscing. Pharetra massa massa ultricies mi quis hendrerit dolor magna. Aliquet enim tortor at auctor urna nunc. Nulla pellentesque dignissim enim sit amet. Suscipit adipiscing bibendum est ultricies integer quis auctor. Lacinia quis vel eros donec ac odio tempor. Aliquam vestibulum morbi blandit cursus risus at.', '', getdate(), '', getdate())
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (4, 7, N'Ornare arcu dui vivamus arcu felis bibendum ut. Tortor vitae purus faucibus ornare. Lectus sit amet est placerat in egestas erat imperdiet sed. Aliquam sem et tortor consequat id. Fermentum iaculis eu non diam phasellus vestibulum. Ultricies integer quis auctor elit sed. Fermentum odio eu feugiat pretium nibh ipsum. Ut consequat semper viverra nam libero. Blandit aliquam etiam erat velit scelerisque in dictum non consectetur. At risus viverra adipiscing at in tellus. Facilisi nullam vehicula ipsum a arcu cursus vitae congue. At varius vel pharetra vel turpis nunc eget lorem dolor. Morbi non arcu risus quis varius. Turpis massa sed elementum tempus egestas.', '', getdate(), '', getdate())
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (5, 16, N'Id consectetur purus ut faucibus pulvinar elementum integer. Bibendum neque egestas congue quisque egestas diam in arcu. Eget nullam non nisi est sit amet facilisis. Sit amet consectetur adipiscing elit pellentesque. Id aliquet risus feugiat in. Enim blandit volutpat maecenas volutpat blandit aliquam etiam erat. Commodo odio aenean sed adipiscing. Pharetra massa massa ultricies mi quis hendrerit dolor magna. Aliquet enim tortor at auctor urna nunc. Nulla pellentesque dignissim enim sit amet. Suscipit adipiscing bibendum est ultricies integer quis auctor. Lacinia quis vel eros donec ac odio tempor. Aliquam vestibulum morbi blandit cursus risus at.  <br /><br /><a href="http://localhost:44357/">Go To Site1</a>', '', getdate(), '', getdate())
GO
INSERT [dbo].[HtmlText] ([HtmlTextId], [ModuleId], [Content], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn]) 
VALUES (6, 17, N'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.', '', getdate(), '', getdate())
GO
SET IDENTITY_INSERT [dbo].[HtmlText] OFF 
GO