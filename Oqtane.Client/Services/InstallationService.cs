using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public InstallationService(HttpClient http, SiteState siteState, NavigationManager navigationManager):base(http)
        {
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string ApiUrl => CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Installation");

        public async Task<Installation> IsInstalled()
        {
            return await GetJsonAsync<Installation>($"{ApiUrl}/installed");
        }

        public async Task<Installation> Install(InstallConfig config)
        {
            return await PostJsonAsync<InstallConfig,Installation>(ApiUrl, config);
        }

        public async Task<Installation> Upgrade()
        {
            return await GetJsonAsync<Installation>($"{ApiUrl}/upgrade");
        }
    }
}
