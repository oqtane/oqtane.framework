using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Managers
{
    public interface IUserManager
    {
        User GetUser(int userid, int siteid);
        User GetUser(string username, int siteid);
        User GetUser(string username, string email, int siteid);
        Task<User> AddUser(User user);
        Task<User> UpdateUser(User user);
        Task DeleteUser(int userid, int siteid);
        Task<User> LoginUser(User user, bool setCookie, bool isPersistent);
        Task<User> VerifyEmail(User user, string token);
        Task ForgotPassword(User user);
        Task<User> ResetPassword(User user, string token);
        User VerifyTwoFactor(User user, string token);
        Task<User> LinkExternalAccount(User user, string token, string type, string key, string name);
        Task<bool> ValidatePassword(string password);
        Task<Dictionary<string, string>> ImportUsers(int siteId, string filePath, bool notify);
    }
}
