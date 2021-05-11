using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Shared;

namespace Oqtane.Database.SqlServer
{
    public abstract class SqlServerDatabaseBase : OqtaneDatabaseBase
    {
        protected SqlServerDatabaseBase(string name, string friendlyName) : base(name, friendlyName)
        {
        }

        public override string Provider => "Microsoft.EntityFrameworkCore.SqlServer";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("SqlServer:Identity", "1, 1");
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
            return optionsBuilder.UseSqlServer(connectionString);
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
