using Oqtane.Shared;
using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using System.Net;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UserService : ServiceBase, IUserService
    {
        public UserService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("User");

        public async Task<User> GetUserAsync(int userId, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/{userId}?siteid={siteId}");
        }

        public async Task<User> GetUserAsync(string username, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/name/{username}?siteid={siteId}");
        }

        public async Task<User> AddUserAsync(User user)
        {
            return await PostJsonAsync<User>(Apiurl, user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            return await PutJsonAsync<User>($"{Apiurl}/{user.UserId}", user);
        }

        public async Task DeleteUserAsync(int userId, int siteId)
        {
            await DeleteAsync($"{Apiurl}/{userId}?siteid={siteId}");
        }

        public async Task<User> LoginUserAsync(User user, bool setCookie, bool isPersistent)
        {
            return await PostJsonAsync<User>($"{Apiurl}/login?setcookie={setCookie}&persistent={isPersistent}", user);
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

        public async Task<User> VerifyTwoFactorAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/twofactor?token={token}", user);
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            return await GetJsonAsync<bool>($"{Apiurl}/validate/{WebUtility.UrlEncode(password)}");
        }

        public async Task<string> GetTokenAsync()
        {
            return await GetStringAsync($"{Apiurl}/token");
        }

        public async Task<string> GetPersonalAccessTokenAsync()
        {
            return await GetStringAsync($"{Apiurl}/personalaccesstoken");
        }

        public async Task<User> LinkUserAsync(User user, string token, string type, string key, string name)
        {
            return await PostJsonAsync<User>($"{Apiurl}/link?token={token}&type={type}&key={key}&name={name}", user);
        }

    }
}
