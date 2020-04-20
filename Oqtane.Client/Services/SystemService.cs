using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class SystemService : ServiceBase, ISystemService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public SystemService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "System"); }
        }

        public async Task<Dictionary<string, string>> GetSystemInfoAsync()
        {
            return await GetJsonAsync<Dictionary<string, string>>(Apiurl);
        }
    }
}
