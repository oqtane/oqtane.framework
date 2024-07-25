using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SearchResultsController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SearchResultsController(ISearchService searchService, ILogManager logger, ITenantManager tenantManager) 
        {
            _searchService = searchService;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        [HttpPost]
        public async Task<SearchResults> Post([FromBody] SearchQuery searchQuery)
        {
            if (ModelState.IsValid && searchQuery.SiteId == _alias.SiteId)
            {
                return await _searchService.GetSearchResultsAsync(searchQuery);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Search Results Post Attempt {SearchQuery}", searchQuery);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }
    }
}
