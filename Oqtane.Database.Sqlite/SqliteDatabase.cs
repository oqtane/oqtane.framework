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

        public SqliteDatabase() :base(_name, _friendlyName) { }

        public override string Provider => "Microsoft.EntityFrameworkCore.Sqlite";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Sqlite:Autoincrement", true);
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
