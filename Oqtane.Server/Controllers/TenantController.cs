using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class TenantController : Controller
    {
        private readonly ITenantRepository _tenants;

        public TenantController(ITenantRepository tenants)
        {
            _tenants = tenants;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<Tenant> Get()
        {
            return _tenants.GetTenants();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public Tenant Get(int id)
        {
            return _tenants.GetTenant(id);
        }
    }
}
