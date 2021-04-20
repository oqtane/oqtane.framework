using System;
using System.Collections.Generic;
using System.Globalization;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Oqtane.Shared;

namespace Oqtane.Database.PostgreSQL
{
    public class PostgreSQLDatabase : OqtaneDatabaseBase
    {
        private static string _friendlyName => "PostgreSQL";

        private static string _name => "PostgreSQL";

        private readonly INameRewriter _rewriter;

        private static readonly List<ConnectionStringField> _connectionStringFields = new()
        {
            new() {Name = "Server", FriendlyName = "Server", Value = "127.0.0.1", HelpText="Enter the database server"},
            new() {Name = "Port", FriendlyName = "Port", Value = "5432", HelpText="Enter the port used to connect to the server"},
            new() {Name = "Database", FriendlyName = "Database", Value = "Oqtane-{{Date}}", HelpText="Enter the name of the database"},
            new() {Name = "IntegratedSecurity", FriendlyName = "Integrated Security", Value = "true", HelpText="Select if you want integrated security or not"},
            new() {Name = "Uid", FriendlyName = "User Id", Value = "", HelpText="Enter the username to use for the database"},
            new() {Name = "Pwd", FriendlyName = "Password", Value = "", HelpText="Enter the password to use for the database"}
        };

        public PostgreSQLDatabase() : base(_name, _friendlyName, _connectionStringFields)
        {
            _rewriter = new SnakeCaseNameRewriter(CultureInfo.InvariantCulture);
        }

        public override string Provider => "Npgsql.EntityFrameworkCore.PostgreSQL";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
        }

        public override string BuildConnectionString()
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

        public override string ConcatenateSql(params string[] values)
        {
            var returnValue = String.Empty;
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    returnValue += " || ";
                }
                returnValue += values[i];
            }

            return returnValue;
        }

        public override string RewriteName(string name)
        {
            return _rewriter.RewriteName(name);
        }

        public override void UpdateIdentityStoreTableNames(ModelBuilder builder)
        {
            foreach(var entity in builder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (tableName.StartsWith("AspNetUser"))
                {
                    // Replace table names
                    entity.SetTableName(RewriteName(entity.GetTableName()));

                    // Replace column names
                    foreach(var property in entity.GetProperties())
                    {
                        property.SetColumnName(RewriteName(property.GetColumnName()));
                    }

                    foreach(var key in entity.GetKeys())
                    {
                        key.SetName(RewriteName(key.GetName()));
                    }

                    foreach(var index in entity.GetIndexes())
                    {
                        index.SetName(RewriteName(index.GetName()));
                    }
                }
            }
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }
}
