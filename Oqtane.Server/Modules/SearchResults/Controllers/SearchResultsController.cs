using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Modules.SearchResults.Services;
using Oqtane.Shared;

namespace Oqtane.Modules.SearchResults.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    [PrivateApi("Mark SearchResults classes as private, since it's not very useful in the public docs")]
    public class SearchResultsController : ModuleControllerBase
    {
        private readonly ISearchResultsService _searchResultsService;

        public SearchResultsController(ISearchResultsService searchResultsService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _searchResultsService = searchResultsService;
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.SearchResults> Post([FromBody] Models.SearchQuery searchQuery)
        {
            try
            {
                return await _searchResultsService.SearchAsync(AuthEntityId(EntityNames.Module), searchQuery);
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex, "Fetch search results failed.", searchQuery);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }
    }
}
