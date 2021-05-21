using Oqtane.Models;
using Oqtane.Shared;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SqlService : ServiceBase, ISqlService
    {
        private readonly SiteState _siteState;

        public SqlService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("Sql", _siteState.Alias);

        public async Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery)
        {
            return await PostJsonAsync<SqlQuery>(Apiurl, sqlquery);
        }
    }
}
