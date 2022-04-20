using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
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
