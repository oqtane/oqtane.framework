using System.Data.SqlClient;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISqlRepository
    {
        int ExecuteNonQuery(Tenant tenant, string query);
        SqlDataReader ExecuteReader(Tenant tenant, string query);
    }
}
