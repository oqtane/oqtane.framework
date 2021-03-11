using Oqtane.Models;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public interface IInstallationService
    {
        Task<Installation> IsInstalled();
        Task<Installation> Install(InstallConfig config);
        Task<Installation> Upgrade();
        Task RestartAsync();
    }
}
