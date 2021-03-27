using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Oqtane.Database.PostgreSQL
{
    public class PostgreSQLDatabase : IOqtaneDatabase
    {

        public PostgreSQLDatabase()
        {
            ConnectionStringFields = new List<ConnectionStringField>()
            {
                new() {Name = "Server", FriendlyName = "Server", Value = "127.0.0.1"},
                new() {Name = "Port", FriendlyName = "Port", Value = "5432"},
                new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}"},
                new() {Name = "IntegratedSecurity", FriendlyName = "Integrated Security", Value = "true"},
                new() {Name = "Uid", FriendlyName = "User Id", Value = ""},
                new() {Name = "Pwd", FriendlyName = "Password", Value = ""}
            };
        }

        public string FriendlyName => Name;

        public string Name => "PostgreSQL";

        public string Provider => "Npgsql.EntityFrameworkCore.PostgreSQL";

        public List<ConnectionStringField> ConnectionStringFields { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
        }

        public string BuildConnectionString()
        {
            var connectionString = String.Empty;

            var server = ConnectionStringFields[0].Value;
            var port = ConnectionStringFields[1].Value;
            var database = ConnectionStringFields[2].Value;
            var integratedSecurity = Boolean.Parse(ConnectionStringFields[3].Value);
            var userId = ConnectionStringFields[4].Value;
            var password = ConnectionStringFields[5].Value;

            if (!String.IsNullOrEmpty(server)  && !String.IsNullOrEmpty(database) && !String.IsNullOrEmpty(port))
            {
                connectionString = $"Server={server};Port={port};Database={database};";
            }

            if (integratedSecurity)
            {
                connectionString += "Integrated Security=true;";
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

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }
}
