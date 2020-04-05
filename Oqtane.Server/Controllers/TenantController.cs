using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Enums;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class TenantController : Controller
    {
        private readonly ITenantRepository _tenants;
        private readonly ILogManager _logger;

        public TenantController(ITenantRepository tenants, ILogManager logger)
        {
            _tenants = tenants;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public IEnumerable<Tenant> Get()
        {
            return _tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Get(int id)
        {
            return _tenants.GetTenant(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Post([FromBody] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant = _tenants.AddTenant(tenant);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Tenant Added {TenantId}", tenant.TenantId);
            }
            return tenant;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Put(int id, [FromBody] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant = _tenants.UpdateTenant(tenant);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Tenant Updated {TenantId}", tenant.TenantId);
            }
            return tenant;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            _tenants.DeleteTenant(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Tenant Deleted {TenantId}", id);
        }
    }
}
