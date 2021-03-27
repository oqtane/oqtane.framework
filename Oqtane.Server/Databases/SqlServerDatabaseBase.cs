using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Models;

namespace Oqtane.Repository.Databases
{
    public abstract class SqlServerDatabaseBase : IOqtaneDatabase
    {
        protected SqlServerDatabaseBase(string name, string friendlyName, List<ConnectionStringField> connectionStringFields)
        {
            Name = name;
            FriendlyName = friendlyName;
            ConnectionStringFields = connectionStringFields;
        }

        public  string FriendlyName { get; }

        public string Name { get; }

        public string Provider => "Microsoft.EntityFrameworkCore.SqlServer";

        public List<ConnectionStringField> ConnectionStringFields { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("SqlServer:Identity", "1, 1");
        }

        public abstract string BuildConnectionString();

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
