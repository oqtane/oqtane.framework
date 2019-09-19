using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class UserRoleService : ServiceBase, IUserRoleService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public UserRoleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "UserRole"); }
        }

        public async Task<List<UserRole>> GetUserRolesAsync()
        {
            return await http.GetJsonAsync<List<UserRole>>(apiurl);
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int SiteId)
        {
            return await http.GetJsonAsync<List<UserRole>>(apiurl + "?siteid=" + SiteId.ToString());
        }

        public async Task<UserRole> GetUserRoleAsync(int UserRoleId)
        {
            return await http.GetJsonAsync<UserRole>(apiurl + "/" + UserRoleId.ToString());
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole UserRole)
        {
            return await http.PostJsonAsync<UserRole>(apiurl, UserRole);
        }

        public async Task<UserRole> UpdateUserRoleAsync(UserRole UserRole)
        {
            return await http.PutJsonAsync<UserRole>(apiurl + "/" + UserRole.UserRoleId.ToString(), UserRole);
        }

        public async Task DeleteUserRoleAsync(int UserRoleId)
        {
            await http.DeleteAsync(apiurl + "/" + UserRoleId.ToString());
        }
    }
}
