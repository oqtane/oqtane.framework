using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Metadata;
using Oqtane.Databases;
using Oqtane.Shared;

namespace Oqtane.Database.MySQL
{
    public class MySQLDatabase : DatabaseBase
    {
        private static string _friendlyName => "MySQL";

        private static string _name => "MySQL";

        static MySQLDatabase()
        {
            Initialize(typeof(MySQLDatabase));
        }

        public MySQLDatabase() :base(_name, _friendlyName) { }

        public override string Provider => "MySql.EntityFrameworkCore";

        public override OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return table.Column<int>(name: name, nullable: false).Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        public override string ConcatenateSql(params string[] values)
        {
            var returnValue = "CONCAT(";
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    returnValue += ",";
                }
                returnValue += values[i];
            }

            returnValue += ")";

            return returnValue;
        }

        public override int ExecuteNonQuery(string connectionString, string query)
        {
            var conn = new MySqlConnection(connectionString);
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
            var conn = new MySqlConnection(connectionString);
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }


        public override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseMySQL(connectionString);
        }

        private void PrepareCommand(MySqlConnection conn, MySqlCommand cmd, string query)
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
