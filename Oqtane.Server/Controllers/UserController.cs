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
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IUserRoleRepository _userRoles;
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly ITenantResolver _tenants;
        private readonly INotificationRepository _notifications;
        private readonly IFolderRepository _folders;
        private readonly ISyncManager _syncManager;
        private readonly ISiteRepository _sites;
        private readonly ILogManager _logger;

        public UserController(IUserRepository users, IRoleRepository roles, IUserRoleRepository userRoles, UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, ITenantResolver tenants, INotificationRepository notifications, IFolderRepository folders, ISyncManager syncManager, ISiteRepository sites, ILogManager logger)
        {
            _users = users;
            _roles = roles;
            _userRoles = userRoles;
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _tenants = tenants;
            _folders = folders;
            _notifications = notifications;
            _syncManager = syncManager;
            _sites = sites;
            _logger = logger;
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        [Authorize]
        public User Get(int id, string siteid)
        {
            User user = _users.GetUser(id);
            if (user != null)
            {
                user.SiteId = int.Parse(siteid);
                user.Roles = GetUserRoles(user.UserId, user.SiteId);
            }
            return Filter(user);
        }

        // GET api/<controller>/name/x?siteid=x
        [HttpGet("name/{name}")]
        public User Get(string name, string siteid)
        {
            User user = _users.GetUser(name);
            if (user != null)
            {
                user.SiteId = int.Parse(siteid);
                user.Roles = GetUserRoles(user.UserId, user.SiteId);
            }
            return Filter(user);
        }

        private User Filter(User user)
        {
            if (user != null && !User.IsInRole(Constants.AdminRole) && User.Identity.Name?.ToLower() != user.Username.ToLower())
            {
                user.DisplayName = "";
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
                user.Password = "";
                user.IsAuthenticated = false;
            }
            return user;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<User> Post([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                var newUser = await CreateUser(user);
                return newUser;
            }

            return null;
        }

        //TODO shoud be moved to another layer
        private async Task<User> CreateUser(User user)
        {
            User newUser = null;

            bool verified;
            bool allowregistration;
            if (user.Username == Constants.HostUser || User.IsInRole(Constants.AdminRole))
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
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = user.Username;
                    identityuser.Email = user.Email;
                    identityuser.EmailConfirmed = verified;
                    var result = await _identityUserManager.CreateAsync(identityuser, user.Password);
                    if (result.Succeeded)
                    {
                        user.LastLoginOn = null;
                        user.LastIPAddress = "";
                        newUser = _users.AddUser(user);
                        if (!verified)
                        {
                            Notification notification = new Notification();
                            notification.SiteId = user.SiteId;
                            notification.FromUserId = null;
                            notification.ToUserId = newUser.UserId;
                            notification.ToEmail = newUser.Email;
                            notification.Subject = "User Account Verification";
                            string token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(identityuser);
                            string url = HttpContext.Request.Scheme + "://" + _tenants.GetAlias().Name + "/login?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                            notification.Body = "Dear " + user.DisplayName + ",\n\nIn Order To Complete The Registration Of Your User Account Please Click The Link Displayed Below:\n\n" + url + "\n\nThank You!";
                            notification.ParentId = null;
                            notification.CreatedOn = DateTime.UtcNow;
                            notification.IsDelivered = false;
                            notification.DeliveredOn = null;
                            notification.SendOn = DateTime.UtcNow;
                            _notifications.AddNotification(notification);
                        }

                        // assign to host role if this is the host user ( initial installation )
                        if (user.Username == Constants.HostUser)
                        {
                            int hostroleid = _roles.GetRoles(user.SiteId, true).Where(item => item.Name == Constants.HostRole).FirstOrDefault().RoleId;
                            UserRole userrole = new UserRole();
                            userrole.UserId = newUser.UserId;
                            userrole.RoleId = hostroleid;
                            userrole.EffectiveDate = null;
                            userrole.ExpiryDate = null;
                            _userRoles.AddUserRole(userrole);
                        }

                        // add folder for user
                        Folder folder = _folders.GetFolder(user.SiteId, Utilities.PathCombine("Users","\\"));
                        if (folder != null)
                        {
                            _folders.AddFolder(new Folder
                            {
                                SiteId = folder.SiteId,
                                ParentId = folder.FolderId,
                                Name = "My Folder",
                                Path = Utilities.PathCombine(folder.Path, newUser.UserId.ToString(),"\\"),
                                Order = 1,
                                IsSystem = true,
                                Permissions = "[{\"PermissionName\":\"Browse\",\"Permissions\":\"[" + newUser.UserId.ToString() + "]\"},{\"PermissionName\":\"View\",\"Permissions\":\"All Users\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"[" +
                                              newUser.UserId.ToString() + "]\"}]"
                            });
                        }
                    }
                }
                else
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                    if (result.Succeeded)
                    {
                        newUser = _users.GetUser(user.Username);
                    }
                }

                if (newUser != null && user.Username != Constants.HostUser)
                {
                    // add auto assigned roles to user for site
                    List<Role> roles = _roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned).ToList();
                    foreach (Role role in roles)
                    {
                        UserRole userrole = new UserRole();
                        userrole.UserId = newUser.UserId;
                        userrole.RoleId = role.RoleId;
                        userrole.EffectiveDate = null;
                        userrole.ExpiryDate = null;
                        _userRoles.AddUserRole(userrole);
                    }
                }

                if (newUser != null)
                {
                    newUser.Password = ""; // remove sensitive information
                    _logger.Log(user.SiteId, LogLevel.Information, this, LogFunction.Create, "User Added {User}", newUser);
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
            if (ModelState.IsValid)
            {
                if (User.IsInRole(Constants.AdminRole) || User.Identity.Name == user.Username)
                {
                    if (user.Password != "")
                    {
                        IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                        if (identityuser != null)
                        {
                            identityuser.PasswordHash = _identityUserManager.PasswordHasher.HashPassword(identityuser, user.Password);
                            await _identityUserManager.UpdateAsync(identityuser);
                        }
                    }
                    user = _users.UpdateUser(user);
                    _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.User, user.UserId);
                    user.Password = ""; // remove sensitive information
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Updated {User}", user);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update User {User}", user);
                    HttpContext.Response.StatusCode = 401;
                    user = null;
                }
            }
            return user;
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task Delete(int id)
        {
            IdentityUser identityuser = await _identityUserManager.FindByNameAsync(_users.GetUser(id).Username);
            
            if (identityuser != null)
            {
                var result = await _identityUserManager.DeleteAsync(identityuser);

                if (result != null)
                {
                    _users.DeleteUser(id);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Deleted {UserId}", id);
                }
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        public async Task<User> Login([FromBody] User user, bool setCookie, bool isPersistent)
        {
            User loginUser = new User { Username = user.Username, IsAuthenticated = false };

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                    if (result.Succeeded)
                    {
                        loginUser = _users.GetUser(identityuser.UserName);
                        if (loginUser != null)
                        {
                            if (identityuser.EmailConfirmed)
                            {
                                loginUser.IsAuthenticated = true;
                                loginUser.LastLoginOn = DateTime.UtcNow;
                                loginUser.LastIPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
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
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "User Login Failed {Username}", user.Username);
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
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout {Username}", (user != null) ? user.Username : "");
        }

        // POST api/<controller>/verify
        [HttpPost("verify")]
        public async Task<User> Verify([FromBody] User user, string token)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    var result = await _identityUserManager.ConfirmEmailAsync(identityuser, token);
                    if (result.Succeeded)
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Email Verified For {Username}", user.Username);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username}", user.Username);
                        user = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username}", user.Username);
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
                    Notification notification = new Notification();
                    notification.SiteId = user.SiteId;
                    notification.FromUserId = null;
                    notification.ToUserId = user.UserId;
                    notification.ToEmail = "";
                    notification.Subject = "User Password Reset";
                    string token = await _identityUserManager.GeneratePasswordResetTokenAsync(identityuser);
                    string url = HttpContext.Request.Scheme + "://" + _tenants.GetAlias().Name + "/reset?name=" + user.Username + "&token=" + WebUtility.UrlEncode(token);
                    notification.Body = "Dear " + user.DisplayName + ",\n\nPlease Click The Link Displayed Below To Reset Your Password:\n\n" + url + "\n\nThank You!";
                    notification.ParentId = null;
                    notification.CreatedOn = DateTime.UtcNow;
                    notification.IsDelivered = false;
                    notification.DeliveredOn = null;
                    notification.SendOn = DateTime.UtcNow;
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
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Failed For {Username}", user.Username);
                        user = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Failed For {Username}", user.Username);
                    user = null;
                }
            }
            return user;
        }

        // GET api/<controller>/current
        [HttpGet("authenticate")]
        public User Authenticate()
        {
            User user = new User { IsAuthenticated = User.Identity.IsAuthenticated, Username = "", UserId = -1, Roles = "" };            
            if (user.IsAuthenticated)
            {
                user.Username = User.Identity.Name;
                user.UserId = int.Parse(User.Claims.First(item => item.Type == ClaimTypes.PrimarySid).Value);
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
                if (userrole.Role.Name == Constants.HostRole && userroles.Where(item => item.Role.Name == Constants.AdminRole).FirstOrDefault() == null)
                {
                    roles += Constants.AdminRole + ";";
                }
            }
            if (roles != "") roles = ";" + roles;
            return roles;
        }
    }
}
