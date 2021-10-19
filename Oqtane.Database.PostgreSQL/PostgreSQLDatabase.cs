using System;
using System.Data;
using System.Globalization;
using EFCore.NamingConventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Oqtane.Databases;
using Oqtane.Shared;

namespace Oqtane.Database.PostgreSQL
{
    public class PostgreSQLDatabase : DatabaseBase
    {
        private static string _friendlyName => "PostgreSQL";

        private static string _name => "PostgreSQL";

        private readonly INameRewriter _rewriter;

        static PostgreSQLDatabase()
        {
            Initialize(typeof(PostgreSQLDatabase));
        }

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

        public override int ExecuteNonQuery(string connectionString, string query)
        {
            var conn = new NpgsqlConnection(connectionString);
            var cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(conn, cmd, query);
                var val = -1;
                try
                {
                    val = cmd.ExecuteNonQuery();
                }
                catch
                {
                    // an error occurred executing the query
                }
                return val;
            }

        }

        public override IDataReader ExecuteReader(string connectionString, string query)
        {
            var conn = new NpgsqlConnection(connectionString);
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        public override string RewriteName(string name)
        {
            return _rewriter.RewriteName(name);
        }

        public override string RewriteValue(string value, string type)
        {
            if (type == "bool")
            {
                value = (value == "1") ? "true" : "false";
            }
            return value;
        }

        public override void UpdateIdentityStoreTableNames(ModelBuilder builder)
        {
            foreach(var entity in builder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (tableName.StartsWith("AspNetUser"))
                {
                    // replace table name
                    entity.SetTableName(RewriteName(entity.GetTableName()));

                    // replace column names
                    foreach(var property in entity.GetProperties())
                    {
                        property.SetColumnName(RewriteName(property.Name));
                    }

                    // replace key names
                    foreach(var key in entity.GetKeys())
                    {
                        key.SetName(RewriteName(key.GetName()));
                    }

                    // replace foreign key names
                    foreach (var key in entity.GetForeignKeys())
                    {
                        key.PrincipalKey.SetName(RewriteName(key.PrincipalKey.GetName()));
                    }

                    // replace index names
                    foreach (var index in entity.GetIndexes())
                    {
                        index.SetDatabaseName(RewriteName(index.GetDatabaseName()));
                    }
                }
            }
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .ReplaceService<IHistoryRepository, OqtaneHistoryRepository>();
        }

        private void PrepareCommand(NpgsqlConnection conn, NpgsqlCommand cmd, string query)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
        }
    }
}
