using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SiteGroupDefinitionController : Controller
    {
        private readonly ISiteGroupDefinitionRepository _siteGroupDefinitionRepository;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SiteGroupDefinitionController(ISiteGroupDefinitionRepository siteGroupDefinitionRepository, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _siteGroupDefinitionRepository = siteGroupDefinitionRepository;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<SiteGroupDefinition> Get()
        {
            return _siteGroupDefinitionRepository.GetSiteGroupDefinitions().ToList();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroupDefinition Get(int id)
        {
            var group = _siteGroupDefinitionRepository.GetSiteGroupDefinition(id);
            if (group != null)
            {
                return group;
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroupDefinition Post([FromBody] SiteGroupDefinition siteGroupDefinition)
        {
            if (ModelState.IsValid)
            {
                siteGroupDefinition = _siteGroupDefinitionRepository.AddSiteGroupDefinition(siteGroupDefinition);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupDefinition, siteGroupDefinition.SiteGroupDefinitionId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Site Group Definition Added {Group}", siteGroupDefinition);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Definition Post Attempt {Group}", siteGroupDefinition);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroupDefinition = null;
            }
            return siteGroupDefinition;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroupDefinition Put(int id, [FromBody] SiteGroupDefinition siteGroupDefinition)
        {
            if (ModelState.IsValid && siteGroupDefinition.SiteGroupDefinitionId == id && _siteGroupDefinitionRepository.GetSiteGroupDefinition(siteGroupDefinition.SiteGroupDefinitionId, false) != null)
            {
                siteGroupDefinition = _siteGroupDefinitionRepository.UpdateSiteGroupDefinition(siteGroupDefinition);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupDefinition, siteGroupDefinition.SiteGroupDefinitionId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Site Group Definition Updated {Group}", siteGroupDefinition);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Definition Put Attempt {Group}", siteGroupDefinition);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroupDefinition = null;
            }
            return siteGroupDefinition;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            var siteGroupDefinition = _siteGroupDefinitionRepository.GetSiteGroupDefinition(id);
            if (siteGroupDefinition != null)
            {
                _siteGroupDefinitionRepository.DeleteSiteGroupDefinition(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupDefinition, siteGroupDefinition.SiteGroupDefinitionId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Site Group Definition Deleted {siteGroupDefinitionId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Definition Delete Attempt {siteGroupDefinitionId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
