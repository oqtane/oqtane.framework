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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public RoleService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Role"); }
        }

        public async Task<List<Role>> GetRolesAsync(int siteId)
        {
            List<Role> roles = await _http.GetJsonAsync<List<Role>>(Apiurl + "?siteid=" + siteId.ToString());
            return roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<Role> GetRoleAsync(int roleId)
        {
            return await _http.GetJsonAsync<Role>(Apiurl + "/" + roleId.ToString());
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            return await _http.PostJsonAsync<Role>(Apiurl, role);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            return await _http.PutJsonAsync<Role>(Apiurl + "/" + role.RoleId.ToString(), role);
        }
        public async Task DeleteRoleAsync(int roleId)
        {
            await _http.DeleteAsync(Apiurl + "/" + roleId.ToString());
        }
    }
}
