namespace Oqtane.Models
{
    /// <summary>
    /// Information about a Database used in the current Oqtane installation
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Friendly name for the Admin to identify the DB
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Name of the Database
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Namespace & name of the UI control to configure this database, like `Oqtane.Installer.Controls.SqlServerConfig, Oqtane.Client`
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// Type of DB using the full namespace, like `Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer`
        /// </summary>
        public string DBType { get; set; }

        /// <summary>
        /// Software package responsible for using this DB - like `Oqtane.Database.MySQL`
        /// </summary>
        public string Package { get; set; }
    }
}
