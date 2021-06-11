using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Oqtane.Databases.Interfaces
{
    public interface IDatabase
    {
        public string AssemblyName { get; }

        public string FriendlyName { get; }

        public string Name { get; }

        public string Provider { get; }

        public string TypeName { get; }

        public OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name);

        public string ConcatenateSql(params string[] values);

        public int ExecuteNonQuery(string connectionString, string query);

        public IDataReader ExecuteReader(string connectionString, string query);

        public string RewriteName(string name);

        public void UpdateIdentityStoreTableNames(ModelBuilder builder);

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}
