using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Returns a specific package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<Package> GetPackageAsync(string packageId, string version);

        /// <summary>
        /// Downloads a specific package as .nupkg file
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task DownloadPackageAsync(string packageId, string version, string folder);

        /// <summary>
        /// Installs all packages located in //TODO: 2dm where?
        /// </summary>
        /// <returns></returns>
        Task InstallPackagesAsync();
    }
}
