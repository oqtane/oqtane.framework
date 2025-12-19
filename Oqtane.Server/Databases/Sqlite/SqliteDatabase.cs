using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases;

namespace Oqtane.Database.Sqlite
{
    public class SqliteDatabase : DatabaseBase
    {
        private static string _friendlyName => "SQLite";

        private static string _name => "Sqlite";

        static SqliteDatabase()
        {
            Initialize(typeof(SqliteDatabase));
        }

        public SqliteDatabase() :base(_name, _friendlyName) { }

        public override string Provider => "Microsoft.EntityFrameworkCore.Sqlite";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("Sqlite:Autoincrement", true);
        }

        public override void DropColumn(MigrationBuilder builder, string name, string table)
        {
            // SQLite supports dropping columns starting with version 3.35.0 but EF Core does not implement it yet
            // note that a column cannot be dropped if it has a UNIQUE constraint, is part of a PRIMARY KEY, is indexed, or is referenced by other parts of the schema
            builder.Sql($"ALTER TABLE {table} DROP COLUMN {name};");
        }

        public override void AlterStringColumn(MigrationBuilder builder, string name, string table, int length, bool nullable, bool unicode, string index)
        {
            // not implemented as SQLite does not support altering columns
            // note that column length does not need to be modified as SQLite uses a TEXT type which utilizes variable length strings
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
            var conn = new SqliteConnection(connectionString);
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
            var conn = new SqliteConnection(connectionString);
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        public override string DelimitName(string name)
        {
            return $"\"{name}\"";
        }

        public override string RewriteValue(object value)
        {
            var type = value.GetType().Name;
            if (type == "DateTime")
            {
                return $"'{value}'";
            }
            if (type == "Boolean")
            {
                return (bool)value ? "1" : "0"; // SQLite uses 1/0 for boolean values
            }
            return value.ToString();
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlite(connectionString)
                .ReplaceService<IHistoryRepository, HistoryRepository>();
        }

        private void PrepareCommand(SqliteConnection conn, SqliteCommand cmd, string query)
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
