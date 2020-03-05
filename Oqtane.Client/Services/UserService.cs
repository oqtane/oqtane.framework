using Oqtane.Shared;
using Oqtane.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class UserService : ServiceBase, IUserService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public UserService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            _http = http;
            _siteState = sitestate;
            _navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "User"); }
        }

        public async Task<User> GetUserAsync(int UserId, int SiteId)
        {
            return await _http.GetJsonAsync<User>(apiurl + "/" + UserId.ToString() + "?siteid=" + SiteId.ToString());
        }

        public async Task<User> GetUserAsync(string Username, int SiteId)
        {
            return await _http.GetJsonAsync<User>(apiurl + "/name/" + Username + "?siteid=" + SiteId.ToString());
        }

        public async Task<User> AddUserAsync(User User)
        {
            try
            {
                return await _http.PostJsonAsync<User>(apiurl, User);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> AddUserAsync(User User, Alias Alias)
        {
            try
            {
                return await _http.PostJsonAsync<User>(CreateCrossTenantUrl(apiurl, Alias), User);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> UpdateUserAsync(User User)
        {
            return await _http.PutJsonAsync<User>(apiurl + "/" + User.UserId.ToString(), User);
        }
        public async Task DeleteUserAsync(int UserId)
        {
            await _http.DeleteAsync(apiurl + "/" + UserId.ToString());
        }

        public async Task<User> LoginUserAsync(User User, bool SetCookie, bool IsPersistent)
        {
            return await _http.PostJsonAsync<User>(apiurl + "/login?setcookie=" + SetCookie.ToString() + "&persistent=" + IsPersistent.ToString(), User);
        }

        public async Task LogoutUserAsync(User User)
        {
            // best practices recommend post is preferrable to get for logout
            await _http.PostJsonAsync(apiurl + "/logout", User); 
        }

        public async Task<User> VerifyEmailAsync(User User, string Token)
        {
            return await _http.PostJsonAsync<User>(apiurl + "/verify?token=" + Token, User);
        }

        public async Task ForgotPasswordAsync(User User)
        {
            await _http.PostJsonAsync(apiurl + "/forgot", User);
        }

        public async Task<User> ResetPasswordAsync(User User, string Token)
        {
            return await _http.PostJsonAsync<User>(apiurl + "/reset?token=" + Token, User);
        }

    }
}
