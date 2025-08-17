using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Net;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage packages (<see cref="Package"/>)
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        /// Returns a list of packages matching the given parameters
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<List<Package>> GetPackagesAsync(string type);

        /// <summary>
        /// Returns a list of packages matching the given parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="search"></param>
        /// <param name="price"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        Task<List<Package>> GetPackagesAsync(string type, string search, string price, string package);

        /// <summary>
        /// Returns a list of packages matching the given parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="search"></param>
        /// <param name="price"></param>
        /// <param name="package"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        Task<List<Package>> GetPackagesAsync(string type, string search, string price, string package, string sort);

        /// <summary>
        /// Returns a list of packages based on installationid
        /// </summary>
        /// <returns></returns>
        Task<List<Package>> GetPackageUpdatesAsync(string type);

        /// <summary>
        /// Returns a specific package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<Package> GetPackageAsync(string packageId, string version, bool download);

        /// <summary>
        /// Downloads a specific package as .nupkg file
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task DownloadPackageAsync(string packageId, string version);

        /// <summary>
        /// Installs all packages located in //TODO: 2dm where?
        /// </summary>
        /// <returns></returns>
        Task InstallPackagesAsync();
    }

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
