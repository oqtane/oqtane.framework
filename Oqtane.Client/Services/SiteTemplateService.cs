using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteTemplateService : ServiceBase, ISiteTemplateService
    {
        public SiteTemplateService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteTemplate");

        public async Task<List<SiteTemplate>> GetSiteTemplatesAsync()
        {
            List<SiteTemplate> siteTemplates = await GetJsonAsync<List<SiteTemplate>>(Apiurl);
            return siteTemplates.OrderBy(item => item.Name).ToList();
        }
    }
}
