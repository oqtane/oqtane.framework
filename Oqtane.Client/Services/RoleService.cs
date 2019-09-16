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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public RoleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Role"); }
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            List<Role> Roles = await http.GetJsonAsync<List<Role>>(apiurl);
            return Roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<List<Role>> GetRolesAsync(int SiteId)
        {
            List<Role> Roles = await http.GetJsonAsync<List<Role>>(apiurl + "?siteid=" + SiteId.ToString());
            return Roles.OrderBy(item => item.Name).ToList();
        }

        public async Task<Role> GetRoleAsync(int RoleId)
        {
            return await http.GetJsonAsync<Role>(apiurl + "/" + RoleId.ToString());
        }

        public async Task<Role> AddRoleAsync(Role Role)
        {
            return await http.PostJsonAsync<Role>(apiurl, Role);
        }

        public async Task<Role> UpdateRoleAsync(Role Role)
        {
            return await http.PutJsonAsync<Role>(apiurl + "/" + Role.SiteId.ToString(), Role);
        }
        public async Task DeleteRoleAsync(int RoleId)
        {
            await http.DeleteAsync(apiurl + "/" + RoleId.ToString());
        }
    }
}
