using System;
using System.Data;
using System.Data.SqlClient;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SqlRepository : ISqlRepository
    {

        public void ExecuteScript(Tenant tenant, string script)
        {
            // execute script in curent tenant
            foreach (string query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
            {
                ExecuteNonQuery(tenant, query);
            }
        }

        public int ExecuteNonQuery(Tenant tenant, string query)
        {
            SqlConnection conn = new SqlConnection(FormatConnectionString(tenant.DBConnectionString));
            SqlCommand cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(conn, cmd, query);
                int val = -1;
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

        public SqlDataReader ExecuteReader(Tenant tenant, string query)
        {
            SqlConnection conn = new SqlConnection(FormatConnectionString(tenant.DBConnectionString));
            SqlCommand cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
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

        private string FormatConnectionString(string connectionString)
        {
            return connectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }
    }
}
