/*  

Version 2.0.2 Tenant migration script

*/

ALTER TABLE [dbo].[Setting] ALTER COLUMN [SettingName] [nvarchar](200) NOT NULL
GO

ALTER TABLE [dbo].[Site]
	DROP COLUMN DefaultLayoutType
GO

ALTER TABLE [dbo].[Page]
	DROP COLUMN LayoutType
GO

UPDATE [dbo].[Site] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client';
GO
UPDATE [dbo].[Site] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client';
GO

UPDATE [dbo].[Page] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client';
GO
UPDATE [dbo].[Page] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client';
GO

UPDATE [dbo].[PageModule] SET ContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client';
GO
UPDATE [dbo].[PageModule] SET ContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client' WHERE ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client';
GO
