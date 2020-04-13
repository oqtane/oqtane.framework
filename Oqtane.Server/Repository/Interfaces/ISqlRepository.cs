using System.Data.SqlClient;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISqlRepository
    {
        void ExecuteScript(Tenant tenant, string script);
        int ExecuteNonQuery(Tenant tenant, string query);
        SqlDataReader ExecuteReader(Tenant tenant, string query);
    }
}
