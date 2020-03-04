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

        public RoleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Role"); }
        }

        public async Task<List<Role>> GetRolesAsync(int SiteId)
        {
            List<Role> Roles = await _http.GetJsonAsync<List<Role>>(apiurl + "?siteid=" + SiteId.ToString());
            return Roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<Role> GetRoleAsync(int RoleId)
        {
            return await _http.GetJsonAsync<Role>(apiurl + "/" + RoleId.ToString());
        }

        public async Task<Role> AddRoleAsync(Role Role)
        {
            return await _http.PostJsonAsync<Role>(apiurl, Role);
        }

        public async Task<Role> UpdateRoleAsync(Role Role)
        {
            return await _http.PutJsonAsync<Role>(apiurl + "/" + Role.RoleId.ToString(), Role);
        }
        public async Task DeleteRoleAsync(int RoleId)
        {
            await _http.DeleteAsync(apiurl + "/" + RoleId.ToString());
        }
    }
}
