using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IUserRoleService
    {
        Task<List<UserRole>> GetUserRolesAsync();
        Task<List<UserRole>> GetUserRolesAsync(int SiteId);
        Task<UserRole> GetUserRoleAsync(int UserRoleId);
        Task<UserRole> AddUserRoleAsync(UserRole UserRole);
        Task<UserRole> UpdateUserRoleAsync(UserRole UserRole);
        Task DeleteUserRoleAsync(int UserRoleId);
    }
}
