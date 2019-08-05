using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class TenantController : Controller
    {
        private readonly ITenantRepository tenants;

        public TenantController(ITenantRepository Tenants)
        {
            tenants = Tenants;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Tenant> Get()
        {
            return tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Tenant Get(int id)
        {
            return tenants.GetTenant(id);
        }

        // POST api/<controller>
        [HttpPost]
        public Tenant Post([FromBody] Tenant Tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant = tenants.AddTenant(Tenant);
            }
            return Tenant;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public Tenant Put(int id, [FromBody] Tenant Tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant = tenants.UpdateTenant(Tenant);
            }
            return Tenant;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            tenants.DeleteTenant(id);
        }
    }
}
