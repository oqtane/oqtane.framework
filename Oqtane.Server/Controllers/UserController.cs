using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Oqtane.Shared;
using System;
using System.IO;
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Extensions;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class UserController : Controller
    {
        private readonly IUserRepository _users;
        private readonly IUserRoleRepository _userRoles;
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly ITenantManager _tenantManager;
        private readonly INotificationRepository _notifications;
        private readonly IFolderRepository _folders;
        private readonly ISiteRepository _sites;
        private readonly IUserPermissions _userPermissions;
        private readonly IJwtManager _jwtManager;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public UserController(IUserRepository users, IUserRoleRepository userRoles, UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, ITenantManager tenantManager, INotificationRepository notifications, IFolderRepository folders, ISiteRepository sites, IUserPermissions userPermissions, IJwtManager jwtManager, ISyncManager syncManager, ILogManager logger)
        {
            _users = users;
            _userRoles = userRoles;
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _tenantManager = tenantManager;
            _notifications = notifications;
            _folders = folders;
            _sites = sites;
            _userPermissions = userPermissions;
            _jwtManager = jwtManager;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        [Authorize]
        public User Get(int id, string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                User user = _users.GetUser(id);
                if (user != null)
                {
                    user.SiteId = int.Parse(siteid);
                    user.Roles = GetUserRoles(user.UserId, user.SiteId);
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

        // GET api/<controller>/name/x?siteid=x
        [HttpGet("name/{name}")]
        public User Get(string name, string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                User user = _users.GetUser(name);
                if (user != null)
                {
                    user.SiteId = int.Parse(siteid);
                    user.Roles = GetUserRoles(user.UserId, user.SiteId);
                }
                return Filter(user);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Get Attempt {Username} {SiteId}", name, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        private User Filter(User user)
        {
            if (user != null)
            {
                user.Password = "";
                user.IsAuthenticated = false;
                user.TwoFactorCode = "";
                user.TwoFactorExpiry = null;

                if (!_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) && User.Identity.Name?.ToLower() != user.Username.ToLower())
                {
                    user.Email = "";
                    user.PhotoFileId = null;
                    user.LastLoginOn = DateTime.MinValue;
                    user.LastIPAddress = "";
                    user.Roles = "";
                    user.CreatedBy = "";
                    user.CreatedOn = DateTime.MinValue;
                    user.ModifiedBy = "";
                    user.ModifiedOn = DateTime.MinValue;
                    user.DeletedBy = "";
                    user.DeletedOn = DateTime.MinValue;
                    user.IsDeleted = false;
                    user.TwoFactorRequired = false;
                }
            }
            return user;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<User> Post([FromBody] User user)
        {
            if (ModelState.IsValid && user.SiteId == _tenantManager.GetAlias().SiteId)
            {
                var User = await CreateUser(user);
                return User;
            }
            else
            {
                user.Password = ""; // remove sensitive information
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Post Attempt {User}", user);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        private async Task<User> CreateUser(User user)
        {
            User newUser = null;

            bool verified;
            bool allowregistration;
            if (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin))
            {
                verified = true;
                allowregistration = true;
            }
            else
            {
                verified = false;
                allowregistration = _sites.GetSite(user.SiteId).AllowRegistration;
            }

            if (allowregistration)
            {
                bool succeeded;
                string errors = "";
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = user.Username;
                    identityuser.Email = user.Email;
                    identityuser.EmailConfirmed = verified;
                    var result = await _identityUserManager.CreateAsync(identityuser, user.Password);
                    succeeded = result.Succeeded;
                    if (!succeeded)
                    {
                        errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                    succeeded = result.Succeeded;
                    if (!succeeded)
                    {
                        errors = "Password Not Valid For User";
                    }
                    verified = succeeded;
                }

                if (succeeded)
                {
                    user.LastLoginOn = null;
                    user.LastIPAddress = "";
                    newUser = _users.AddUser(user);
                    _syncManager.AddSyncEvent(_tenantManager.GetAlias().TenantId, EntityNames.User, newUser.UserId, SyncEventActions.Create);
                }
                else
                {
                    _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "Unable To Add User {Username} - {Errors}", user.Username, errors);
                }

                if (newUser != null)
                {
                    if (!verified)
                    {
                        string token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(identityuser);
                        string url = HttpContext.Request.Scheme + "://" + _tenantManager.GetAlias().Name + "/login?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                        string body = "Dear " + user.DisplayName + ",\n\nIn Order To Complete The Registration Of Your User Account Please Click The Link Displayed Below:\n\n" + url + "\n\nThank You!";
                        var notification = new Notification(user.SiteId, newUser, "User Account Verification", body);
                        _notifications.AddNotification(notification);
                    }
                    else
                    {
                        string url = HttpContext.Request.Scheme + "://" + _tenantManager.GetAlias().Name;
                        string body = "Dear " + user.DisplayName + ",\n\nA User Account Has Been Successfully Created For You. Please Use The Following Link To Access The Site:\n\n" + url + "\n\nThank You!";
                        var notification = new Notification(user.SiteId, newUser, "User Account Notification", body);
                        _notifications.AddNotification(notification);
                    }

                    newUser.Password = ""; // remove sensitive information
                    _logger.Log(user.SiteId, LogLevel.Information, this, LogFunction.Create, "User Added {User}", newUser);
                }
                else
                {
                    user.Password = ""; // remove sensitive information
                    _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "Unable To Add User {User}", user);
                }
            }
            else
            {
                _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Create, "User Registration Is Not Enabled For Site. User Was Not Added {User}", user);
            }

            return newUser;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<User> Put(int id, [FromBody] User user)
        {
            if (ModelState.IsValid && user.SiteId == _tenantManager.GetAlias().SiteId && _users.GetUser(user.UserId, false) != null
                && (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || User.Identity.Name == user.Username))
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    identityuser.Email = user.Email;
                    var valid = true;
                    if (user.Password != "")
                    {
                        var validator = new PasswordValidator<IdentityUser>();
                        var result = await validator.ValidateAsync(_identityUserManager, null, user.Password);
                        valid = result.Succeeded;
                        if (valid)
                        {
                            identityuser.PasswordHash = _identityUserManager.PasswordHasher.HashPassword(identityuser, user.Password);
                        }
                    }
                    if (valid)
                    {
                        await _identityUserManager.UpdateAsync(identityuser);

                        user = _users.UpdateUser(user);
                        _syncManager.AddSyncEvent(_tenantManager.GetAlias().TenantId, EntityNames.User, user.UserId, SyncEventActions.Update);
                        _syncManager.AddSyncEvent(_tenantManager.GetAlias().TenantId, EntityNames.User, user.UserId, SyncEventActions.Refresh);
                        user.Password = ""; // remove sensitive information
                        _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Updated {User}", user);
                    }
                    else
                    {
                        _logger.Log(user.SiteId, LogLevel.Error, this, LogFunction.Update, "Unable To Update User {Username}. Password Does Not Meet Complexity Requirements.", user.Username);
                        user = null;
                    }
                }
            }
            else
            {
                user.Password = ""; // remove sensitive information
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Post Attempt {User}", user);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                user = null;
            }
            return user;
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = $"{EntityNames.User}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public async Task Delete(int id, string siteid)
        {
            int SiteId;
            User user = _users.GetUser(id);
            if (user != null && int.TryParse(siteid, out SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                // remove user roles for site
                foreach (UserRole userrole in _userRoles.GetUserRoles(user.UserId, SiteId).ToList())
                {
                    _userRoles.DeleteUserRole(userrole.UserRoleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRole}", userrole);
                }

                // remove user folder for site
                var folder = _folders.GetFolder(SiteId, $"Users{user.UserId}/");
                if (folder != null)
                {
                    if (Directory.Exists(_folders.GetFolderPath(folder)))
                    {
                        Directory.Delete(_folders.GetFolderPath(folder), true);
                    }
                    _folders.DeleteFolder(folder.FolderId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Folder Deleted {Folder}", folder);
                }

                // delete user if they are not a member of any other sites
                if (!_userRoles.GetUserRoles(user.UserId, -1).Any())
                {
                    // get identity user
                    IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                    if (identityuser != null)
                    {
                        // delete identity user
                        var result = await _identityUserManager.DeleteAsync(identityuser);
                        if (result != null)
                        {
                            // delete user
                            _users.DeleteUser(user.UserId);
                            _syncManager.AddSyncEvent(_tenantManager.GetAlias().TenantId, EntityNames.User, user.UserId, SyncEventActions.Delete);
                            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Deleted {UserId}", user.UserId, result.ToString());
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Error Deleting User {UserId}", user.UserId);
                        }
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized User Delete Attempt {UserId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        public async Task<User> Login([FromBody] User user, bool setCookie, bool isPersistent)
        {
            User loginUser = new User { SiteId = user.SiteId, Username = user.Username, IsAuthenticated = false };

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, true);
                    if (result.Succeeded)
                    {
                        var LastIPAddress = user.LastIPAddress ?? "";

                        user = _users.GetUser(user.Username);
                        if (user.TwoFactorRequired)
                        {
                            var token = await _identityUserManager.GenerateTwoFactorTokenAsync(identityuser, "Email");
                            user.TwoFactorCode = token;
                            user.TwoFactorExpiry = DateTime.UtcNow.AddMinutes(10);
                            _users.UpdateUser(user);

                            string body = "Dear " + user.DisplayName + ",\n\nYou requested a secure verification code to log in to your account. Please enter the secure verification code on the site:\n\n" + token +
                                "\n\nPlease note that the code is only valid for 10 minutes so if you are unable to take action within that time period, you should initiate a new login on the site." +
                                "\n\nThank You!";
                            var notification = new Notification(loginUser.SiteId, user, "User Verification Code", body);
                            _notifications.AddNotification(notification);

                            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Verification Notification Sent For {Username}", user.Username);
                            loginUser.TwoFactorRequired = true;
                        }
                        else
                        {
                            loginUser = _users.GetUser(identityuser.UserName);
                            if (loginUser != null)
                            {
                                if (identityuser.EmailConfirmed)
                                {
                                    loginUser.IsAuthenticated = true;
                                    loginUser.LastLoginOn = DateTime.UtcNow;
                                    loginUser.LastIPAddress = LastIPAddress;
                                    _users.UpdateUser(loginUser);
                                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Login Successful {Username}", user.Username);

                                    if (setCookie)
                                    {
                                        await _identitySignInManager.SignInAsync(identityuser, isPersistent);
                                    }
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Not Verified {Username}", user.Username);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (result.IsLockedOut)
                        {
                            user = _users.GetUser(user.Username);
                            string token = await _identityUserManager.GeneratePasswordResetTokenAsync(identityuser);
                            string url = HttpContext.Request.Scheme + "://" + _tenantManager.GetAlias().Name + "/reset?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                            string body = "Dear " + user.DisplayName + ",\n\nYou attempted multiple times unsuccessfully to log in to your account and it is now locked out. Please wait a few minutes and then try again... or use the link below to reset your password:\n\n" + url +
                                "\n\nPlease note that the link is only valid for 24 hours so if you are unable to take action within that time period, you should initiate another password reset on the site." +
                                "\n\nThank You!";
                            var notification = new Notification(loginUser.SiteId, user, "User Lockout", body);
                            _notifications.AddNotification(notification);
                            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Lockout Notification Sent For {Username}", user.Username);
                        }
                        else
                        {
                            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Login Failed {Username}", user.Username);
                        }
                    }
                }
            }

            return loginUser;
        }

        // POST api/<controller>/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task Logout([FromBody] User user)
        {
            await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout {Username}", (user != null) ? user.Username : "");
        }

        // POST api/<controller>/verify
        [HttpPost("verify")]
        public async Task<User> Verify([FromBody] User user, string token)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null && !string.IsNullOrEmpty(token))
                {
                    var result = await _identityUserManager.ConfirmEmailAsync(identityuser, token);
                    if (result.Succeeded)
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Email Verified For {Username}", user.Username);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username} - Error {Error}", user.Username, string.Join(" ", result.Errors.ToList().Select(e => e.Description)));
                        user = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username}And Token {Token}", user.Username, token);
                    user = null;
                }
            }
            return user;
        }

        // POST api/<controller>/forgot
        [HttpPost("forgot")]
        public async Task Forgot([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    user = _users.GetUser(user.Username);
                    string token = await _identityUserManager.GeneratePasswordResetTokenAsync(identityuser);
                    string url = HttpContext.Request.Scheme + "://" + _tenantManager.GetAlias().Name + "/reset?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                    string body = "Dear " + user.DisplayName + ",\n\nYou recently requested to reset your password. Please use the link below to complete the process:\n\n" + url +
                        "\n\nPlease note that the link is only valid for 24 hours so if you are unable to take action within that time period, you should initiate another password reset on the site." +
                        "\n\nIf you did not request to reset your password you can safely ignore this message." +
                        "\n\nThank You!";

                    var notification = new Notification(_tenantManager.GetAlias().SiteId, user, "User Password Reset", body);
                    _notifications.AddNotification(notification);
                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "Password Reset Notification Sent For {Username}", user.Username);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Notification Failed For {Username}", user.Username);
                }
            }
        }

        // POST api/<controller>/reset
        [HttpPost("reset")]
        public async Task<User> Reset([FromBody] User user, string token)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null && !string.IsNullOrEmpty(token))
                {
                    var result = await _identityUserManager.ResetPasswordAsync(identityuser, token, user.Password);
                    if (result.Succeeded)
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Password Reset For {Username}", user.Username);
                        user.Password = "";
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Password Reset Failed For {Username} - Error {Error}", user.Username, string.Join(" ", result.Errors.ToList().Select(e => e.Description)));
                        user = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Failed For {Username} And Token {Token}", user.Username, token);
                    user = null;
                }
            }
            return user;
        }

        // POST api/<controller>/twofactor
        [HttpPost("twofactor")]
        public User TwoFactor([FromBody] User user, string token)
        {
            User loginUser = new User { SiteId = user.SiteId, Username = user.Username, IsAuthenticated = false };

            if (ModelState.IsValid && !string.IsNullOrEmpty(token))
            {
                user = _users.GetUser(user.Username);
                if (user != null)
                {
                    if (user.TwoFactorRequired && user.TwoFactorCode == token && DateTime.UtcNow < user.TwoFactorExpiry)
                    {
                        loginUser.IsAuthenticated = true;
                    }
                }
            }

            return loginUser;
        }

        // POST api/<controller>/link
        [HttpPost("link")]
        public async Task<User> Link([FromBody] User user, string token, string type, string key, string name)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null && !string.IsNullOrEmpty(token))
                {
                    var result = await _identityUserManager.ConfirmEmailAsync(identityuser, token);
                    if (result.Succeeded)
                    {
                        // make LoginProvider multi-tenant aware
                        type += ":" + user.SiteId.ToString();
                        await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(type, key, name));
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "External Login Linkage Successful For {Username} And Provider {Provider}", user.Username, type);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "External Login Linkage Failed For {Username} - Error {Error}", user.Username, string.Join(" ", result.Errors.ToList().Select(e => e.Description)));
                        user = null;
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "External Login Linkage Failed For {Username} And Token {Token}", user.Username, token);
                user = null;
            }
            return user;
        }

        // GET api/<controller>/validate/x
        [HttpGet("validate/{password}")]
        public async Task<bool> Validate(string password)
        {
            var validator = new PasswordValidator<IdentityUser>();
            var result = await validator.ValidateAsync(_identityUserManager, null, password);
            return result.Succeeded;
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
                if (User.HasClaim(item => item.Type == ClaimTypes.NameIdentifier))
                {
                    user.UserId = int.Parse(User.Claims.First(item => item.Type == ClaimTypes.NameIdentifier).Value);
                }
                string roles = "";
                foreach (var claim in User.Claims.Where(item => item.Type == ClaimTypes.Role))
                {
                    roles += claim.Value + ";";
                }
                if (roles != "") roles = ";" + roles;
                user.Roles = roles;
            }
            return user;
        }

        private string GetUserRoles(int userId, int siteId)
        {
            string roles = "";
            List<UserRole> userroles = _userRoles.GetUserRoles(userId, siteId).ToList();
            foreach (UserRole userrole in userroles)
            {
                roles += userrole.Role.Name + ";";
                if (userrole.Role.Name == RoleNames.Host && userroles.Where(item => item.Role.Name == RoleNames.Admin).FirstOrDefault() == null)
                {
                    roles += RoleNames.Admin + ";";
                }
            }
            if (roles != "") roles = ";" + roles;
            return roles;
        }
    }
}
