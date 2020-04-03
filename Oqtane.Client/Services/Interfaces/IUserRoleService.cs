using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<List<UserRole>> GetUserRolesAsync(int siteId);
        Task<UserRole> GetUserRoleAsync(int userRoleId);
        Task<UserRole> AddUserRoleAsync(UserRole userRole);
        Task<UserRole> UpdateUserRoleAsync(UserRole userRole);
        Task DeleteUserRoleAsync(int userRoleId);
    }
}
