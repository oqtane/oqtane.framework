using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Oqtane.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class CacheController : Controller
    {
        private readonly ICacheService _cacheService;

        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        // DELETE api/<controller>/outputCache/evictByTag/{tag}
        [HttpDelete("outputCache/evictByTag/{tag}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task EvictOutputCacheByTag(string tag, CancellationToken cancellationToken = default)
        {
            await _cacheService.EvictOutputCacheByTag(tag, cancellationToken);
        }
    }
}
