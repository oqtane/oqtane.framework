using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Services
{
    public class SqlService : ServiceBase, ISqlService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SqlService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Sql"); }
        }

        public async Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery)
        {
            return await PostJsonAsync<SqlQuery>(Apiurl, sqlquery);
        }
    }
}
