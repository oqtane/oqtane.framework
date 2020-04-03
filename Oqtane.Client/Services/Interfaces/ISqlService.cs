using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface ISqlService
    {
        Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery);
    }
}
