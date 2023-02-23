using System.Data;
using System.Reflection;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISqlRepository
    {
        void ExecuteScript(Tenant tenant, string script);

        bool ExecuteScript(string connectionString, string databaseType, Assembly assembly, string filename);

        bool ExecuteScript(Tenant tenant, Assembly assembly, string filename);

        int ExecuteNonQuery(Tenant tenant, string query);

        int ExecuteNonQuery(string connectionString, string databaseType, string query);

        IDataReader ExecuteReader(Tenant tenant, string query);

        IDataReader ExecuteReader(string DBType, string DBConnectionString, string query);

        string GetScriptFromAssembly(Assembly assembly, string fileName);
    }
}
