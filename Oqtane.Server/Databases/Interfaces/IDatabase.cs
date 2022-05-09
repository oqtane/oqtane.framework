using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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

        public string RewriteValue(string value, string type);

        public void UpdateIdentityStoreTableNames(ModelBuilder builder);

        public void DropColumn(MigrationBuilder builder, string name, string table);

        public void AlterStringColumn(MigrationBuilder builder, string name, string table, int length, bool nullable, bool unicode, string index);

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}
