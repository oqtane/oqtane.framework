using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class SiteTemplateController : Controller
    {
        private readonly ISiteTemplateRepository _siteTemplates;

        public SiteTemplateController(ISiteTemplateRepository siteTemplates)
        {
            _siteTemplates = siteTemplates;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<SiteTemplate> Get()
        {
            return _siteTemplates.GetSiteTemplates();
        }
    }
}
