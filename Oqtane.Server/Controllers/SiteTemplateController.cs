using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SiteTemplateController : Controller
    {
        private readonly ISiteTemplateRepository _siteTemplates;

        public SiteTemplateController(ISiteTemplateRepository siteTemplates)
        {
            _siteTemplates = siteTemplates;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<SiteTemplate> Get()
        {
            return _siteTemplates.GetSiteTemplates();
        }
    }
}
