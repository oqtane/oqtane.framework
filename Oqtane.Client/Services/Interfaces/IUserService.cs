using Oqtane.Models;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Manage (get / update) user information
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get a <see cref="User"/> of a specific site
        /// </summary>
        /// <param name="userId">ID of a <see cref="User"/></param>
        /// <param name="siteId">ID of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<User> GetUserAsync(int userId, int siteId);

        
        /// <summary>
        /// Get a <see cref="User"/> of a specific site
        /// </summary>
        /// <param name="username">Username / login of a <see cref="User"/></param>
        /// <param name="siteId">ID of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<User> GetUserAsync(string username, int siteId);

        /// <summary>
        /// Save a user to the Database.
        /// The <see cref="User"/> object contains all the information incl. what <see cref="Site"/> it belongs to. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User> AddUserAsync(User user);

        /// <summary>
        /// Update an existing user in the database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// Delete / remove a user in the database
        /// </summary>
        /// <param name="userId">ID-reference to the <see cref="User"/></param>
        /// <param name="siteId">ID-reference to the <see cref="Site"/></param>
        /// <returns></returns>
        Task DeleteUserAsync(int userId, int siteId);

        /// <summary>
        /// Will login the specified <see cref="User"/>.
        /// 
        /// Note that this will probably not be a real User, but a user object where the `Username` and `Password` have been filled.
        /// </summary>
        /// <param name="user">A <see cref="User"/> object which should have at least the <see cref="User.Username"/> and <see cref="User.Password"/> set.</param>
        /// <param name="setCookie">Determines if the login should be stored in the cookie.</param>
        /// <param name="isPersistent">Determines if the login should be persisted in the cookie for a long time.</param>
        /// <returns></returns>
        Task<User> LoginUserAsync(User user, bool setCookie, bool isPersistent);

        /// <summary>
        /// Logout a <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task LogoutUserAsync(User user);

        /// <summary>
        /// Update e-mail verification status of a user.
        /// </summary>
        /// <param name="user">The <see cref="User"/> we're verifying</param>
        /// <param name="token">A Hash value in the URL which verifies this user got the e-mail (containing this token)</param>
        /// <returns></returns>
        Task<User> VerifyEmailAsync(User user, string token);

        /// <summary>
        /// Trigger a forgot-password e-mail for this <see cref="User"/>. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task ForgotPasswordAsync(User user);

        /// <summary>
        /// Reset the password of this <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> ResetPasswordAsync(User user, string token);
    }
}
