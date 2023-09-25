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
        public PackageService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Package");

        public async Task<List<Package>> GetPackagesAsync(string type)
        {
            return await GetPackagesAsync(type, "", "", "");
        }

        public async Task<List<Package>> GetPackagesAsync(string type, string search, string price, string package)
        {
            return await GetPackagesAsync(type, search, price, package, "");
        }

        public async Task<List<Package>> GetPackagesAsync(string type, string search, string price, string package, string sort)
        {
            return await GetJsonAsync<List<Package>>($"{Apiurl}?type={type}&search={WebUtility.UrlEncode(search)}&price={price}&package={package}&sort={sort}");
        }

        public async Task<List<Package>> GetPackageUpdatesAsync(string type)
        {
            return await GetJsonAsync<List<Package>>($"{Apiurl}/updates/?type={type}");
        }

        public async Task<Package> GetPackageAsync(string packageId, string version, bool download)
        {
            return await PostJsonAsync<Package>($"{Apiurl}?packageid={packageId}&version={version}&download={download}&install=false", null);
        }

        public async Task DownloadPackageAsync(string packageId, string version)
        {
            await PostAsync($"{Apiurl}?packageid={packageId}&version={version}&download=true&install=true");
        }

        public async Task InstallPackagesAsync()
        {
            await GetJsonAsync<List<string>>($"{Apiurl}/install");
        }
    }
}
