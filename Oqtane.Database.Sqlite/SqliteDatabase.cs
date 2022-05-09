using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases;
using Oqtane.Shared;

namespace Oqtane.Database.Sqlite
{
    public class SqliteDatabase : DatabaseBase
    {
        private static string _friendlyName => "Sqlite";

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
            // not implemented as SQLite does not support dropping columns
        }

        public override void AlterStringColumn(MigrationBuilder builder, string name, string table, int length, bool nullable, bool unicode, string index)
        {
            // not implemented as SQLite does not support altering columns
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

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlite(connectionString)
                .ReplaceService<IHistoryRepository, OqtaneHistoryRepository>();
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
