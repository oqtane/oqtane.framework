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

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository Users;
        private readonly ISiteUserRepository SiteUsers;
        private readonly IRoleRepository Roles;
        private readonly IUserRoleRepository UserRoles;
        private readonly UserManager<IdentityUser> IdentityUserManager;
        private readonly SignInManager<IdentityUser> IdentitySignInManager;

        public UserController(IUserRepository Users, ISiteUserRepository SiteUsers, IRoleRepository Roles, IUserRoleRepository UserRoles, UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager)
        {
            this.Users = Users;
            this.SiteUsers = SiteUsers;
            this.Roles = Roles;
            this.UserRoles = UserRoles;
            this.IdentityUserManager = IdentityUserManager;
            this.IdentitySignInManager = IdentitySignInManager;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<User> Get(string siteid)
        {
            List<User> users = new List<User>();
            IEnumerable<SiteUser> siteusers = SiteUsers.GetSiteUsers(int.Parse(siteid));
            foreach (SiteUser siteuser in siteusers)
            {
                User user = siteuser.User;
                user.SiteId = siteuser.SiteId;
                users.Add(user);
            }
            return users;
        }

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        public User Get(int id, string siteid)
        {
            User user = Users.GetUser(id);
            if (user != null)
            {
                user.SiteId = int.Parse(siteid);
                if (!user.IsHost) // host users are part of every site by default
                {
                    SiteUser siteuser = SiteUsers.GetSiteUser(user.SiteId, id);
                    if (siteuser != null)
                    {
                        user.Roles = GetUserRoles(user.UserId, user.SiteId);
                    }
                }
                else
                {
                    user.Roles = ";" + Constants.HostRole + ";" + Constants.AdminRole + ";";
                }
            }
            return user;
        }

        // GET api/<controller>/name/x?siteid=x
        [HttpGet("name/{name}")]
        public User Get(string name, string siteid)
        {
            User user = Users.GetUser(name);
            if (user != null)
            {
                user.SiteId = int.Parse(siteid);
                if (!user.IsHost) // host users are part of every site by default
                {
                    SiteUser siteuser = SiteUsers.GetSiteUser(user.SiteId, user.UserId);
                    if (siteuser != null)
                    {
                        user.Roles = GetUserRoles(user.UserId, user.SiteId);
                    }
                    else
                    {
                        user = null;
                    }
                }
                else
                {
                    user.Roles = ";" + Constants.HostRole + ";" + Constants.AdminRole + ";";
                }
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
                bool installed = true;
                if (!Users.GetUsers().Any())
                {
                    installed = false; // during initial installation we need to be able to create the host user
                }

                IdentityUser identityuser = await IdentityUserManager.FindByNameAsync(User.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = User.Username;
                    identityuser.Email = User.Username;
                    var result = await IdentityUserManager.CreateAsync(identityuser, User.Password);
                    if (result.Succeeded)
                    {
                        user = Users.AddUser(User);
                        if (!user.IsHost) // host users are part of every site by default
                        {
                            SiteUser siteuser = new SiteUser();
                            siteuser.SiteId = User.SiteId;
                            siteuser.UserId = user.UserId;
                            SiteUsers.AddSiteUser(siteuser);

                            List<Role> roles = Roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned == true).ToList();
                            foreach (Role role in roles)
                            {
                                UserRole userrole = new UserRole();
                                userrole.UserId = user.UserId;
                                userrole.RoleId = role.RoleId;
                                userrole.EffectiveDate = null;
                                userrole.ExpiryDate = null;
                                UserRoles.AddUserRole(userrole);
                            }
                        }
                    }
                }
                else
                {
                    user = Users.GetUser(User.Username);
                    SiteUser siteuser = SiteUsers.GetSiteUser(User.SiteId, user.UserId);
                    if (siteuser == null)
                    {
                        if (!user.IsHost) // host users are part of every site by default
                        {
                            siteuser = new SiteUser();
                            siteuser.SiteId = User.SiteId;
                            siteuser.UserId = user.UserId;
                            SiteUsers.AddSiteUser(siteuser);

                            List<Role> roles = Roles.GetRoles(User.SiteId).Where(item => item.IsAutoAssigned == true).ToList();
                            foreach (Role role in roles)
                            {
                                UserRole userrole = new UserRole();
                                userrole.UserId = user.UserId;
                                userrole.RoleId = role.RoleId;
                                userrole.EffectiveDate = null;
                                userrole.ExpiryDate = null;
                                UserRoles.AddUserRole(userrole);
                            }
                        }
                    }
                }
            }

            return user;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize]
        public User Put(int id, [FromBody] User User)
        {
            if (ModelState.IsValid)
            {
                User = Users.UpdateUser(User);
            }
            return User;
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id, string siteid)
        {
            SiteUser siteuser = SiteUsers.GetSiteUser(id, int.Parse(siteid));
            if (siteuser != null)
            {
                SiteUsers.DeleteSiteUser(siteuser.SiteUserId);
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        public async Task<User> Login([FromBody] User User, bool SetCookie, bool IsPersistent)
        {
            User user = new Models.User { Username = User.Username, IsAuthenticated = false };

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await IdentityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null)
                {
                    var result = await IdentitySignInManager.CheckPasswordSignInAsync(identityuser, User.Password, false);
                    if (result.Succeeded)
                    {
                        user = Users.GetUser(identityuser.UserName);
                        if (user != null)
                        {
                            if (!user.IsHost) // host users are part of every site by default
                            {
                                SiteUser siteuser = SiteUsers.GetSiteUser(User.SiteId, user.UserId);
                                if (siteuser != null)
                                {
                                    user.IsAuthenticated = true;
                                }
                            }
                            else
                            {
                                user.IsAuthenticated = true;
                            }
                            if (user.IsAuthenticated && SetCookie)
                            {
                                await IdentitySignInManager.SignInAsync(identityuser, IsPersistent);
                            }
                        }
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
            IEnumerable<UserRole> userroles = UserRoles.GetUserRoles(UserId, SiteId);
            foreach (UserRole userrole in userroles)
            {
                roles += userrole.Role.Name + ";";
            }
            if (roles != "") roles = ";" + roles;
            return roles;
        }
    }
}
