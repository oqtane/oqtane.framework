using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetRolesAsync();

        Task<List<Role>> GetRolesAsync(int SiteId);

        Task<Role> GetRoleAsync(int RoleId);

        Task<Role> AddRoleAsync(Role Role);

        Task<Role> UpdateRoleAsync(Role Role);

        Task DeleteRoleAsync(int RoleId);
    }
}
