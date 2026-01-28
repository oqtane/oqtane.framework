using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class SiteGroupController : Controller
    {
        private readonly ISiteGroupRepository _siteGroupRepository;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SiteGroupController(ISiteGroupRepository siteGroupRepository, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _siteGroupRepository = siteGroupRepository;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&groupid=y
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<SiteGroup> Get(string siteid, string groupid)
        {
            if (int.TryParse(siteid, out int SiteId) && int.TryParse(groupid, out int SiteGroupDefinitionId))
            {
                return _siteGroupRepository.GetSiteGroups(SiteId, SiteGroupDefinitionId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Get Attempt for SiteId {SiteId} And SiteGroupDefinitionId {SiteGroupDefinitionId}", siteid, groupid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroup Get(int id)
        {
            var siteGroup = _siteGroupRepository.GetSiteGroup(id);
            if (siteGroup != null)
            {
                return siteGroup;
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
        public SiteGroup Post([FromBody] SiteGroup siteGroup)
        {
            if (ModelState.IsValid)
            {
                siteGroup = _siteGroupRepository.AddSiteGroup(siteGroup);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroup, siteGroup.SiteGroupDefinitionId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Site Group Added {SiteGroup}", siteGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Post Attempt {SiteGroup}", siteGroup);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroup = null;
            }
            return siteGroup;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroup Put(int id, [FromBody] SiteGroup siteGroup)
        {
            if (ModelState.IsValid && siteGroup.SiteGroupDefinitionId == id && _siteGroupRepository.GetSiteGroup(siteGroup.SiteGroupDefinitionId, false) != null)
            {
                siteGroup = _siteGroupRepository.UpdateSiteGroup(siteGroup);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroup, siteGroup.SiteGroupDefinitionId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Site Group Updated {SiteGroup}", siteGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Put Attempt {SiteGroup}", siteGroup);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroup = null;
            }
            return siteGroup;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            var siteGroup = _siteGroupRepository.GetSiteGroup(id);
            if (siteGroup != null)
            {
                _siteGroupRepository.DeleteSiteGroup(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroup, siteGroup.SiteGroupId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Site Group Deleted {SiteGroupId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Delete Attempt {SiteGroupId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
