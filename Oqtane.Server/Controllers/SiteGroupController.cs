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

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<SiteGroup> Get(string siteid)
        {
            if (User.IsInRole(RoleNames.Host) || (int.TryParse(siteid, out int SiteId) && SiteId == _alias.SiteId))
            {
                var siteGroups = _siteGroupRepository.GetSiteGroups();
                if (!User.IsInRole(RoleNames.Host))
                {
                    siteGroups = siteGroups.Where(item => item.PrimarySiteId == _alias.SiteId);
                }
                return siteGroups.ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroup Get(int id)
        {
            var group = _siteGroupRepository.GetSiteGroup(id);
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
        public SiteGroup Post([FromBody] SiteGroup siteGroup)
        {
            if (ModelState.IsValid)
            {
                siteGroup = _siteGroupRepository.AddSiteGroup(siteGroup);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroup, siteGroup.SiteGroupId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Site Group Added {Group}", siteGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Post Attempt {Group}", siteGroup);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroup = null;
            }
            return siteGroup;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public SiteGroup Put(int id, [FromBody] SiteGroup siteGroup)
        {
            if (ModelState.IsValid && siteGroup.SiteGroupId == id)
            {
                if (!User.IsInRole(RoleNames.Host) && siteGroup.Synchronize)
                {
                    // admins can only update the synchronize field
                    siteGroup = _siteGroupRepository.GetSiteGroup(siteGroup.SiteGroupId, false);
                    siteGroup.Synchronize = true;
                }
                siteGroup = _siteGroupRepository.UpdateSiteGroup(siteGroup);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroup, siteGroup.SiteGroupId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Site Group Updated {Group}", siteGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Put Attempt {Group}", siteGroup);
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
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Site Group Deleted {siteGroupId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Delete Attempt {siteGroupId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
