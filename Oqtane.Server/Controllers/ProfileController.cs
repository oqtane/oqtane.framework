using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profiles;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ProfileController(IProfileRepository profiles, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _profiles = profiles;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Profile> Get(string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                return _profiles.GetProfiles(SiteId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Profile Get(int id)
        {
            var profile = _profiles.GetProfile(id);
            if (profile != null && profile.SiteId == _alias.SiteId)
            {
                return profile;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Get Attempt {ProfileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = $"{EntityNames.Profile}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public Profile Post([FromBody] Profile profile)
        {
            if (ModelState.IsValid && profile.SiteId == _alias.SiteId)
            {
                profile = _profiles.AddProfile(profile);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Profile, profile.ProfileId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Profile Added {Profile}", profile);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Post Attempt {Profile}", profile);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                profile = null;
            }
            return profile;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = $"{EntityNames.Profile}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public Profile Put(int id, [FromBody] Profile profile)
        {
            if (ModelState.IsValid && profile.SiteId == _alias.SiteId && _profiles.GetProfile(profile.ProfileId, false) != null)
            {
                profile = _profiles.UpdateProfile(profile);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Profile, profile.ProfileId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Profile Updated {Profile}", profile);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Put Attempt {Profile}", profile);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                profile = null;
            }
            return profile;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = $"{EntityNames.Profile}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public void Delete(int id)
        {
            var profile = _profiles.GetProfile(id);
            if (profile != null && profile.SiteId == _alias.SiteId)
            {
                _profiles.DeleteProfile(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Profile, profile.ProfileId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Profile Deleted {ProfileId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Delete Attempt {ProfileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
