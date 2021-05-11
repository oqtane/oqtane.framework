namespace Oqtane.Database.SqlServer
{
    public class SqlServerDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "SQL Server";

        private static string _name => "SqlServer";

        public SqlServerDatabase() :base(_name, _friendlyName) { }
    }
}
