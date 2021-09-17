using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    ///  Service to execute a <see cref="SqlQuery"/> against the backend database
    /// </summary>
    public interface ISqlService
    {
        /// <summary>
        /// Executes a sql query and returns its result
        /// </summary>
        /// <param name="sqlquery"></param>
        /// <returns></returns>
        Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery);
    }
}
