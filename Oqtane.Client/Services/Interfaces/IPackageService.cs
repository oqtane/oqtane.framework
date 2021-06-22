using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetPackagesAsync(string type);
        Task<List<Package>> GetPackagesAsync(string type, string search);
        Task DownloadPackageAsync(string packageId, string version, string folder);
        Task InstallPackagesAsync();
    }
}
