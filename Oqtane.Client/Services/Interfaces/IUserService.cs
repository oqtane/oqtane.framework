using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IUserService
    {
        Task<User> GetUserAsync(int userId, int siteId);

        Task<User> GetUserAsync(string username, int siteId);

        Task<User> AddUserAsync(User user);

        Task<User> AddUserAsync(User user, Alias alias);

        Task<User> UpdateUserAsync(User user);

        Task DeleteUserAsync(int userId);

        Task<User> LoginUserAsync(User user, bool setCookie, bool isPersistent);

        Task LogoutUserAsync(User user);

        Task<User> VerifyEmailAsync(User user, string token);

        Task ForgotPasswordAsync(User user);

        Task<User> ResetPasswordAsync(User user, string token);
    }
}
