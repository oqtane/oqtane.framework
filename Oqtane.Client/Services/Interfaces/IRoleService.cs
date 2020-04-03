using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
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
