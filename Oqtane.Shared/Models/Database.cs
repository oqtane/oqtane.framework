namespace Oqtane.Models
{
    /// <summary>
    /// Information about a Database used in the current Oqtane installation
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Name of the Database
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Namespace &amp; name of the UI control to configure this database, like `Oqtane.Installer.Controls.SqlServerConfig, Oqtane.Client`
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// Type of DB using the full namespace, like `Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer`
        /// </summary>
        public string DBType { get; set; }

        /// <summary>
        /// whether this item is the default database provider ( ie. specified by DefaultDBType in appsettings.json )
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
