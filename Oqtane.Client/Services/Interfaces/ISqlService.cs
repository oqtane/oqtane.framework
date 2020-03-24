using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISqlService
    {
        Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery);
    }
}
