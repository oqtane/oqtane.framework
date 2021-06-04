using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Databases
{
    public abstract class DatabaseBase : IDatabase
    {
        private static string _assemblyName;

        private static string _typeName;

        protected DatabaseBase(string name, string friendlyName)
        {
            Name = name;
            FriendlyName = friendlyName;
        }

        protected static void Initialize(Type type)
        {
            var typeQualifiedName = type.AssemblyQualifiedName;
            var assembly = type.Assembly;
            var assemblyName = assembly.FullName;

            _typeName = typeQualifiedName.Substring(0, typeQualifiedName.IndexOf(", Version"));
            _assemblyName = assemblyName.Substring(0, assemblyName.IndexOf(", Version"));
        }

        public string AssemblyName => _assemblyName;

        public string FriendlyName { get; }

        public string Name { get; }

        public abstract string Provider { get; }

        public string TypeName => _typeName;

        public abstract OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name);

        public virtual string ConcatenateSql(params string[] values)
        {
            var returnValue = String.Empty;
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    returnValue += " + ";
                }
                returnValue += values[i];
            }

            return returnValue;
        }

        public abstract int ExecuteNonQuery(string connectionString, string query);

        public abstract IDataReader ExecuteReader(string connectionString, string query);

        public virtual string RewriteName(string name)
        {
            return name;
        }

        public virtual void UpdateIdentityStoreTableNames(ModelBuilder builder)
        {

        }

        public abstract DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}
