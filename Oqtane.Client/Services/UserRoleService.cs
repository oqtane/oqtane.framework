using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class UserRoleService : ServiceBase, IUserRoleService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public UserRoleService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "UserRole"); }
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId)
        {
            return await GetJsonAsync<List<UserRole>>($"{Apiurl}?siteid={siteId.ToString()}");
        }

        public async Task<UserRole> GetUserRoleAsync(int userRoleId)
        {
            return await GetJsonAsync<UserRole>($"{Apiurl}/{userRoleId.ToString()}");
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            return await PostJsonAsync<UserRole>(Apiurl, userRole);
        }

        public async Task<UserRole> UpdateUserRoleAsync(UserRole userRole)
        {
            return await PutJsonAsync<UserRole>($"{Apiurl}/{userRole.UserRoleId.ToString()}", userRole);
        }

        public async Task DeleteUserRoleAsync(int userRoleId)
        {
            await DeleteAsync($"{Apiurl}/{userRoleId.ToString()}");
        }
    }
}
