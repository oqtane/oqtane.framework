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

        public MySQLDatabase() :base(_name, _friendlyName) { }

        public override string Provider => "MySql.EntityFrameworkCore";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
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
