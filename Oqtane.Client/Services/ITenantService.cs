using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ITenantService
    {
        Task<Tenant> GetTenantAsync();
    }
}
