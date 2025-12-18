using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Managers;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class UserController : Controller
    {
        private readonly IUserRepository _users;
        private readonly ITenantManager _tenantManager;
        private readonly IUserManager _userManager;
        private readonly ISiteRepository _sites;
        private readonly IUserPermissions _userPermissions;
        private readonly IJwtManager _jwtManager;
        private readonly IFileRepository _files;
        private readonly ISettingRepository _settings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public UserController(IUserRepository users, ITenantManager tenantManager, IUserManager userManager, ISiteRepository sites, IUserPermissions userPermissions, IJwtManager jwtManager, IFileRepository files, ISettingRepository settings, ISyncManager syncManager, ILogManager logger)
        {
            _users = users;
            _tenantManager = tenantManager;
            _userManager = userManager;
            _sites = sites;
            _userPermissions = userPermissions;
            _jwtManager = jwtManager;
            _files = files;
            _settings = settings;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        [Authorize]
        public User Get(int id, string siteid)
        {
            if (int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                User user = _userManager.GetUser(id, SiteId);
                if (user == null)
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return Filter(user);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Get Attempt {UserId} {SiteId}", id, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/username/{username}?siteid=x
        [HttpGet("username/{username}")]
        public User Get(string username, string siteid)
        {
            if (int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                User user = _userManager.GetUser(username, SiteId);
                if (user == null)
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return Filter(user);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Get Attempt {Username} {SiteId}", username, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/name/{username}/{email}?siteid=x
        [HttpGet("search/{username}/{email}")]
        public User Get(string username, string email, string siteid)
        {
            if (int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                username = (username == "-") ? "" : username;
                email = (email == "-") ? "" : email;
                User user = _userManager.GetUser(username, email, SiteId);
                if (user == null)
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return Filter(user);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Get Attempt {Username} {Email} {SiteId}", username, email, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        private User Filter(User user)
        {
            // clone object to avoid mutating cache 
            User filtered = null;

            if (user != null)
            {
                filtered = new User();

                // public properties
                filtered.SiteId = user.SiteId;
                filtered.UserId = user.UserId;
                filtered.Username = user.Username;
                filtered.DisplayName = user.DisplayName;

                // restricted properties
                filtered.Password = "";
                filtered.TwoFactorCode = "";
                filtered.SecurityStamp = "";

                // include private properties if authenticated user is accessing their own user account or is an administrator
                if (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == user.UserId)
                {
                    filtered.Email = user.Email;
                    filtered.TimeZoneId = user.TimeZoneId;
                    filtered.PhotoFileId = user.PhotoFileId;
                    filtered.LastLoginOn = user.LastLoginOn;
                    filtered.LastIPAddress = user.LastIPAddress;
                    filtered.TwoFactorRequired = user.TwoFactorRequired;
                    filtered.EmailConfirmed = user.EmailConfirmed;
                    filtered.Roles = user.Roles;
                    filtered.CreatedBy = user.CreatedBy;
                    filtered.CreatedOn = user.CreatedOn;
                    filtered.ModifiedBy = user.ModifiedBy;
                    filtered.ModifiedOn = user.ModifiedOn;
                    filtered.DeletedBy = user.DeletedBy;
                    filtered.DeletedOn = user.DeletedOn;
                    filtered.IsDeleted = user.IsDeleted;
                    filtered.Settings = user.Settings; // include all settings
                }
            }

            return filtered;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<User> Post([FromBody] User user)
        {
            if (ModelState.IsValid && user.SiteId == _tenantManager.GetAlias().SiteId)
            {
                bool allowregistration;
                if (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin))
                {
                    user.IsAuthenticated = true; // admins can add any existing user to a site
                    allowregistration = true;
                }
                else
                {
                    user.EmailConfirmed = false; // standard users cannot specify that their email is verified
                    user.IsAuthenticated = false; // existing users can only be added to a site if they provide a valid username and password
                    allowregistration = _sites.GetSite(user.SiteId).AllowRegistration;
                }

                if (allowregistration)
                {
                    user = await _userManager.AddUser(user);
                }
                else
                {
                    _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "User Registration Is Not Enabled For Site. User Was Not Added {User}", user);
                }

                return user;
            }
            else
            {
                user.Password = ""; // remove sensitive information
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Post Attempt {User}", user);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<User> Put(int id, [FromBody] User user)
        {
            var existing = _userManager.GetUser(user.UserId, user.SiteId);
            if (ModelState.IsValid && user.SiteId == _tenantManager.GetAlias().SiteId && user.UserId == id && existing != null
                && (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || User.Identity.Name == user.Username))
            {
                // only authorized users can update the email confirmation
                if (!_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin))
                {
                    user.EmailConfirmed = existing.EmailConfirmed;
                }
                user = await _userManager.UpdateUser(user);
            }
            else
            {
                user.Password = ""; // remove sensitive information
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Put Attempt {User}", user);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                user = null;
            }
            return user;
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = $"{EntityNames.User}:{PermissionNames.Write}:{RoleNames.Host}")]
        public async Task Delete(int id, string siteid)
        {
            User user = _users.GetUser(id, false);
            if (user != null && int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                await _userManager.DeleteUser(id, SiteId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Delete Attempt {UserId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // POST api/<controller>/signin
        [HttpPost("signin")]
        public async Task<User> Login([FromBody] User user, bool setCookie, bool isPersistent)
        {
            if (ModelState.IsValid)
            {
                user = await _userManager.LoginUser(user, setCookie, isPersistent);
            }
            else
            {
                user = new User { SiteId = user.SiteId, Username = user.Username, IsAuthenticated = false };
            }
            return user;
        }

        // POST api/<controller>/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task Logout([FromBody] User user)
        {
            if (_userPermissions.GetUser(User).UserId == user.UserId)
            {
                await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
                _syncManager.AddSyncEvent(_tenantManager.GetAlias(), EntityNames.User, user.UserId, "Logout");
                _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout {Username}", (user != null) ? user.Username : "");
            }
        }

        // POST api/<controller>/logout
        [HttpPost("logouteverywhere")]
        [Authorize]
        public async Task LogoutEverywhere([FromBody] User user)
        {
            if (_userPermissions.GetUser(User).UserId == user.UserId)
            {
                await _userManager.LogoutUserEverywhere(user);
                await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
                _syncManager.AddSyncEvent(_tenantManager.GetAlias(), EntityNames.User, user.UserId, "Logout");
                _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout Everywhere {Username}", (user != null) ? user.Username : "");
            }
        }

        // POST api/<controller>/verify
        [HttpPost("verify")]
        public async Task<User> Verify([FromBody] User user, string token)
        {
            if (ModelState.IsValid)
            {
                user = await _userManager.VerifyEmail(user, token);
            }
            return user;
        }

        // GET api/<controller>/forgotpassword/x
        [HttpGet("forgotpassword/{username}")]
        public async Task<bool> ForgotPassword(string username)
        {
            return await _userManager.ForgotPassword(username);
        }

        // GET api/<controller>/forgotusername/x
        [HttpGet("forgotusername/{email}")]
        public async Task<bool> ForgotUsername(string email)
        {
            return await _userManager.ForgotUsername(email);
        }

        // POST api/<controller>/reset
        [HttpPost("reset")]
        public async Task<User> Reset([FromBody] User user, string token)
        {
            if (ModelState.IsValid)
            {
                user = await _userManager.ResetPassword(user, token);
            }
            return user;
        }

        // POST api/<controller>/twofactor
        [HttpPost("twofactor")]
        public User TwoFactor([FromBody] User user, string token)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(token))
            {
                user = _userManager.VerifyTwoFactor(user, token);
            }
            else
            {
                user.IsAuthenticated = false;
            }

            return user;
        }

        // GET api/<controller>/validate/x
        [HttpGet("validateuser")]
        public async Task<UserValidateResult> ValidateUser(string username, string email, string password)
        {
            return await _userManager.ValidateUser(username, email, password);
        }

        // GET api/<controller>/validate/x
        [HttpGet("validate/{password}")]
        public async Task<bool> Validate(string password)
        {
            return await _userManager.ValidatePassword(password);
        }

        // GET api/<controller>/token
        [HttpGet("token")]
        [Authorize(Roles = RoleNames.Registered)]
        public string Token()
        {
            var token = "";
            var sitesettings = HttpContext.GetSiteSettings();
            var secret = sitesettings.GetValue("JwtOptions:Secret", "");
            if (!string.IsNullOrEmpty(secret))
            {
                token = _jwtManager.GenerateToken(_tenantManager.GetAlias(), (ClaimsIdentity)User.Identity, secret, sitesettings.GetValue("JwtOptions:Issuer", ""), sitesettings.GetValue("JwtOptions:Audience", ""), int.Parse(sitesettings.GetValue("JwtOptions:Lifetime", "20")));
            }
            return token;
        }

        // GET api/<controller>/personalaccesstoken
        [HttpGet("personalaccesstoken")]
        [Authorize(Roles = RoleNames.Admin)]
        public string PersonalAccessToken()
        {
            var token = "";
            var sitesettings = HttpContext.GetSiteSettings();
            var secret = sitesettings.GetValue("JwtOptions:Secret", "");
            if (!string.IsNullOrEmpty(secret))
            {
                var lifetime = 525600; // long-lived token set to 1 year
                token = _jwtManager.GenerateToken(_tenantManager.GetAlias(), (ClaimsIdentity)User.Identity, secret, sitesettings.GetValue("JwtOptions:Issuer", ""), sitesettings.GetValue("JwtOptions:Audience", ""), lifetime);
            }
            return token;
        }

        // GET api/<controller>/authenticate
        [HttpGet("authenticate")]
        public User Authenticate()
        {
            User user = new User { IsAuthenticated = User.Identity.IsAuthenticated, Username = "", UserId = -1, Roles = "" };
            if (user.IsAuthenticated)
            {
                user.Username = User.Identity.Name;
                user.UserId = User.UserId();
                string roles = "";
                foreach (var roleName in User.Roles())
                {
                    roles += roleName + ";";
                }
                if (roles != "") roles = ";" + roles;
                user.Roles = roles;
                user.SecurityStamp = User.SecurityStamp();
            }
            return user;
        }

        // GET api/<controller>/passwordrequirements/5
        [HttpGet("passwordrequirements/{siteid}")]
        public Dictionary<string, string> PasswordRequirements(int siteid)
        {
            var requirements = new Dictionary<string, string>();

            var site = _sites.GetSite(siteid);
            if (site != null && (site.AllowRegistration || User.IsInRole(RoleNames.Registered)))
            {
                // get password settings
                var sitesettings = HttpContext.GetSiteSettings();
                requirements = sitesettings.Where(item => item.Key.StartsWith("IdentityOptions:Password:"))
                    .ToDictionary(item => item.Key, item => item.Value);
            }

            return requirements;
        }

        // POST api/<controller>/import?siteid=x&fileid=y&notify=z
        [HttpPost("import")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<Dictionary<string, string>> Import(string siteid, string fileid, string notify)
        {
            if (int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId && int.TryParse(fileid, out int FileId) && bool.TryParse(notify, out bool Notify))
            {
                var file = _files.GetFile(FileId);
                if (file != null)
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
                    {
                        return await _userManager.ImportUsers(SiteId, _files.GetFilePath(file), Notify);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Import Attempt {SiteId} {FileId}", siteid, fileid);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Import File Does Not Exist {SiteId} {FileId}", siteid, fileid);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Import Attempt {SiteId} {FileId}", siteid, fileid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET: api/<controller>/passkey?id=x
        [HttpGet("passkey")]
        [Authorize]
        public async Task<IEnumerable<UserPasskey>> GetPasskeys(int id)
        {
            if (_userPermissions.IsAuthorized(User, _tenantManager.GetAlias().SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == id)
            {
                return await _userManager.GetPasskeys(id, _tenantManager.GetAlias().SiteId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Passkey Get Attempt {UserId} {SiteId}", id, _tenantManager.GetAlias().SiteId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/passkey
        [HttpPut("passkey")]
        [Authorize]
        public async Task UpdatePasskey([FromBody] UserPasskey passkey)
        {
            if (ModelState.IsValid)
            {
                if (_userPermissions.IsAuthorized(User, _tenantManager.GetAlias().SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == passkey.UserId)
                {
                    // passkey name is prefixed with SiteId for multi-tenancy
                    passkey.Name = $"{_tenantManager.GetAlias().SiteId}:" + passkey.Name;
                    await _userManager.UpdatePasskey(passkey);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Passkey Put Attempt {PassKey}", passkey);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Passkey Put Attempt {PassKey}", passkey);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/passkey?id=x&credential=y
        [HttpDelete("passkey")]
        [Authorize]
        public async Task DeletePasskey(int id, string credential)
        {
            if (_userPermissions.IsAuthorized(User, _tenantManager.GetAlias().SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == id)
            {
                await _userManager.DeletePasskey(id, Base64Url.DecodeFromChars(credential));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Passkey Delete Attempt {UserId} {Credential}", id, credential);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET: api/<controller>/login?id=x
        [HttpGet("login")]
        [Authorize]
        public async Task<IEnumerable<UserLogin>> GetLogins(int id)
        {
            if (_userPermissions.IsAuthorized(User, _tenantManager.GetAlias().SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == id)
            {
                return await _userManager.GetLogins(id, _tenantManager.GetAlias().SiteId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized External Login Get Attempt {UserId} {SiteId}", id, _tenantManager.GetAlias().SiteId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/login
        [HttpPost("login")]
        public async Task<User> AddLogin([FromBody] User user, string token, string type, string key, string name)
        {
            if (ModelState.IsValid)
            {
                user = await _userManager.AddLogin(user, token, type, key, name);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized External Login Post Attempt {Username} {Token}", user.Username, token);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                user = null;
            }
            return user;
        }

        // DELETE api/<controller>/login?id=x&provider=y&key=z
        [HttpDelete("login")]
        [Authorize]
        public async Task DeleteLogin(int id, string provider, string key)
        {
            if (_userPermissions.IsAuthorized(User, _tenantManager.GetAlias().SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || _userPermissions.GetUser(User).UserId == id)
            {
                await _userManager.DeleteLogin(id, provider, key);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized External Login Delete Attempt {UserId} {Provider} {Key}", id, provider, key);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET api/<controller>/loginlink/x/y
        [HttpGet("loginlink/{email}/{returnurl}")]
        public async Task<bool> SendLoginLink(string email, string returnurl)
        {
            return await _userManager.SendLoginLink(email, returnurl);
        }
    }
}
