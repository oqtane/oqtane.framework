using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class SiteTemplateService : ServiceBase, ISiteTemplateService
    {
        public SiteTemplateService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("SiteTemplate");

        public async Task<List<SiteTemplate>> GetSiteTemplatesAsync()
        {
            List<SiteTemplate> siteTemplates = await GetJsonAsync<List<SiteTemplate>>(Apiurl);
            return siteTemplates.OrderBy(item => item.Name).ToList();
        }
    }
}
