using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class TenantController : Controller
    {
        private readonly ITenantRepository Tenants;

        public TenantController(ITenantRepository Tenants)
        {
            this.Tenants = Tenants;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Tenant> Get()
        {
            return Tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
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
            }
            return Tenant;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            Tenants.DeleteTenant(id);
        }
    }
}
