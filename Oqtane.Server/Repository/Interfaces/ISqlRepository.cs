using System.Data.SqlClient;
using System.Reflection;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISqlRepository
    {
        bool ExecuteEmbeddedScript(Assembly assembly, string script);
        void ExecuteScript(Tenant tenant, string script);
        int ExecuteNonQuery(Tenant tenant, string query);
        SqlDataReader ExecuteReader(Tenant tenant, string query);
    }
}
