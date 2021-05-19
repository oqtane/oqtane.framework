namespace Oqtane.Database.SqlServer
{
    public class SqlServerDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "SQL Server";

        private static string _name => "SqlServer";

        private readonly static string _typeName;

        static SqlServerDatabase()
        {
            var typeQualifiedName = typeof(SqlServerDatabase).AssemblyQualifiedName;

            _typeName = typeQualifiedName.Substring(0, typeQualifiedName.IndexOf(", Version"));
        }

        public SqlServerDatabase() : base(_name, _friendlyName) { }

        public override string TypeName => _typeName;
    }
}
