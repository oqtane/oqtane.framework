using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class TenantController : Controller
    {
        private readonly ITenantRepository Tenants;
        private readonly ILogManager logger;

        public TenantController(ITenantRepository Tenants, ILogManager logger)
        {
            this.Tenants = Tenants;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
        public IEnumerable<Tenant> Get()
        {
            return Tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Get(int id)
        {
            return Tenants.GetTenant(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Post([FromBody] Tenant Tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant = Tenants.AddTenant(Tenant);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Tenant Added {TenantId}", Tenant.TenantId);
            }
            return Tenant;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Tenant Put(int id, [FromBody] Tenant Tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant = Tenants.UpdateTenant(Tenant);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Tenant Updated {TenantId}", Tenant.TenantId);
            }
            return Tenant;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            Tenants.DeleteTenant(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Tenant Deleted {TenantId}", id);
        }
    }
}
