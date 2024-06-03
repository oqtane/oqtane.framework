using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Documentation;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Services;

namespace Oqtane.Modules.SearchResults.Services
{
    [PrivateApi("Mark SearchResults classes as private, since it's not very useful in the public docs")]
    public class ServerSearchResultsService : ISearchResultsService, ITransientService
    {
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;
        private readonly ISearchService _searchService;

        public ServerSearchResultsService(
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor,
            ISearchService searchService)
        {
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _searchService = searchService;
        }

        public async Task<Models.SearchResults> SearchAsync(int moduleId, SearchQuery searchQuery)
        {
            var results = await _searchService.SearchAsync(searchQuery);
            return results;
        }
    }
}
