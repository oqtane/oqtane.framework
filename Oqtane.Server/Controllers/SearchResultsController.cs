using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SearchResultsController : ModuleControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchResultsController(ISearchService searchService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _searchService = searchService;
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.SearchResults> Post([FromBody] Models.SearchQuery searchQuery)
        {
            try
            {
                return await _searchService.GetSearchResultsAsync(searchQuery);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex, "Fetch search results failed.", searchQuery);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }
    }
}
