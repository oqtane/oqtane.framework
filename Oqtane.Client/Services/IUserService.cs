using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();

        Task<User> GetUserAsync(int UserId);

        Task<User> GetUserAsync(string Username);

        Task AddUserAsync(User user);

        Task UpdateUserAsync(User user);

        Task DeleteUserAsync(int UserId);

        Task<User> GetCurrentUserAsync();

        Task<User> LoginUserAsync(User user);

        Task LogoutUserAsync();

        bool IsAuthorized(User user, string accesscontrollist);
    }
}
