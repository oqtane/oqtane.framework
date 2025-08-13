using Oqtane.Models;
using Oqtane.Shared;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;

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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SqlService : ServiceBase, ISqlService
    {
        public SqlService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Sql");

        public async Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery)
        {
            return await PostJsonAsync<SqlQuery>(Apiurl, sqlquery);
        }
    }
}
