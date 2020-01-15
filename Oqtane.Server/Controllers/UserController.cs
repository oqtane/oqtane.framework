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
        private readonly ILogManager _logger;

        public UserController(IUserRepository users, IRoleRepository roles, IUserRoleRepository userRoles, UserManager<IdentityUser> identityUserManager, SignInManager<IdentityUser> identitySignInManager, ILogManager logger)
        {
            _users = users;
            _roles = roles;
            _userRoles = userRoles;
            _identityUserManager = identityUserManager;
            _identitySignInManager = identitySignInManager;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return _users.GetUsers();
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
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
                int hostroleid = -1;
                if (!_users.GetUsers().Any())
                {
                    hostroleid = _roles.GetAll(User.SiteId, true).Where(item => item.Name == Constants.HostRole).FirstOrDefault().RoleId;
                }

                IdentityUser identityuser = await _identityUserManager.FindByNameAsync(User.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = User.Username;
                    identityuser.Email = User.Email;
                    var result = await _identityUserManager.CreateAsync(identityuser, User.Password);
                    if (result.Succeeded)
                    {
                        user = _users.AddUser(User);

                        // assign to host role if this is the initial installation
                        if (hostroleid != -1)
                        {
                            UserRole userrole = new UserRole();
                            userrole.UserId = user.UserId;
                            userrole.RoleId = hostroleid;
                            userrole.EffectiveDate = null;
                            userrole.ExpiryDate = null;
                            _userRoles.AddUserRole(userrole);
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

                if (user != null && hostroleid == -1)
                {
                    // add auto assigned roles to user for site
                    List<Role> roles = _roles.GetAll(User.SiteId).Where(item => item.IsAutoAssigned == true).ToList();
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
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Added {User}", user);
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
                            user.IsAuthenticated = true;
                            _logger.Log(LogLevel.Information, this, LogFunction.Security, "User Login Successful {Username}", User.Username);
                            if (SetCookie)
                            {
                                await _identitySignInManager.SignInAsync(identityuser, IsPersistent);
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
