using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Modules.SearchResults.Services
{
    [PrivateApi("Mark SearchResults classes as private, since it's not very useful in the public docs")]
    public class SearchResultsService : ServiceBase, ISearchResultsService, IClientService
    {        
        public SearchResultsService(HttpClient http, SiteState siteState) : base(http, siteState) {}

        private string ApiUrl => CreateApiUrl("SearchResults");

        public async Task<Models.SearchResults> SearchAsync(int moduleId, SearchQuery searchQuery)
        {
            return await PostJsonAsync<SearchQuery, Models.SearchResults>(CreateAuthorizationPolicyUrl(ApiUrl, EntityNames.Module, moduleId), searchQuery);
        }
    }
}
