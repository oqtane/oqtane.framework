namespace Oqtane.Shared
{
    public static class SettingKeys
    {
        public const string DatabaseSection = "Database";
        public const string DatabaseTypeKey = "DefaultDBType";

        public const string ConnectionStringsSection = "ConnectionStrings";
        public const string ConnectionStringKey = "DefaultConnection";

        public const string InstallationSection = "Installation";
        public const string DefaultAliasKey = "DefaultAlias";
        public const string HostUsernameKey = "HostUsername";
        public const string HostPasswordKey = "HostPassword";
        public const string HostEmailKey = "HostEmail";
        public const string HostNameKey = "HostName";
        public const string SiteTemplateKey = "SiteTemplate";
        public const string DefaultThemeKey = "DefaultTheme";
        public const string DefaultContainerKey = "DefaultContainer";

        public const string AvailableDatabasesSection = "AvailableDatabases";

        public const string TestModeKey = "TestMode"; // optional - used for testing run-time characteristics
    }
}
