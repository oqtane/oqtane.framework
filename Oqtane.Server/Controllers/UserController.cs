using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
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
        private readonly ILogManager _logger;

        public UserController(IUserRepository Users, IRoleRepository Roles, IUserRoleRepository UserRoles, UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager, ITenantResolver Tenants, INotificationRepository Notifications, IFolderRepository Folders, ILogManager logger)
        {
            this._users = Users;
            this._roles = Roles;
            this._userRoles = UserRoles;
            this._identityUserManager = IdentityUserManager;
            this._identitySignInManager = IdentitySignInManager;
            this._tenants = Tenants;
            this._folders = Folders;
            this._notifications = Notifications;
            this._logger = logger;
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
            return user;
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
            return user;
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<User> Post([FromBody] User User)
        {
            User user = null;

            if (ModelState.IsValid)
            {
                bool verified = true;
                // users created by non-administrators must be verified
                if (!base.User.IsInRole(Constants.AdminRole) && User.Username != Constants.HostUser)
                {
                    verified = false;
                }

                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = User.Username;
                    identityuser.Email = User.Email;
                    identityuser.EmailConfirmed = verified;
                    var result = await _identityUserManager.CreateAsync(identityuser, User.Password);
                    if (result.Succeeded)
                    {
                        User.LastLoginOn = null;
                        User.LastIPAddress = "";
                        user = _users.AddUser(User);
                        if (!verified)
                        {
                            Notification notification = new Notification();
                            notification.SiteId = User.SiteId;
                            notification.FromUserId = null;
                            notification.ToUserId = user.UserId;
                            notification.ToEmail = "";
                            notification.Subject = "User Account Verification";
                            string token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(identityuser);
                            string url = HttpContext.Request.Scheme + "://" + _tenants.GetAlias().Name + "/login?name=" + User.Username + "&token=" + WebUtility.UrlEncode(token);
                            notification.Body = "Dear " + User.DisplayName + ",\n\nIn Order To Complete The Registration Of Your User Account Please Click The Link Displayed Below:\n\n" + url + "\n\nThank You!";
                            notification.ParentId = null;
                            notification.CreatedOn = DateTime.Now;
                            notification.IsDelivered = false;
                            notification.DeliveredOn = null;
                            _notifications.AddNotification(notification);
                        }

                        // assign to host role if this is the host user ( initial installation )
                        if (User.Username == Constants.HostUser)
                        {
                            int hostroleid = _roles.GetRoles(User.SiteId, true).Where(item => item.Name == Constants.HostRole).FirstOrDefault().RoleId;
                            UserRole userrole = new UserRole();
                            userrole.UserId = user.UserId;
                            userrole.RoleId = hostroleid;
                            userrole.EffectiveDate = null;
                            userrole.ExpiryDate = null;
                            _userRoles.AddUserRole(userrole);
                        }

                        // add folder for user
                        Folder folder = _folders.GetFolder(User.SiteId, "Users\\");
                        if (folder != null)
                        {
                            _folders.AddFolder(new Folder { SiteId = folder.SiteId, ParentId = folder.FolderId, Name = "My Folder", Path = folder.Path + user.UserId.ToString() + "\\", Order = 1, IsSystem = true, 
                                Permissions = "[{\"PermissionName\":\"Browse\",\"Permissions\":\"[" + user.UserId.ToString() + "]\"},{\"PermissionName\":\"View\",\"Permissions\":\"All Users\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"[" + user.UserId.ToString() + "]\"}]" });
                        }
                    }
                }
                else
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, User.Password, false);
                    if (result.Succeeded)
                    {
                        user = _users.GetUser(User.Username);
                    }
                }

                if (user != null && User.Username != Constants.HostUser)
                {
                    // add auto assigned roles to user for site
                    List<Role> roles = _roles.GetRoles(User.SiteId).Where(item => item.IsAutoAssigned == true).ToList();
                    foreach (Role role in roles)
                    {
                        UserRole userrole = new UserRole();
                        userrole.UserId = user.UserId;
                        userrole.RoleId = role.RoleId;
                        userrole.EffectiveDate = null;
                        userrole.ExpiryDate = null;
                        _userRoles.AddUserRole(userrole);
                    }
                }
                user.Password = ""; // remove sensitive information
                _logger.Log(User.SiteId, LogLevel.Information, this, LogFunction.Create, "User Added {User}", user);
            }

            return user;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<User> Put(int id, [FromBody] User User)
        {
            if (ModelState.IsValid)
            {
                if (base.User.IsInRole(Constants.AdminRole) || base.User.Identity.Name == User.Username)
                {
                    if (User.Password != "")
                    {
                        IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                        if (identityuser != null)
                        {
                            identityuser.PasswordHash = _identityUserManager.PasswordHasher.HashPassword(identityuser, User.Password);
                            await _identityUserManager.UpdateAsync(identityuser);
                        }
                    }
                    User = _users.UpdateUser(User);
                    User.Password = ""; // remove sensitive information
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Updated {User}", User);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update User {User}", User);
                    HttpContext.Response.StatusCode = 401;
                    User = null;
                }
            }
            return User;
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
        public async Task<User> Login([FromBody] User User, bool SetCookie, bool IsPersistent)
        {
            User user = new Models.User { Username = User.Username, IsAuthenticated = false };

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null)
                {
                    var result = await _identitySignInManager.CheckPasswordSignInAsync(identityuser, User.Password, false);
                    if (result.Succeeded)
                    {
                        user = _users.GetUser(identityuser.UserName);
                        if (user != null)
                        {
                            if (identityuser.EmailConfirmed)
                            {
                                user.IsAuthenticated = true;
                                user.LastLoginOn = DateTime.Now;
                                user.LastIPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                                _users.UpdateUser(user);
                                _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Login Successful {Username}", User.Username);
                                if (SetCookie)
                                {
                                    await _identitySignInManager.SignInAsync(identityuser, IsPersistent);
                                }
                            }
                            else
                            {
                                _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Not Verified {Username}", User.Username);
                            }
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "User Login Failed {Username}", User.Username);
                    }
                }
            }

            return user;
        }

        // POST api/<controller>/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task Logout([FromBody] User User)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout {Username}", User.Username);
        }

        // POST api/<controller>/verify
        [HttpPost("verify")]
        public async Task<User> Verify([FromBody] User User, string token)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null)
                {
                    var result = await _identityUserManager.ConfirmEmailAsync(identityuser, token);
                    if (result.Succeeded)
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Email Verified For {Username}", User.Username);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username}", User.Username);
                        User = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Email Verification Failed For {Username}", User.Username);
                    User = null;
                }
            }
            return User;
        }
        
        // POST api/<controller>/forgot
        [HttpPost("forgot")]
        public async Task Forgot([FromBody] User User)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null)
                {
                    Notification notification = new Notification();
                    notification.SiteId = User.SiteId;
                    notification.FromUserId = null;
                    notification.ToUserId = User.UserId;
                    notification.ToEmail = "";
                    notification.Subject = "User Password Reset";
                    string token = await _identityUserManager.GeneratePasswordResetTokenAsync(identityuser);
                    string url = HttpContext.Request.Scheme + "://" + _tenants.GetAlias().Name + "/reset?name=" + User.Username + "&token=" + WebUtility.UrlEncode(token);
                    notification.Body = "Dear " + User.DisplayName + ",\n\nPlease Click The Link Displayed Below To Reset Your Password:\n\n" + url + "\n\nThank You!";
                    notification.ParentId = null;
                    notification.CreatedOn = DateTime.Now;
                    notification.IsDelivered = false;
                    notification.DeliveredOn = null;
                    _notifications.AddNotification(notification);
                    _logger.Log(LogLevel.Information, this, LogFunction.Security, "Password Reset Notification Sent For {Username}", User.Username);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Notification Failed For {Username}", User.Username);
                }
            }
        }

        // POST api/<controller>/reset
        [HttpPost("reset")]
        public async Task<User> Reset([FromBody] User User, string token)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null && !string.IsNullOrEmpty(token))
                {
                    var result = await _identityUserManager.ResetPasswordAsync(identityuser, token, User.Password);
                    if (result.Succeeded)
                    {
                        _logger.Log(LogLevel.Information, this, LogFunction.Security, "Password Reset For {Username}", User.Username);
                        User.Password = "";
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Failed For {Username}", User.Username);
                        User = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Password Reset Failed For {Username}", User.Username);
                    User = null;
                }
            }
            return User;
        }

        // GET api/<controller>/current
        [HttpGet("authenticate")]
        public User Authenticate()
        {
            User user = new User();
            user.Username = User.Identity.Name;
            user.IsAuthenticated = User.Identity.IsAuthenticated;
            string roles = "";
            foreach (var claim in User.Claims.Where(item => item.Type == ClaimTypes.Role))
            {
                roles += claim.Value + ";";
            }
            if (roles != "") roles = ";" + roles;
            user.Roles = roles;
            return user;
        }

        private string GetUserRoles(int UserId, int SiteId)
        {
            string roles = "";
            List<UserRole> userroles = _userRoles.GetUserRoles(UserId, SiteId).ToList();
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
