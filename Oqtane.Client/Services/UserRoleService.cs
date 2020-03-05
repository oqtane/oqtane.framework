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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public UserRoleService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            _http = http;
            _siteState = sitestate;
            _navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "UserRole"); }
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int SiteId)
        {
            return await _http.GetJsonAsync<List<UserRole>>(apiurl + "?siteid=" + SiteId.ToString());
        }

        public async Task<UserRole> GetUserRoleAsync(int UserRoleId)
        {
            return await _http.GetJsonAsync<UserRole>(apiurl + "/" + UserRoleId.ToString());
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole UserRole)
        {
            return await _http.PostJsonAsync<UserRole>(apiurl, UserRole);
        }

        public async Task<UserRole> UpdateUserRoleAsync(UserRole UserRole)
        {
            return await _http.PutJsonAsync<UserRole>(apiurl + "/" + UserRole.UserRoleId.ToString(), UserRole);
        }

        public async Task DeleteUserRoleAsync(int UserRoleId)
        {
            await _http.DeleteAsync(apiurl + "/" + UserRoleId.ToString());
        }
    }
}
