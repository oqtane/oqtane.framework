using Oqtane.Shared;
using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oqtane.Modules.Admin.Roles;
using System.Xml.Linq;

namespace Oqtane.Services
{
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
            return await GetUserAsync(username, "", siteId);
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
    }
}
