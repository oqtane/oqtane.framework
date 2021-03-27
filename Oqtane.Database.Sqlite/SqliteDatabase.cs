using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Models;

namespace Oqtane.Repository.Databases
{
    public class SqliteDatabase : IOqtaneDatabase
    {
        public SqliteDatabase()
        {
            ConnectionStringFields = new List<ConnectionStringField>()
            {
                new() {Name = "Server", FriendlyName = "File Name", Value = "Oqtane-{{Date}}.db"}
            };
        }

        public string FriendlyName => Name;

        public string Name => "Sqlite";

        public string Provider => "Microsoft.EntityFrameworkCore.Sqlite";

        public List<ConnectionStringField> ConnectionStringFields { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Sqlite:Autoincrement", true);
        }

        public string BuildConnectionString()
        {
            var connectionstring = String.Empty;

            var server = ConnectionStringFields[0].Value;

            if (!String.IsNullOrEmpty(server))
            {
                connectionstring = $"Data Source={server};";
            }

            return connectionstring;
        }

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlite(connectionString);
        }
    }
}
