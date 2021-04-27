using System;
using System.Collections.Generic;
using System.Globalization;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Oqtane.Shared;

namespace Oqtane.Database.PostgreSQL
{
    public class PostgreSQLDatabase : OqtaneDatabaseBase
    {
        private static string _friendlyName => "PostgreSQL";

        private static string _name => "PostgreSQL";

        private readonly INameRewriter _rewriter;

        public PostgreSQLDatabase() : base(_name, _friendlyName)
        {
            _rewriter = new SnakeCaseNameRewriter(CultureInfo.InvariantCulture);
        }

        public override string Provider => "Npgsql.EntityFrameworkCore.PostgreSQL";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
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

        public override string RewriteName(string name)
        {
            return _rewriter.RewriteName(name);
        }

        public override void UpdateIdentityStoreTableNames(ModelBuilder builder)
        {
            foreach(var entity in builder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (tableName.StartsWith("AspNetUser"))
                {
                    // Replace table names
                    entity.SetTableName(RewriteName(entity.GetTableName()));

                    // Replace column names
                    foreach(var property in entity.GetProperties())
                    {
                        property.SetColumnName(RewriteName(property.GetColumnName()));
                    }

                    foreach(var key in entity.GetKeys())
                    {
                        key.SetName(RewriteName(key.GetName()));
                    }

                    foreach(var index in entity.GetIndexes())
                    {
                        index.SetName(RewriteName(index.GetName()));
                    }
                }
            }
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }
}
