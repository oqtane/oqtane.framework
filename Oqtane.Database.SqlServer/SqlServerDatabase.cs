using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository.Databases
{
    public class SqlServerDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "SQL Server";

        private static string _name => "SqlServer";

        private static readonly List<ConnectionStringField> _connectionStringFields = new()
        {
            new() {Name = "Server", FriendlyName = "Server", Value = ".", HelpText="Enter the database server"},
            new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}", HelpText="Enter the name of the database"},
            new() {Name = "IntegratedSecurity", FriendlyName = "Integrated Security", Value = "true", HelpText="Select if you want integrated security or not"},
            new() {Name = "Uid", FriendlyName = "User Id", Value = "", HelpText="Enter the username to use for the database"},
            new() {Name = "Pwd", FriendlyName = "Password", Value = "", HelpText="Enter the password to use for the database"}
        };

        public SqlServerDatabase() :base(_name, _friendlyName, _connectionStringFields) { }

        public override string BuildConnectionString()
        {
            var connectionString = String.Empty;

            var server = ConnectionStringFields[0].Value;
            var database = ConnectionStringFields[1].Value;
            var integratedSecurity = Boolean.Parse(ConnectionStringFields[2].Value);
            var userId = ConnectionStringFields[3].Value;
            var password = ConnectionStringFields[4].Value;

            if (!String.IsNullOrEmpty(server)  && !String.IsNullOrEmpty(database))
            {
                connectionString = $"Data Source={server};Initial Catalog={database};";
            }

            if (integratedSecurity)
            {
                connectionString += "Integrated Security=SSPI;";
            }
            else
            {
                if (!String.IsNullOrEmpty(userId) && !String.IsNullOrEmpty(password))
                {
                    connectionString += $"User ID={userId};Password={password};";
                }
                else
                {
                    connectionString = String.Empty;
                }
            }

            return connectionString;

        }
    }
}
