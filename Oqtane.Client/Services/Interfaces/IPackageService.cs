using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IPackageService
    {
        Task<List<Package>> GetPackagesAsync(string tag);
        Task DownloadPackageAsync(string packageId, string version, string folder);
    }
}
