using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SiteTemplateService : ServiceBase, ISiteTemplateService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SiteTemplateService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "SiteTemplate"); }
        }

        public async Task<List<SiteTemplate>> GetSiteTemplatesAsync()
        {
            List<SiteTemplate> siteTemplates = await GetJsonAsync<List<SiteTemplate>>(Apiurl);
            return siteTemplates.OrderBy(item => item.Name).ToList();
        }
    }
}
