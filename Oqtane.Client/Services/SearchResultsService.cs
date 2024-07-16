using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SearchResultsService : ServiceBase, ISearchResultsService, IClientService
    {
        public SearchResultsService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("SearchResults");

        public async Task<SearchResults> GetSearchResultsAsync(SearchQuery searchQuery)
        {
            return await PostJsonAsync<SearchQuery, SearchResults>(ApiUrl, searchQuery);
        }
    }
}
