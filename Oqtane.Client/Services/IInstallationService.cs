using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IInstallationService
    {
        Task<GenericResponse> IsInstalled();
        Task<GenericResponse> Install(string connectionstring);
    }
}
