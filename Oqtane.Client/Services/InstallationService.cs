using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class InstallationService : ServiceBase, IInstallationService
    {
        public InstallationService(HttpClient http) : base(http) {}

        private string ApiUrl => CreateApiUrl("Installation", null); // tenant agnostic as SiteState does not exist

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

        public async Task RestartAsync()
        {
            await PostAsync($"{ApiUrl}/restart");
        }
    }
}
