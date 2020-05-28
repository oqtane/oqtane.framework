using Oqtane.Models;
using Oqtane.Shared;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class SqlService : ServiceBase, ISqlService
    {
        private readonly SiteState _siteState;

        public SqlService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Sql");

        public async Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery)
        {
            return await PostJsonAsync<SqlQuery>(Apiurl, sqlquery);
        }
    }
}
