using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetPackagesAsync(string tag);
        Task DownloadPackageAsync(string packageId, string version, string folder);
    }
}
