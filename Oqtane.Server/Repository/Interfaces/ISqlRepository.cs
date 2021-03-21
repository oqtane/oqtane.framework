using System.Reflection;
using Microsoft.Data.SqlClient;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISqlRepository
    {
        void ExecuteScript(Tenant tenant, string script);
        bool ExecuteScript(Tenant tenant, Assembly assembly, string filename);
        int ExecuteNonQuery(Tenant tenant, string query);
        SqlDataReader ExecuteReader(Tenant tenant, string query);
    }
}
