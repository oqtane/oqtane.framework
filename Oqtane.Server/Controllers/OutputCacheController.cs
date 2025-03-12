using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Oqtane.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class OutputCacheController : Controller
    {
        private readonly IOutputCacheService _cacheService;

        public OutputCacheController(IOutputCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        // DELETE api/<controller>/{tag}
        [HttpDelete("{tag}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task EvictByTag(string tag)
        {
            await _cacheService.EvictByTag(tag);
        }
    }
}
