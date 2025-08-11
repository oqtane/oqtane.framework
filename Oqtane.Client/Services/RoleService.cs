using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class RoleService : ServiceBase, IRoleService
    {
        public RoleService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Role");

        public async Task<List<Role>> GetRolesAsync(int siteId)
        {
            return await GetRolesAsync(siteId, false);
        }

        public async Task<List<Role>> GetRolesAsync(int siteId, bool includeGlobalRoles)
        {
            List<Role> roles = await GetJsonAsync<List<Role>>($"{Apiurl}?siteid={siteId}&global={includeGlobalRoles}");
            return roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<Role> GetRoleAsync(int roleId)
        {
            return await GetJsonAsync<Role>($"{Apiurl}/{roleId}");
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            return await PostJsonAsync<Role>(Apiurl, role);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            return await PutJsonAsync<Role>($"{Apiurl}/{role.RoleId}", role);
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            await DeleteAsync($"{Apiurl}/{roleId}");
        }
    }
}
