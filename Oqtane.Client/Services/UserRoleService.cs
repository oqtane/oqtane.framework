using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

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

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UserRoleService : ServiceBase, IUserRoleService
    {
        public UserRoleService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("UserRole");

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId)
        {
            return await GetUserRolesAsync(siteId, -1, "");
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId, int userId)
        {
            return await GetUserRolesAsync(siteId, userId, "");
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId, string roleName)
        {
            return await GetUserRolesAsync(siteId, -1, roleName);
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId, int userId, string roleName)
        {
            var url = $"{Apiurl}?siteid={siteId}";
            if (userId != -1)
            {
                url += $"&userid={userId}";
            }
            if (roleName != "")
            {
                url += $"&rolename={roleName}";
            }
            return await GetJsonAsync<List<UserRole>>(url);
        }

        public async Task<UserRole> GetUserRoleAsync(int userRoleId)
        {
            return await GetJsonAsync<UserRole>($"{Apiurl}/{userRoleId}");
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            return await PostJsonAsync<UserRole>(Apiurl, userRole);
        }

        public async Task<UserRole> UpdateUserRoleAsync(UserRole userRole)
        {
            return await PutJsonAsync<UserRole>($"{Apiurl}/{userRole.UserRoleId}", userRole);
        }

        public async Task DeleteUserRoleAsync(int userRoleId)
        {
            await DeleteAsync($"{Apiurl}/{userRoleId}");
        }
    }
}
