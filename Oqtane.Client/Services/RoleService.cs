using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class RoleService : ServiceBase, IRoleService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public RoleService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Role"); }
        }

        public async Task<List<Role>> GetRolesAsync(int siteId)
        {
            List<Role> roles = await GetJsonAsync<List<Role>>($"{Apiurl}?siteid={siteId.ToString()}");
            return roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<Role> GetRoleAsync(int roleId)
        {
            return await GetJsonAsync<Role>($"{Apiurl}/{roleId.ToString()}");
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            return await PostJsonAsync<Role>(Apiurl, role);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            return await PutJsonAsync<Role>($"{Apiurl}/{role.RoleId.ToString()}", role);
        }
        public async Task DeleteRoleAsync(int roleId)
        {
            await DeleteAsync($"{Apiurl}/{roleId.ToString()}");
        }
    }
}
