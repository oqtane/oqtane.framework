using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using System.Net;
using Oqtane.Services;
using System.Threading.Tasks;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SiteController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ILogManager _logger;

        public SiteController(ISiteService siteService, ILogManager logger)
        {
            _siteService = siteService;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<IEnumerable<Site>> Get()
        {
            return await _siteService.GetSitesAsync();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<Site> Get(int id)
        {
            return await _siteService.GetSiteAsync(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<Site> Post([FromBody] Site site)
        {
            if (ModelState.IsValid)
            {
                site = await _siteService.AddSiteAsync(site);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Post Attempt {Site}", site);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                site = null;
            }
            return site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<Site> Put(int id, [FromBody] Site site)
        {
            if (ModelState.IsValid)
            {
                site = await _siteService.UpdateSiteAsync(site);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Put Attempt {Site}", site);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                site = null;
            }
            return site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public async Task Delete(int id)
        {
            await _siteService.DeleteSiteAsync(id);
        }

        // GET api/<controller>/modules/5/6
        [HttpGet("modules/{siteId}/{pageId}")]
        public async Task<IEnumerable<Module>> GetModules(int siteId, int pageId)
        {
            return await _siteService.GetModulesAsync(siteId, pageId);
        }
    }
}
