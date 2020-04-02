namespace Oqtane.Shared
{
    public class Constants
    {
        public const string PackageId = "Oqtane.Framework";
        public const string Version = "0.0.9";

        public const string PageComponent = "Oqtane.UI.ThemeBuilder, Oqtane.Client";
        public const string ContainerComponent = "Oqtane.UI.ContainerBuilder, Oqtane.Client";

        public const string DefaultTheme = "Oqtane.Themes.BlazorTheme.Default, Oqtane.Client";
        public const string DefaultLayout = "";
        public const string DefaultContainer = "Oqtane.Themes.BlazorTheme.Container, Oqtane.Client";
        public const string DefaultAdminContainer = "Oqtane.Themes.AdminContainer, Oqtane.Client";

        public const string ActionToken = "{Action}";
        public const string DefaultAction = "Index";
        public const string AdminPane = "Admin";

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

        public const string AllUsersRole = "All Users";
        public const string HostRole = "Host Users";
        public const string AdminRole = "Administrators";
        public const string RegisteredRole = "Registered Users";

        public const string ImageFiles = "jpg,jpeg,jpe,gif,bmp,png";
        public const string UploadableFiles = "jpg,jpeg,jpe,gif,bmp,png,mov,wmv,avi,mp4,mp3,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,nupkg";
    }
}
