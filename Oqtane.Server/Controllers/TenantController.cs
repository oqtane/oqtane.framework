using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

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
        public Tenant Get()
        {
            return tenants.GetTenant();
        }
    }
}
