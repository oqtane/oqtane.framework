using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using MySql.EntityFrameworkCore.Metadata;
using Oqtane.Interfaces;
using Oqtane.Models;

namespace Oqtane.Database.MySQL
{
    public class MySQLDatabase : IOqtaneDatabase
    {
        public MySQLDatabase()
        {
            ConnectionStringFields = new List<ConnectionStringField>()
            {
                new() {Name = "Server", FriendlyName = "Server", Value = "127.0.0.1"},
                new() {Name = "Port", FriendlyName = "Port", Value = "3306"},
                new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}"},
                new() {Name = "Uid", FriendlyName = "User Id", Value = ""},
                new() {Name = "Pwd", FriendlyName = "Password", Value = ""}
            };
        }

        public string FriendlyName => Name;

        public string Name => "MySQL";

        public string Provider => "MySql.EntityFrameworkCore";

        public List<ConnectionStringField> ConnectionStringFields { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        public string BuildConnectionString()
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

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseMySQL(connectionString);
        }
    }
}
