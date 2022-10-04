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
    public class UrlMappingController : Controller
    {
        private readonly IUrlMappingRepository _urlMappings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public UrlMappingController(IUrlMappingRepository urlMappings, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _urlMappings = urlMappings;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&ismapped=y
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<UrlMapping> Get(string siteid, string ismapped)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                return _urlMappings.GetUrlMappings(SiteId, bool.Parse(ismapped));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public UrlMapping Get(int id)
        {
            var urlMapping = _urlMappings.GetUrlMapping(id);
            if (urlMapping != null && urlMapping.SiteId == _alias.SiteId)
            {
                return urlMapping;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Get Attempt {UrlMappingId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/url/x?url=y
        [HttpGet("url/{siteid}")]
        public UrlMapping Get(int siteid, string url)
        {            
            var urlMapping = _urlMappings.GetUrlMapping(siteid, WebUtility.UrlDecode(url));
            if (urlMapping != null && urlMapping.SiteId == _alias.SiteId)
            {
                return urlMapping;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Get Attempt {SiteId} {Url}", siteid, url);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public UrlMapping Post([FromBody] UrlMapping urlMapping)
        {
            if (ModelState.IsValid && urlMapping.SiteId == _alias.SiteId)
            {
                urlMapping = _urlMappings.AddUrlMapping(urlMapping);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UrlMapping, urlMapping.UrlMappingId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "UrlMapping Added {UrlMapping}", urlMapping);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Post Attempt {Role}", urlMapping);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                urlMapping = null;
            }
            return urlMapping;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public UrlMapping Put(int id, [FromBody] UrlMapping urlMapping)
        {
            if (ModelState.IsValid && urlMapping.SiteId == _alias.SiteId && _urlMappings.GetUrlMapping(urlMapping.UrlMappingId, false) != null)
            {
                urlMapping = _urlMappings.UpdateUrlMapping(urlMapping);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UrlMapping, urlMapping.UrlMappingId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "UrlMapping Updated {UrlMapping}", urlMapping);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Put Attempt {UrlMapping}", urlMapping);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                urlMapping = null;
            }
            return urlMapping;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            var urlMapping = _urlMappings.GetUrlMapping(id);
            if (urlMapping != null && urlMapping.SiteId == _alias.SiteId)
            {
                _urlMappings.DeleteUrlMapping(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.UrlMapping, urlMapping.UrlMappingId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "UrlMapping Deleted {UrlMappingId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized UrlMapping Delete Attempt {UrlMappingId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
