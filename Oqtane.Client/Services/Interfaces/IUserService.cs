using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();

        Task<User> GetUserAsync(int UserId, int SiteId);

        Task<User> GetUserAsync(string Username, int SiteId);

        Task<User> AddUserAsync(User User);

        Task<User> AddUserAsync(User User, Alias alias);

        Task<User> UpdateUserAsync(User User);

        Task DeleteUserAsync(int UserId);

        Task<User> LoginUserAsync(User User, bool SetCookie, bool IsPersistent);

        Task LogoutUserAsync(User User);
    }
}
