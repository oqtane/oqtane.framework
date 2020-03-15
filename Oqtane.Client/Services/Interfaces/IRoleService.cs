using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetRolesAsync(int siteId);

        Task<Role> GetRoleAsync(int roleId);

        Task<Role> AddRoleAsync(Role role);

        Task<Role> UpdateRoleAsync(Role role);

        Task DeleteRoleAsync(int roleId);
    }
}
