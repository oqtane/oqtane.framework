using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository.Databases
{
    public class SqlServerDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "SQL Server";

        private static string _name => "SqlServer";

        public SqlServerDatabase() :base(_name, _friendlyName) { }
    }
}
