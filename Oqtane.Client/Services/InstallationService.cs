using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class InstallationService : ServiceBase, IInstallationService
    {
        public InstallationService(HttpClient http):base(http) { }

        private string ApiUrl => CreateApiUrl("Installation");

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
