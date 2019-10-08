using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IInstallationService
    {
        Task<GenericResponse> IsInstalled();
        Task<GenericResponse> Install(string connectionstring);
        Task<GenericResponse> Upgrade();
    }
}
