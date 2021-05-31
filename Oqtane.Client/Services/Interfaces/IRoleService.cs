using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Role"/>s on a <see cref="Site"/>
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Get all <see cref="Role"/>s of this <see cref="Site"/>.
        ///
        /// Will exclude global roles which are for all sites. To get those as well, use the overload <see cref="GetRolesAsync(int, bool)"/>
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<List<Role>> GetRolesAsync(int siteId);

        /// <summary>
        /// Get roles of the <see cref="Site"/> and optionally include global Roles.
        /// </summary>
        /// <param name="siteId">ID-reference to a <see cref="Site"/></param>
        /// <param name="includeGlobalRoles">True if it should also include global roles. False will return the same data as just calling <see cref="GetRolesAsync(int)"/></param>
        /// <returns></returns>
        Task<List<Role>> GetRolesAsync(int siteId, bool includeGlobalRoles);

        /// <summary>
        /// Get one specific <see cref="Role"/>
        /// </summary>
        /// <param name="roleId">ID-reference of a <see cref="Role"/></param>
        /// <returns></returns>
        Task<Role> GetRoleAsync(int roleId);

        /// <summary>
        /// Add / save a new <see cref="Role"/> to the database.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<Role> AddRoleAsync(Role role);

        /// <summary>
        /// Update a <see cref="Role"/> in the database.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<Role> UpdateRoleAsync(Role role);

        /// <summary>
        /// Delete / mark-as-deleted a <see cref="Role"/> in the database.
        /// </summary>
        /// <param name="roleId">ID-reference of a <see cref="Role"/></param>
        /// <returns></returns>
        Task DeleteRoleAsync(int roleId);
    }
}
