using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Net;

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

        public async Task<List<Package>> GetPackagesAsync(string type)
        {
            return await GetPackagesAsync(type, "");
        }

        public async Task<List<Package>> GetPackagesAsync(string type, string search)
        {
            return await GetJsonAsync<List<Package>>($"{Apiurl}?type={type}&search={WebUtility.UrlEncode(search)}");
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
