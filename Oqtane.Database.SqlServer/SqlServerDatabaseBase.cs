using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository.Databases
{
    public abstract class SqlServerDatabaseBase : OqtaneDatabaseBase
    {
        protected SqlServerDatabaseBase(string name, string friendlyName) : base(name, friendlyName)
        {
        }

        public override string Provider => "Microsoft.EntityFrameworkCore.SqlServer";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("SqlServer:Identity", "1, 1");
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
