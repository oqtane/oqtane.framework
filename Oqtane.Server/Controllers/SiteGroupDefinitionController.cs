using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

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

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<SiteGroupDefinition> Get(string siteid)
        {
            if (User.IsInRole(RoleNames.Host) || (int.TryParse(siteid, out int SiteId) && SiteId == _alias.SiteId))
            {
                var siteGroupDefinitions = _siteGroupDefinitionRepository.GetSiteGroupDefinitions();
                if (!User.IsInRole(RoleNames.Host))
                {
                    siteGroupDefinitions = siteGroupDefinitions.Where(item => item.PrimarySiteId == _alias.SiteId);
                }
                return siteGroupDefinitions.ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Definition Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
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
        [Authorize(Roles = RoleNames.Admin)]
        public SiteGroupDefinition Put(int id, [FromBody] SiteGroupDefinition siteGroupDefinition)
        {
            if (ModelState.IsValid && siteGroupDefinition.SiteGroupDefinitionId == id)
            {
                if (!User.IsInRole(RoleNames.Host) && siteGroupDefinition.Synchronize)
                {
                    // admins can only update the synchronize field
                    siteGroupDefinition = _siteGroupDefinitionRepository.GetSiteGroupDefinition(siteGroupDefinition.SiteGroupDefinitionId, false);
                    siteGroupDefinition.Synchronize = true;
                }
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
