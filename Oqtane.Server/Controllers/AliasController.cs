using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class AliasController : Controller
    {
        private readonly IAliasRepository _aliases;
        private readonly ITenantRepository _tenants;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public AliasController(IAliasRepository aliases, ITenantRepository tenants, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _aliases = aliases;
            _tenants = tenants;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<Alias> Get()
        {
            var aliases = _aliases.GetAliases();
            if (!User.IsInRole(RoleNames.Host))
            {
                aliases = aliases.Where(item => item.SiteId == _alias.SiteId && item.TenantId == _alias.TenantId);
            }
            return aliases;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Alias Get(int id)
        {
            return _aliases.GetAlias(id);
        }
        
        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public Alias Post([FromBody] Alias alias)
        {
            if (ModelState.IsValid)
            {
                alias = _aliases.AddAlias(alias);
                _syncManager.AddSyncEvent(alias.TenantId, EntityNames.Alias, alias.AliasId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Alias Added {Alias}", alias);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Alias Post Attempt {Alias}", alias);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                alias = null;
            }
            return alias;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Alias Put(int id, [FromBody] Alias alias)
        {
            if (ModelState.IsValid && _aliases.GetAlias(alias.AliasId, false) != null)
            {
                alias = _aliases.UpdateAlias(alias);
                _syncManager.AddSyncEvent(alias.TenantId, EntityNames.Alias, alias.AliasId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Alias Updated {Alias}", alias);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Alias Put Attempt {Alias}", alias);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                alias = null;
            }
            return alias;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            var alias = _aliases.GetAlias(id);
            if (alias != null)
            {
                _aliases.DeleteAlias(id);
                _syncManager.AddSyncEvent(alias.TenantId, EntityNames.Alias, alias.AliasId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Alias Deleted {AliasId}", id);

                var aliases = _aliases.GetAliases();
                if (!aliases.Any(item => item.TenantId == alias.TenantId))
                {
                    _tenants.DeleteTenant(alias.TenantId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Tenant Deleted {TenantId}", alias.TenantId);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Alias Delete Attempt {AliasId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
