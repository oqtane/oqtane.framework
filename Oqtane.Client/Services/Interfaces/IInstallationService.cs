using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services.Interfaces
{
    public interface IInstallationService
    {
        Task<Installation> IsInstalled();
        Task<Installation> Install(InstallConfig config);
        Task<Installation> Upgrade();
    }
}
