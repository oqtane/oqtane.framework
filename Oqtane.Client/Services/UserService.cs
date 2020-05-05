using Oqtane.Shared;
using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class UserService : ServiceBase, IUserService
    {
        private readonly SiteState _siteState;

        public UserService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "User");

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

        public async Task DeleteUserAsync(int userId)
        {
            await DeleteAsync($"{Apiurl}/{userId}");
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
    }
}
