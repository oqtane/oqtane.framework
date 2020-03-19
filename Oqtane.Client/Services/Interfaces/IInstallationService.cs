using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IInstallationService
    {
        Task<Installation> IsInstalled();
        Task<Installation> Install(string connectionstring);
        Task<Installation> Upgrade();
    }
}
