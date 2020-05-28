namespace Oqtane.Shared
{
    public class InstallConfig
    {
        public string ConnectionString { get; set; }
        public string Aliases { get; set; }
        public string TenantName { get; set; }
        public bool IsNewTenant { get; set; }
        public string SiteName { get; set; }
        public string HostPassword { get; set; }
        public string HostEmail { get; set; }
        public string HostName { get; set; }
        public string SiteTemplate { get; set; }
        public string DefaultTheme { get; set; }
        public string DefaultLayout { get; set; }
        public string DefaultContainer { get; set; }
    }
}
