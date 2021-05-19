namespace Oqtane.Database.SqlServer
{
    public class LocalDbDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "Local Database";
        private static string _name => "LocalDB";

        private readonly static string _typeName;

        static LocalDbDatabase()
        {
            var typeQualifiedName = typeof(LocalDbDatabase).AssemblyQualifiedName;

            _typeName = typeQualifiedName.Substring(0, typeQualifiedName.IndexOf(", Version"));
        }

        public LocalDbDatabase() :base(_name, _friendlyName) { }

        public override string TypeName => _typeName;
    }
}
