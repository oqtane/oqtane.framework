using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetPackagesAsync(string Tag);
        Task DownloadPackageAsync(string PackageId, string Version, string Folder);
    }
}
