using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases;
using Oqtane.Shared;

namespace Oqtane.Database.SqlServer
{
    public class SqlServerDatabase : DatabaseBase
    {
        private static string _friendlyName => "SQL Server";

        private static string _name => "SqlServer";

        static SqlServerDatabase()
        {
            Initialize(typeof(SqlServerDatabase));
        }

        public SqlServerDatabase() : base(_name, _friendlyName)
        {
        }

        public override string Provider => "Microsoft.EntityFrameworkCore.SqlServer";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("SqlServer:Identity", "1, 1");
        }

        public override void AlterStringColumn(MigrationBuilder builder, string name, string table, int length, bool nullable, bool unicode, string index)
        {
            var elements = index.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length != 0)
            {
                builder.DropIndex(elements[0], table);
            }
            builder.AlterColumn<string>(name, table, maxLength: length, nullable: nullable, unicode: unicode);
            if (elements.Length != 0)
            {
                var columns = elements[1].Split(',');
                builder.CreateIndex(elements[0], table, columns, null, bool.Parse(elements[2]), null);
            }
        }

        public override int ExecuteNonQuery(string connectionString, string query)
        {
            var conn = new SqlConnection(FormatConnectionString(connectionString));
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
            var conn = new SqlConnection(FormatConnectionString(connectionString));
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlServer(connectionString)
                .ReplaceService<IHistoryRepository, OqtaneHistoryRepository>();
        }

        private string FormatConnectionString(string connectionString)
        {
            return connectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }

        private void PrepareCommand(SqlConnection conn, SqlCommand cmd, string query)
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
