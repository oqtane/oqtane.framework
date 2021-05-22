using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UserRoleService : ServiceBase, IUserRoleService
    {
        
        private readonly SiteState _siteState;

        public UserRoleService(HttpClient http, SiteState siteState) : base(http)
        {            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("UserRole", _siteState.Alias);

        public async Task<List<UserRole>> GetUserRolesAsync(int siteId)
        {
            return await GetJsonAsync<List<UserRole>>($"{Apiurl}?siteid={siteId}");
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
