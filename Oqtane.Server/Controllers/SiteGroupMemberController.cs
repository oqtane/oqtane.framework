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
    public class SiteGroupMemberController : Controller
    {
        private readonly ISiteGroupMemberRepository _siteGroupMemberRepository;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SiteGroupMemberController(ISiteGroupMemberRepository siteGroupMemberRepository, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _siteGroupMemberRepository = siteGroupMemberRepository;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&groupid=y
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<SiteGroupMember> Get(string siteid, string groupid)
        {
            if (int.TryParse(siteid, out int SiteId) && int.TryParse(groupid, out int SiteGroupId))
            {
                return _siteGroupMemberRepository.GetSiteGroupMembers(SiteId, SiteGroupId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Member Get Attempt for SiteId {SiteId} And SiteGroupId {SiteGroupId}", siteid, groupid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroupMember Get(int id)
        {
            var siteGroupMember = _siteGroupMemberRepository.GetSiteGroupMember(id);
            if (siteGroupMember != null)
            {
                return siteGroupMember;
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
        public SiteGroupMember Post([FromBody] SiteGroupMember siteGroupMember)
        {
            if (ModelState.IsValid)
            {
                siteGroupMember = _siteGroupMemberRepository.AddSiteGroupMember(siteGroupMember);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupMember, siteGroupMember.SiteGroupId, SyncEventActions.Create);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, siteGroupMember.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Site Group Member Added {SiteGroupMember}", siteGroupMember);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Member Post Attempt {SiteGroupMember}", siteGroupMember);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroupMember = null;
            }
            return siteGroupMember;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public SiteGroupMember Put(int id, [FromBody] SiteGroupMember siteGroupMember)
        {
            if (ModelState.IsValid && siteGroupMember.SiteGroupId == id && _siteGroupMemberRepository.GetSiteGroupMember(siteGroupMember.SiteGroupId, false) != null)
            {
                siteGroupMember = _siteGroupMemberRepository.UpdateSiteGroupMember(siteGroupMember);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupMember, siteGroupMember.SiteGroupId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, siteGroupMember.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Site Group Member Updated {SiteGroupMember}", siteGroupMember);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Member Put Attempt {SiteGroupMember}", siteGroupMember);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                siteGroupMember = null;
            }
            return siteGroupMember;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            var siteGroupMember = _siteGroupMemberRepository.GetSiteGroupMember(id);
            if (siteGroupMember != null)
            {
                _siteGroupMemberRepository.DeleteSiteGroupMember(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.SiteGroupMember, siteGroupMember.SiteGroupMemberId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, siteGroupMember.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Site Group Member Deleted {SiteGroupMemberId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Site Group Member Delete Attempt {SiteGroupMemberId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
