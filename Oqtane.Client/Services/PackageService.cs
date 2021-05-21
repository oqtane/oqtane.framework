using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class PackageService : ServiceBase, IPackageService
    {
        private readonly SiteState _siteState;

        public PackageService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }
        private string Apiurl => CreateApiUrl("Package", _siteState.Alias);

        public async Task<List<Package>> GetPackagesAsync(string tag)
        {
            List<Package> packages = await GetJsonAsync<List<Package>>($"{Apiurl}?tag={tag}");
            return packages.OrderByDescending(item => item.Downloads).ToList();
        }

        public async Task DownloadPackageAsync(string packageId, string version, string folder)
        {
            await PostAsync($"{Apiurl}?packageid={packageId}&version={version}&folder={folder}");
        }

        public async Task InstallPackagesAsync()
        {
            await GetJsonAsync<List<string>>($"{Apiurl}/install");
        }
    }
}
