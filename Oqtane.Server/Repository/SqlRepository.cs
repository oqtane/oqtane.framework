using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Oqtane.Models;
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable InvertIf
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Repository
{
    public class SqlRepository : ISqlRepository
    {

        public void ExecuteScript(Tenant tenant, string script)
        {
            // execute script in current tenant
            foreach (var query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
            {
                ExecuteNonQuery(tenant, query);
            }
        }

        public bool ExecuteScript(string connectionString, Assembly assembly, string fileName)
        {
            var success = true;
            var script = GetScriptFromAssembly(assembly, fileName);

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    foreach (var query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
                    {
                        ExecuteNonQuery(connectionString, query);
                    }
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public bool ExecuteScript(Tenant tenant, Assembly assembly, string fileName)
        {
            var success = true;
            var script = GetScriptFromAssembly(assembly, fileName);

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    ExecuteScript(tenant, script);
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public int ExecuteNonQuery(Tenant tenant, string query)
        {
            return ExecuteNonQuery(tenant.DBConnectionString, query);
        }

        public SqlDataReader ExecuteReader(Tenant tenant, string query)
        {
            SqlConnection conn = new SqlConnection(FormatConnectionString(tenant.DBConnectionString));
            SqlCommand cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        private int ExecuteNonQuery(string connectionString, string query)
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

        private string GetScriptFromAssembly(Assembly assembly, string fileName)
        {
            // script must be included as an Embedded Resource within an assembly
            var script = "";

            if (assembly != null)
            {
                var name = assembly.GetManifestResourceNames().FirstOrDefault(item => item.EndsWith("." + fileName));
                if (name != null)
                {
                    var resourceStream = assembly.GetManifestResourceStream(name);
                    if (resourceStream != null)
                    {
                        using (var reader = new StreamReader(resourceStream))
                        {
                            script = reader.ReadToEnd();
                        }
                    }
                }
            }

            return script;
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
