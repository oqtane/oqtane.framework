using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Enums;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class TenantController : Controller
    {
        private readonly ITenantRepository _tenants;
        private readonly ILogManager _logger;
        private readonly IStringLocalizer _localizer;

        public TenantController(ITenantRepository tenants, ILogManager logger, IStringLocalizer<TenantController> localizer)
        {
            _tenants = tenants;
            _logger = logger;
            _localizer = localizer;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<Tenant> Get()
        {
            return _tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Tenant Get(int id)
        {
            return _tenants.GetTenant(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public Tenant Post([FromBody] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant = _tenants.AddTenant(tenant);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, _localizer["Tenant Added {TenantId}"], tenant.TenantId);
            }
            return tenant;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Tenant Put(int id, [FromBody] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant = _tenants.UpdateTenant(tenant);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, _localizer["Tenant Updated {TenantId}"], tenant.TenantId);
            }
            return tenant;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            _tenants.DeleteTenant(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Tenant Deleted {TenantId}"], id);
        }
    }
}
