using System.Buffers.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

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
        /// Get a <see cref="User"/> of a specific site
        /// </summary>
        /// <param name="username">Username / login of a <see cref="User"/></param>
        /// <param name="email">email address of a <see cref="User"/></param>
        /// <param name="siteId">ID of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<User> GetUserAsync(string username, string email, int siteId);

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
        /// <param name="setCookie">Determines if the login cookie should be set (only relevant for Hybrid scenarios)</param>
        /// <param name="isPersistent">Determines if the login cookie should be persisted for a long time.</param>
        /// <returns></returns>
        Task<User> LoginUserAsync(User user, bool setCookie, bool isPersistent);

        /// <summary>
        /// Logout a <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task LogoutUserAsync(User user);

        /// <summary>
        /// Logout a <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task LogoutUserEverywhereAsync(User user);

        /// <summary>
        /// Update e-mail verification status of a user.
        /// </summary>
        /// <param name="user">The <see cref="User"/> we're verifying</param>
        /// <param name="token">A Hash value in the URL which verifies this user got the e-mail (containing this token)</param>
        /// <returns></returns>
        Task<User> VerifyEmailAsync(User user, string token);

        /// <summary>
        /// Trigger a forgot-password e-mail. 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<bool> ForgotPasswordAsync(string username);

        /// <summary>
        /// Trigger a username reminder e-mail. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> ForgotUsernameAsync(string email);

        /// <summary>
        /// Reset the password of this <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> ResetPasswordAsync(User user, string token);

        /// <summary>
        /// Verify the two factor verification code <see cref="User"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> VerifyTwoFactorAsync(User user, string token);

        /// <summary>
        /// Validate identity user info.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<UserValidateResult> ValidateUserAsync(string username, string email, string password);

        /// <summary>
        /// Validate a users password against the password policy 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ValidatePasswordAsync(string password);

        /// <summary>
        /// Get token for current user
        /// </summary>
        /// <returns></returns>
        Task<string> GetTokenAsync();

        /// <summary>
        /// Get personal access token for current user (administrators only)
        /// </summary>
        /// <returns></returns>
        Task<string> GetPersonalAccessTokenAsync();

        /// <summary>
        /// Get password requirements for site
        /// </summary>
        /// <param name="siteId">ID of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<string> GetPasswordRequirementsAsync(int siteId);

        /// <summary>
        /// Bulk import of users
        /// </summary>
        /// <param name="siteId">ID of a <see cref="Site"/></param>
        /// <param name="fileId">ID of a <see cref="File"/></param>
        /// <param name="notify">Indicates if new users should be notified by email</param>
        /// <returns></returns>
        Task<Dictionary<string, string>> ImportUsersAsync(int siteId, int fileId, bool notify);

        /// <summary>
        /// Get passkeys for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserPasskey>> GetPasskeysAsync(int userId);

        /// <summary>
        /// Update a user passkey
        /// </summary>
        /// <param name="passkey"></param>
        /// <returns></returns>
        Task<UserPasskey> UpdatePasskeyAsync(UserPasskey passkey);

        /// <summary>
        /// Delete a user passkey
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="credentialId"></param>
        /// <returns></returns>
        Task DeletePasskeyAsync(int userId, byte[] credentialId);

        /// <summary>
        /// Get logins for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserLogin>> GetLoginsAsync(int userId);

        /// <summary>
        /// Link an external login with a local user account
        /// </summary>
        /// <param name="user">The <see cref="User"/> we're verifying</param>
        /// <param name="token">A Hash value in the URL which verifies this user got the e-mail (containing this token)</param>
        /// <param name="type">External Login provider type</param>
        /// <param name="key">External Login provider key</param>
        /// <param name="name">External Login provider display name</param>
        /// <returns></returns>
        Task<User> AddLoginAsync(User user, string token, string type, string key, string name);

        /// <summary>
        /// Delete a user login
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task DeleteLoginAsync(int userId, string provider, string key);

        /// <summary>
        /// Send a login link
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> SendLoginLinkAsync(string email, string returnurl);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UserService : ServiceBase, IUserService
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UserService(IStringLocalizer<SharedResources> localizer, HttpClient http, SiteState siteState) : base(http, siteState)
        {
            _localizer = localizer;
        }

        private string Apiurl => CreateApiUrl("User");

        public async Task<User> GetUserAsync(int userId, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/{userId}?siteid={siteId}");
        }

        public async Task<User> GetUserAsync(string username, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/username/{username}?siteid={siteId}");
        }

        public async Task<User> GetUserAsync(string username, string email, int siteId)
        {
            return await GetJsonAsync<User>($"{Apiurl}/name/{(!string.IsNullOrEmpty(username) ? username : "-")}/{(!string.IsNullOrEmpty(email) ? email : "-")}/?siteid={siteId}");
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
            return await PostJsonAsync<User>($"{Apiurl}/signin?setcookie={setCookie}&persistent={isPersistent}", user);
        }

        public async Task LogoutUserAsync(User user)
        {
            await PostJsonAsync($"{Apiurl}/logout", user);
        }

        public async Task LogoutUserEverywhereAsync(User user)
        {
            await PostJsonAsync($"{Apiurl}/logouteverywhere", user);
        }

        public async Task<User> VerifyEmailAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/verify?token={token}", user);
        }

        public async Task<bool> ForgotPasswordAsync(string username)
        {
            return await GetJsonAsync<bool>($"{Apiurl}/forgotpassword/{WebUtility.UrlEncode(username)}");
        }

        public async Task<bool> ForgotUsernameAsync(string email)
        {
            return await GetJsonAsync<bool>($"{Apiurl}/forgotusername/{WebUtility.UrlEncode(email)}");
        }

        public async Task<User> ResetPasswordAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/reset?token={token}", user);
        }

        public async Task<User> VerifyTwoFactorAsync(User user, string token)
        {
            return await PostJsonAsync<User>($"{Apiurl}/twofactor?token={token}", user);
        }

        public async Task<UserValidateResult> ValidateUserAsync(string username, string email, string password)
        {
            return await GetJsonAsync<UserValidateResult>($"{Apiurl}/validateuser?username={WebUtility.UrlEncode(username)}&email={WebUtility.UrlEncode(email)}&password={WebUtility.UrlEncode(password)}");
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

        public async Task<string> GetPasswordRequirementsAsync(int siteId)
        {
            var requirements = await GetJsonAsync<Dictionary<string, string>>($"{Apiurl}/passwordrequirements/{siteId}");

            var minimumlength = (requirements.ContainsKey("IdentityOptions:Password:RequiredLength")) ? requirements["IdentityOptions:Password:RequiredLength"] : "6";
            var uniquecharacters = (requirements.ContainsKey("IdentityOptions:Password:RequiredUniqueChars")) ? requirements["IdentityOptions:Password:RequiredUniqueChars"] : "1";
            var requiredigit = bool.Parse((requirements.ContainsKey("IdentityOptions:Password:RequireDigit")) ? requirements["IdentityOptions:Password:RequireDigit"] : "true");
            var requireupper = bool.Parse((requirements.ContainsKey("IdentityOptions:Password:RequireUppercase")) ? requirements["IdentityOptions:Password:RequireUppercase"] : "true");
            var requirelower = bool.Parse((requirements.ContainsKey("IdentityOptions:Password:RequireLowercase")) ? requirements["IdentityOptions:Password:RequireLowercase"] : "true");
            var requirepunctuation = bool.Parse((requirements.ContainsKey("IdentityOptions:Password:RequireNonAlphanumeric")) ? requirements["IdentityOptions:Password:RequireNonAlphanumeric"] : "true");

            // replace the placeholders with the setting values
            string digitRequirement = requiredigit ? _localizer["Password.DigitRequirement"] + ", " : "";
            string uppercaseRequirement = requireupper ? _localizer["Password.UppercaseRequirement"] + ", " : "";
            string lowercaseRequirement = requirelower ? _localizer["Password.LowercaseRequirement"] + ", " : "";
            string punctuationRequirement = requirepunctuation ? _localizer["Password.PunctuationRequirement"] + ", " : "";
            string passwordValidationCriteriaTemplate = _localizer["Password.ValidationCriteria"];

            // format requirements
            return string.Format(passwordValidationCriteriaTemplate, minimumlength, uniquecharacters, digitRequirement, uppercaseRequirement, lowercaseRequirement, punctuationRequirement);
        }

        public async Task<Dictionary<string, string>> ImportUsersAsync(int siteId, int fileId, bool notify)
        {
            return await PostJsonAsync<Dictionary<string, string>>($"{Apiurl}/import?siteid={siteId}&fileid={fileId}&notify={notify}", null);
        }

        public async Task<List<UserPasskey>> GetPasskeysAsync(int userId)
        {
            return await GetJsonAsync<List<UserPasskey>>($"{Apiurl}/passkey?id={userId}");
        }

        public async Task<UserPasskey> UpdatePasskeyAsync(UserPasskey passkey)
        {
            return await PutJsonAsync<UserPasskey>($"{Apiurl}/passkey", passkey);
        }

        public async Task DeletePasskeyAsync(int userId, byte[] credentialId)
        {
            await DeleteAsync($"{Apiurl}/passkey?id={userId}&credential={Base64Url.EncodeToString(credentialId)}");
        }

        public async Task<List<UserLogin>> GetLoginsAsync(int userId)
        {
            return await GetJsonAsync<List<UserLogin>>($"{Apiurl}/login?id={userId}");
        }

        public async Task<User> AddLoginAsync(User user, string token, string type, string key, string name)
        {
            return await PostJsonAsync<User>($"{Apiurl}/login?token={token}&type={type}&key={key}&name={name}", user);
        }

        public async Task DeleteLoginAsync(int userId, string provider, string key)
        {
            await DeleteAsync($"{Apiurl}/login?id={userId}&provider={provider}&key={key}");
        }

        public async Task<bool> SendLoginLinkAsync(string email, string returnurl)
        {
            return await GetJsonAsync<bool>($"{Apiurl}/loginlink/{WebUtility.UrlEncode(email)}/{WebUtility.UrlEncode(returnurl)}");
        }
    }
}
