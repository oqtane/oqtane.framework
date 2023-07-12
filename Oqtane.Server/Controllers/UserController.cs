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
using Oqtane.Security;
using Oqtane.Extensions;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class UserController : Controller
    {
        private readonly IUserRepository _users;
        private readonly UserManager<IdentityUser> _identityUserManager;
        private readonly SignInManager<IdentityUser> _identitySignInManager;
        private readonly ITenantManager _tenantManager;
        private readonly INotificationRepository _notifications;
        private readonly IUserManager _userManager;
        private readonly ISiteRepository _sites;
        private readonly IUserPermissions _userPermissions;
        private readonly IJwtManager _jwtManager;
        private readonly ILogManager _logger;

        public UserController(IUserRepository users, UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, ITenantManager tenantManager, INotificationRepository notifications, IUserManager userManager, ISiteRepository sites, IUserPermissions userPermissions, IJwtManager jwtManager, ILogManager logger)
        {
            _users = users;
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _tenantManager = tenantManager;
            _notifications = notifications;
            _userManager = userManager;
            _sites = sites;
            _userPermissions = userPermissions;
            _jwtManager = jwtManager;
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

        // GET api/<controller>/name/x?siteid=x
        [HttpGet("name/{name}")]
        public User Get(string name, string siteid)
        {
            if (int.TryParse(siteid, out int SiteId) && SiteId == _tenantManager.GetAlias().SiteId)
            {
                User user = _userManager.GetUser(name, SiteId);
                if (user == null)
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
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
                bool allowregistration;
                if (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin))
                {
                    user.EmailConfirmed = true;
                    allowregistration = true;
                }
                else
                {
                    user.EmailConfirmed = false;
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
            if (ModelState.IsValid && user.SiteId == _tenantManager.GetAlias().SiteId && _users.GetUser(user.UserId, false) != null
                && (_userPermissions.IsAuthorized(User, user.SiteId, EntityNames.User, -1, PermissionNames.Write, RoleNames.Admin) || User.Identity.Name == user.Username))
            {
                user = await _userManager.UpdateUser(user);
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

        // POST api/<controller>/login
        [HttpPost("login")]
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
            await HttpContext.SignOutAsync(Constants.AuthenticationScheme);
            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Logout {Username}", (user != null) ? user.Username : "");
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

        // POST api/<controller>/forgot
        [HttpPost("forgot")]
        public async Task Forgot([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                await _userManager.ForgotPassword(user);
            }
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

        // POST api/<controller>/link
        [HttpPost("link")]
        public async Task<User> Link([FromBody] User user, string token, string type, string key, string name)
        {
            if (ModelState.IsValid)
            {
                user = await _userManager.LinkExternalAccount(user, token, type, key, name);
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
    }
}
