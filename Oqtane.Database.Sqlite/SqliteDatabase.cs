using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository.Databases
{
    public class SqliteDatabase : OqtaneDatabaseBase
    {
        private static string _friendlyName => "Sqlite";

        private static string _name => "Sqlite";

        private static readonly List<ConnectionStringField> _connectionStringFields = new()
        {
            new() {Name = "Server", FriendlyName = "File Name", Value = "Oqtane-{{Date}}.db", HelpText="Enter the file name to use for the database"}
        };

        public SqliteDatabase() :base(_name, _friendlyName, _connectionStringFields) { }

        public override string Provider => "Microsoft.EntityFrameworkCore.Sqlite";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Sqlite:Autoincrement", true);
        }

        public override string BuildConnectionString()
        {
            var connectionstring = String.Empty;

            var server = ConnectionStringFields[0].Value;

            if (!String.IsNullOrEmpty(server))
            {
                connectionstring = $"Data Source={server};";
            }

            return connectionstring;
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

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlite(connectionString);
        }
    }
}
