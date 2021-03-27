using System;
using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Repository.Databases;

namespace Oqtane.Databases
{
    public class LocalDbDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "Local Database";
        private static string _name => "LocalDB";

        private static readonly List<ConnectionStringField> _connectionStringFields = new()
        {
            new() {Name = "Server", FriendlyName = "Server", Value = "(LocalDb)\\MSSQLLocalDB"},
            new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}"}
        };

        public LocalDbDatabase() :base(_name, _friendlyName, _connectionStringFields) { }

        public override string BuildConnectionString()
        {
            var connectionString = String.Empty;

            var server = ConnectionStringFields[0].Value;
            var database = ConnectionStringFields[1].Value;

            if (!String.IsNullOrEmpty(server)  && !String.IsNullOrEmpty(database))
            {
                connectionString = $"Data Source={server};AttachDbFilename=|DataDirectory|\\{database}.mdf;Initial Catalog={database};Integrated Security=SSPI;";
            }

            return connectionString;
        }
    }
}
