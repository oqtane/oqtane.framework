/*  

Create tables

*/

CREATE TABLE [dbo].[Site](
	[SiteId] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[LogoFileId] [int] NULL,
	[FaviconFileId] [int] NULL,
	[DefaultThemeType] [nvarchar](200) NOT NULL,
	[DefaultLayoutType] [nvarchar](200) NOT NULL,
	[DefaultContainerType] [nvarchar](200) NOT NULL,
	[PwaIsEnabled] [bit] NOT NULL,
	[PwaAppIconFileId] [int] NULL,
	[PwaSplashIconFileId] [int] NULL,
	[AllowRegistration] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
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
	[Title] [nvarchar](200) NULL,
	[ThemeType] [nvarchar](200) NULL,
	[Icon] [nvarchar](50) NOT NULL,
	[ParentId] [int] NULL,
	[Order] [int] NOT NULL,
	[IsNavigation] [bit] NOT NULL,
	[Url] [nvarchar](500) NULL,
	[LayoutType] [nvarchar](200) NOT NULL,
	[EditMode] [bit] NOT NULL,
	[UserId] [int] NULL,
	[IsPersonalizable] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
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
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
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
	[PhotoFileId] [int] NULL,
	[LastLoginOn] [datetime] NULL,
	[LastIPAddress] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
  CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
  (
	[UserId] ASC
  )
)
GO

CREATE TABLE [dbo].[Role](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[IsAutoAssigned] [bit] NOT NULL,
	[IsSystem] [bit] NOT NULL,
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
	[Title] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](256) NULL,
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

CREATE TABLE [dbo].[Log] (

   [LogId] [int] IDENTITY(1,1) NOT NULL,
   [SiteId] [int] NULL,
   [LogDate] [datetime] NOT NULL,
   [PageId] [int] NULL,
   [ModuleId] [int] NULL,
   [UserId] [int] NULL,
   [Url] [nvarchar](2048) NOT NULL,
   [Server] [nvarchar](200) NOT NULL,
   [Category] [nvarchar](200) NOT NULL,
   [Feature] [nvarchar](200) NOT NULL,
   [Function] [nvarchar](20) NOT NULL,
   [Level] [nvarchar](20) NOT NULL,
   [Message] [nvarchar](max) NOT NULL,
   [MessageTemplate] [nvarchar](max) NOT NULL,
   [Exception] [nvarchar](max) NULL,
   [Properties] [nvarchar](max) NULL

   CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
   (
     [LogId] ASC
   ) 
)
GO

CREATE TABLE [dbo].[Notification](
	[NotificationId] [int] IDENTITY(1,1) NOT NULL,
    [SiteId] [int] NOT NULL,
	[FromUserId] [int] NULL,
	[ToUserId] [int] NULL,
	[ToEmail] [nvarchar](256) NOT NULL,
	[Subject] [nvarchar](256) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[ParentId] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[IsDelivered] [bit] NOT NULL,
	[DeliveredOn] [datetime] NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
  CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
  (
	[NotificationId] ASC
  )
)
GO

CREATE TABLE [dbo].[Folder](
	[FolderId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Path] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ParentId] [int] NULL,
	[Order] [int] NOT NULL,
	[IsSystem] [bit] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
  CONSTRAINT [PK_Folder] PRIMARY KEY CLUSTERED 
  (
	[FolderId] ASC
  )
)
GO

CREATE TABLE [dbo].[File](
	[FileId] [int] IDENTITY(1,1) NOT NULL,
	[FolderId] [int] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Extension] [nvarchar](50) NOT NULL,
	[Size] [int] NOT NULL,
	[ImageHeight] [int] NOT NULL,
	[ImageWidth] [int] NOT NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedBy] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[DeletedBy] [nvarchar](256) NULL,
	[DeletedOn] [datetime] NULL,
	[IsDeleted][bit] NOT NULL,
  CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED 
  (
	[FileId] ASC
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

ALTER TABLE [dbo].[Log] WITH CHECK ADD CONSTRAINT [FK_Log_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Notification] WITH CHECK ADD CONSTRAINT [FK_Notification_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Folder] WITH CHECK ADD CONSTRAINT [FK_Folder_Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Site] ([SiteId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[File] WITH CHECK ADD CONSTRAINT [FK_File_Folder] FOREIGN KEY([FolderId])
REFERENCES [dbo].[Folder] ([FolderId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[HtmlText] WITH CHECK ADD CONSTRAINT [FK_HtmlText_Module] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[Module] ([ModuleId])
ON DELETE CASCADE
GO


/*  

Create indexes

*/

CREATE UNIQUE NONCLUSTERED INDEX IX_Setting ON [dbo].Setting
	(
	EntityName,
	EntityId,
	SettingName
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_User ON [dbo].[User]
	(
	Username
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Permission ON [dbo].Permission
	(
	SiteId,
	EntityName,
	EntityId,
	PermissionName,
	RoleId,
	UserId
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Page ON [dbo].Page
	(
	SiteId,
	[Path],
	UserId
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_UserRole ON [dbo].UserRole
	(
	RoleId,
	UserId
	) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Folder ON [dbo].Folder
	(
	SiteId,
	[Path]
	) ON [PRIMARY]
GO

/*  

ASP.NET Identity Minimal Schema

*/

CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
