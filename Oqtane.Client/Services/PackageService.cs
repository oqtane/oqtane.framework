using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace Oqtane.Services
{
    public class PackageService : ServiceBase, IPackageService
    {        
        public PackageService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("Package");

        public async Task<List<Package>> GetPackagesAsync(string tag)
        {
            List<Package> packages = await GetJsonAsync<List<Package>>($"{Apiurl}?tag={tag}");
            return packages.OrderByDescending(item => item.Downloads).ToList();
        }

        public async Task DownloadPackageAsync(string packageId, string version, string folder)
        {
            await PostAsync($"{Apiurl}?packageid={packageId}&version={version}&folder={folder}");
        }
    }
}
