using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Models;

namespace Oqtane.Interfaces
{
    public interface IOqtaneDatabase
    {
        public string FriendlyName { get; }

        public string Name { get; }

        public string Provider { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name);

        public string ConcatenateSql(params string[] values);

        public string RewriteName(string name);

        public void UpdateIdentityStoreTableNames(ModelBuilder builder);

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}
