using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using MySql.EntityFrameworkCore.Metadata;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Database.MySQL
{
    public class MySQLDatabase : OqtaneDatabaseBase
    {
        private static string _friendlyName => "MySQL";

        private static string _name => "MySQL";

        private static readonly List<ConnectionStringField> _connectionStringFields = new()
        {
            new() {Name = "Server", FriendlyName = "Server", Value = "127.0.0.1", HelpText="Enter the database server"},
            new() {Name = "Port", FriendlyName = "Port", Value = "3306", HelpText="Enter the port used to connect to the server"},
            new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}", HelpText="Enter the name of the database"},
            new() {Name = "Uid", FriendlyName = "User Id", Value = "", HelpText="Enter the username to use for the database"},
            new() {Name = "Pwd", FriendlyName = "Password", Value = "", HelpText="Enter the password to use for the database"}
        };

        public MySQLDatabase() :base(_name, _friendlyName, _connectionStringFields) { }

        public override string Provider => "MySql.EntityFrameworkCore";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        public override string BuildConnectionString()
        {
            var connectionString = String.Empty;

            var server = ConnectionStringFields[0].Value;
            var port = ConnectionStringFields[1].Value;
            var database = ConnectionStringFields[2].Value;
            var userId = ConnectionStringFields[3].Value;
            var password = ConnectionStringFields[4].Value;

            if (!String.IsNullOrEmpty(server) && !String.IsNullOrEmpty(database) && !String.IsNullOrEmpty(userId) && !String.IsNullOrEmpty(password))
            {
                connectionString = $"Server={server};Database={database};Uid={userId};Pwd={password};";
            }

            if (!String.IsNullOrEmpty(port))
            {
                connectionString += $"Port={port};";
            }
            return connectionString;
        }

        public override string ConcatenateSql(params string[] values)
        {
            var returnValue = "CONCAT(";
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    returnValue += ",";
                }
                returnValue += values[i];
            }

            returnValue += ")";

            return returnValue;
        }


        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseMySQL(connectionString);
        }
    }
}
