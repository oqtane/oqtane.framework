using System;

namespace Oqtane.Shared
{
    public class Constants
    {
        public static readonly string Version = "3.4.3";
        public const string ReleaseVersions = "1.0.0,1.0.1,1.0.2,1.0.3,1.0.4,2.0.0,2.0.1,2.0.2,2.1.0,2.2.0,2.3.0,2.3.1,3.0.0,3.0.1,3.0.2,3.0.3,3.1.0,3.1.1,3.1.2,3.1.3,3.1.4,3.2.0,3.2.1,3.3.0,3.3.1,3.4.0,3.4.1,3.4.2,3.4.3";
        public const string PackageId = "Oqtane.Framework";
        public const string ClientId = "Oqtane.Client";
        public const string UpdaterPackageId = "Oqtane.Updater";
        public const string PackageRegistryUrl = "https://www.oqtane.net";

        public const string DataDirectory = "DataDirectory";
        public const string DefaultDBType = "Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer";

        public const string PageComponent = "Oqtane.UI.ThemeBuilder, Oqtane.Client";
        public const string ContainerComponent = "Oqtane.UI.ContainerBuilder, Oqtane.Client";

        public const string DefaultTheme = "Oqtane.Themes.OqtaneTheme.Default, Oqtane.Client";
        public const string DefaultContainer = "Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client";
        public const string DefaultAdminContainer = "Oqtane.Themes.AdminContainer, Oqtane.Client";

        public const string ActionToken = "{Action}";
        public const string DefaultAction = "Index";

        public static readonly string[] ReservedRoutes = { "api", "pages", "files" };
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

        public const string FileUrl = "/files/";
        public const string ImageUrl = "/api/file/image/";
        public const int UserFolderCapacity = 20; // megabytes
        public const string PackagesFolder = "Packages";

        public const string DefaultSite = "Default Site";

        public const string ImageFiles = "jpg,jpeg,jpe,gif,bmp,png,ico,webp";
        public const string UploadableFiles = ImageFiles + ",mov,wmv,avi,mp4,mp3,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,nupkg,csv,json,xml,rss,css";
        public const string ReservedDevices = "CON,NUL,PRN,COM0,COM1,COM2,COM3,COM4,COM5,COM6,COM7,COM8,COM9,LPT0,LPT1,LPT2,LPT3,LPT4,LPT5,LPT6,LPT7,LPT8,LPT9,CONIN$,CONOUT$";

        public static readonly char[] InvalidFileNameChars =
        {
            '\"', '<', '>', '|', '\0', (Char) 1, (Char) 2, (Char) 3, (Char) 4, (Char) 5, (Char) 6, (Char) 7, (Char) 8,
            (Char) 9, (Char) 10, (Char) 11, (Char) 12, (Char) 13, (Char) 14, (Char) 15, (Char) 16, (Char) 17, (Char) 18,
            (Char) 19, (Char) 20, (Char) 21, (Char) 22, (Char) 23, (Char) 24, (Char) 25, (Char) 26, (Char) 27,
            (Char) 28, (Char) 29, (Char) 30, (Char) 31, ':', '*', '?', '\\', '/'
        };
        public static readonly string[] InvalidFileNameEndingChars = { ".", " " };

        public static readonly string SatelliteAssemblyExtension = ".resources.dll";

        public static readonly string DefaultCulture = "en";

        public static readonly string AuthenticationScheme = "Identity.Application";
        public static readonly string RequestVerificationToken = "__RequestVerificationToken";
        public static readonly string AntiForgeryTokenHeaderName = "X-XSRF-TOKEN-HEADER";
        public static readonly string AntiForgeryTokenCookieName = "X-XSRF-TOKEN-COOKIE";

        public static readonly string DefaultVisitorFilter = "bot,crawler,slurp,spider,(none),??";

        public static readonly string HttpContextAliasKey = "Alias";
        public static readonly string HttpContextSiteSettingsKey = "SiteSettings";

        public static readonly string MauiUserAgent = "MAUI";
        public static readonly string VisitorCookiePrefix = "APP_VISITOR_";

        // Obsolete constants

        const string RoleObsoleteMessage = "Use the corresponding member from Oqtane.Shared.RoleNames";

        [Obsolete(RoleObsoleteMessage)]
        public const string AllUsersRole = RoleNames.Everyone;
        [Obsolete(RoleObsoleteMessage)]
        public const string HostRole = RoleNames.Host;
        [Obsolete(RoleObsoleteMessage)]
        public const string AdminRole = RoleNames.Admin;
        [Obsolete(RoleObsoleteMessage)]
        public const string RegisteredRole = RoleNames.Registered;

        [Obsolete("DefaultLayout is deprecated")]
        public const string DefaultLayout = "";

        [Obsolete("Use PaneNames.Admin")]
        public const string AdminPane = PaneNames.Admin;

        [Obsolete("Use UserNames.Host instead.")]
        public const string HostUser = UserNames.Host;

        [Obsolete("Use TenantNames.Master instead")]
        public const string MasterTenant = TenantNames.Master;

        // [Obsolete("Use FileUrl instead")]
        public const string ContentUrl = "/api/file/download/";
    }
}
