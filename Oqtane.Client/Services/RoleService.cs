using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class RoleService : ServiceBase, IRoleService
    {
        
        private readonly SiteState _siteState;

        public RoleService(HttpClient http, SiteState siteState) : base(http)
        {
            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Role");

        public async Task<List<Role>> GetRolesAsync(int siteId)
        {
            List<Role> roles = await GetJsonAsync<List<Role>>($"{Apiurl}?siteid={siteId}");
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
