using System;

namespace Oqtane.Shared
{
    public class Constants
    {
        public const string PackageId = "Oqtane.Framework";
        public const string Version = "1.0.1";
        public const string ReleaseVersions = "0.9.0,0.9.1,0.9.2,1.0.0,1.0.1";

        public const string PageComponent = "Oqtane.UI.ThemeBuilder, Oqtane.Client";
        public const string ContainerComponent = "Oqtane.UI.ContainerBuilder, Oqtane.Client";

        public const string DefaultTheme = "Oqtane.Themes.OqtaneTheme.Default, Oqtane.Client";
        public const string DefaultLayout = "Oqtane.Themes.OqtaneTheme.SinglePane, Oqtane.Client";
        public const string DefaultContainer = "Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client";
        public const string DefaultAdminContainer = "Oqtane.Themes.AdminContainer, Oqtane.Client";

        public const string ActionToken = "{Action}";
        public const string DefaultAction = "Index";
        public const string AdminPane = "Admin";
        public const string ModuleDelimiter = "*";
        public const string UrlParametersDelimiter = "!";

        // Default Module Actions are reserved and should not be used by modules
        public static readonly string[] DefaultModuleActions = new[] { "Settings", "Import", "Export" };
        public static readonly string DefaultModuleActionsTemplate = "Oqtane.Modules.Admin.Modules." + ActionToken + ", Oqtane.Client";

        public const string AdminDashboardModule = "Oqtane.Modules.Admin.Dashboard, Oqtane.Client";
        public const string PageManagementModule = "Oqtane.Modules.Admin.Pages, Oqtane.Client";
        public const string ErrorModule = "Oqtane.Modules.Admin.Error.{Action}, Oqtane.Client";
        public const string ModuleMessageComponent = "Oqtane.Modules.Controls.ModuleMessage, Oqtane.Client";

        public const string DefaultSiteTemplate = "Oqtane.SiteTemplates.DefaultSiteTemplate, Oqtane.Server";

        public const string ContentUrl = "/api/file/download/";

        public const string HostUser = "host";

        public const string MasterTenant = "Master";
        public const string DefaultSite = "Default Site";

        public const string AllUsersRole = "All Users";
        public const string HostRole = "Host Users";
        public const string AdminRole = "Administrators";
        public const string RegisteredRole = "Registered Users";

        public const string ImageFiles = "jpg,jpeg,jpe,gif,bmp,png";
        public const string UploadableFiles = "jpg,jpeg,jpe,gif,bmp,png,mov,wmv,avi,mp4,mp3,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,nupkg";
        public const string ReservedDevices = "CON,NUL,PRN,COM0,COM1,COM2,COM3,COM4,COM5,COM6,COM7,COM8,COM9,LPT0,LPT1,LPT2,LPT3,LPT4,LPT5,LPT6,LPT7,LPT8,LPT9,CONIN$,CONOUT$";

        public static readonly char[] InvalidFileNameChars =
        {
            '\"', '<', '>', '|', '\0', (Char) 1, (Char) 2, (Char) 3, (Char) 4, (Char) 5, (Char) 6, (Char) 7, (Char) 8,
            (Char) 9, (Char) 10, (Char) 11, (Char) 12, (Char) 13, (Char) 14, (Char) 15, (Char) 16, (Char) 17, (Char) 18,
            (Char) 19, (Char) 20, (Char) 21, (Char) 22, (Char) 23, (Char) 24, (Char) 25, (Char) 26, (Char) 27,
            (Char) 28, (Char) 29, (Char) 30, (Char) 31, ':', '*', '?', '\\', '/'
        };
        public static readonly string[] InvalidFileNameEndingChars = { ".", " " };
    }
}