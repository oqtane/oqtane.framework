using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Manage <see cref="Role"/>s assigned to a specific <see cref="User"/>
    /// </summary>
    public interface IUserRoleService
    {
        /// <summary>
        /// Get all <see cref="UserRole"/>s on a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId">ID-reference to a <see cref="Site"/></param>
        /// <returns></returns>
        Task<List<UserRole>> GetUserRolesAsync(int siteId);

        /// <summary>
        /// Get all <see cref="UserRole"/>s on a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId">ID-reference to a <see cref="Site"/></param>
        /// <param name="userId">ID-reference to a <see cref="User"/></param>
        /// <returns></returns>
        Task<List<UserRole>> GetUserRolesAsync(int siteId, int userId);

        /// <summary>
        /// Get all <see cref="UserRole"/>s on a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId">ID-reference to a <see cref="Site"/></param>
        /// <param name="roleName">Name reference a <see cref="Role"/></param>
        /// <returns></returns>
        Task<List<UserRole>> GetUserRolesAsync(int siteId, string roleName);

        /// <summary>
        /// Get all <see cref="UserRole"/>s on a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId">ID-reference to a <see cref="Site"/></param>
        /// <param name="userId">ID-reference to a <see cref="User"/></param>
        /// <param name="roleName">Name reference a <see cref="Role"/></param>
        /// <returns></returns>
        Task<List<UserRole>> GetUserRolesAsync(int siteId, int userId, string roleName);

        /// <summary>
        /// Get one specific <see cref="UserRole"/>
        /// </summary>
        /// <param name="userRoleId">ID-reference to a <see cref="UserRole"/></param>
        /// <returns></returns>
        Task<UserRole> GetUserRoleAsync(int userRoleId);

        /// <summary>
        /// Save a new <see cref="UserRole"/>
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns></returns>
        Task<UserRole> AddUserRoleAsync(UserRole userRole);

        /// <summary>
        /// Update a <see cref="UserRole"/> in the database
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns></returns>
        Task<UserRole> UpdateUserRoleAsync(UserRole userRole);

        /// <summary>
        /// Delete a <see cref="UserRole"/> in the database
        /// </summary>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        Task DeleteUserRoleAsync(int userRoleId);
    }
}
