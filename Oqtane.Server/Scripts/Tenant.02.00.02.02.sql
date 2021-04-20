/*  

Version 2.0.2 Tenant migration script

*/

UPDATE [dbo].[Site] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client';
GO
UPDATE [dbo].[Site] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client';
GO

UPDATE [dbo].[Page] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client';
GO
UPDATE [dbo].[Page] SET DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client' WHERE DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client';
GO

UPDATE [dbo].[PageModule] SET ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client' WHERE ContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client';
GO
UPDATE [dbo].[PageModule] SET ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client' WHERE ContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client';
GO
