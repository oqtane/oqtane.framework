using Oqtane.Shared;
using Oqtane.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class UserService : ServiceBase, IUserService
    {
        
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;
        private readonly ISiteService _siteService;        

        public UserService(HttpClient http, SiteState siteState, NavigationManager navigationManager, ISiteService siteService) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
            _siteService = siteService;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "User"); }
        }

        public async Task<User> GetUserAsync(int userId, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/{userId.ToString()}?siteid={siteId.ToString()}");
        }

        public async Task<User> GetUserAsync(string username, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/name/{username}?siteid={siteId.ToString()}");
        }

        public async Task<User> AddUserAsync(User user)
        {
            Site site = await _siteService.GetSiteAsync(_siteState.Alias.SiteId, _siteState.Alias);

            if (!site.AllowRegistration)
            {
                return null;
            }

            try
            {
                return await PostJsonAsync<User>(Apiurl, user);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> AddUserAsync(User user, Alias alias)
        {
            try
            {
                return await PostJsonAsync<User>(CreateCrossTenantUrl(Apiurl, alias), user);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            return await PutJsonAsync<User>($"{Apiurl}/{user.UserId.ToString()}", user);
        }
        public async Task DeleteUserAsync(int userId)
        {
            await DeleteAsync($"{Apiurl}/{userId.ToString()}");
        }

        public async Task<User> LoginUserAsync(User user, bool setCookie, bool isPersistent)
        {
            return await PostJsonAsync<User>($"{Apiurl}/login?setcookie={setCookie.ToString()}&persistent={isPersistent.ToString()}", user);
        }

        public async Task LogoutUserAsync(User user)
        {
            // best practices recommend post is preferrable to get for logout
            await PostJsonAsync($"{Apiurl}/logout", user); 
        }

        public async Task<User> VerifyEmailAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/verify?token={token}", user);
        }

        public async Task ForgotPasswordAsync(User user)
        {
            await PostJsonAsync($"{Apiurl}/forgot", user);
        }

        public async Task<User> ResetPasswordAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/reset?token={token}", user);
        }

    }
}
