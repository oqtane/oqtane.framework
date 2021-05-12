using System;
using Oqtane.Interfaces;

namespace Oqtane.Shared
{
    public class InstallConfig
    {
        private IOqtaneDatabase _database;

        public string ConnectionString { get; set; }
        public string DatabaseType { get; set; }

        public IOqtaneDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    var type = Type.GetType(DatabaseType);
                    _database = Activator.CreateInstance(type) as IOqtaneDatabase;
                }

                return _database;
            }
        }

        public string Aliases { get; set; }
        public string TenantName { get; set; }
        public bool IsNewTenant { get; set; }
        public string SiteName { get; set; }
        public string HostPassword { get; set; }
        public string HostEmail { get; set; }
        public string HostName { get; set; }
        public string SiteTemplate { get; set; }
        public string DefaultTheme { get; set; }
        public string DefaultContainer { get; set; }
        public string DefaultAdminContainer { get; set; }
    }
}
