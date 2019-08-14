using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository users;
        private readonly ISiteUserRepository siteusers;
        private readonly UserManager<IdentityUser> identityUserManager;
        private readonly SignInManager<IdentityUser> identitySignInManager;

        public UserController(IUserRepository Users, ISiteUserRepository SiteUsers, UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager)
        {
            users = Users;
            siteusers = SiteUsers;
            identityUserManager = IdentityUserManager;
            identitySignInManager = IdentitySignInManager;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return users.GetUsers();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public User Get(int id)
        {
            return users.GetUser(id);
        }
        
        // POST api/<controller>
        [HttpPost]
        public async Task<User> Post([FromBody] User User)
        {
            User user = null;

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await identityUserManager.FindByNameAsync(User.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = User.Username;
                    identityuser.Email = User.Username;
                    var result = await identityUserManager.CreateAsync(identityuser, User.Password);
                    if (result.Succeeded)
                    {
                        user = users.AddUser(User);
                        SiteUser SiteUser = new SiteUser();
                        SiteUser.SiteId = User.SiteId;
                        SiteUser.UserId = user.UserId;
                        SiteUser.IsAuthorized = true;
                        siteusers.AddSiteUser(SiteUser);
                    }
                }
            }

            return user;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public User Put(int id, [FromBody] User User)
        {
            if (ModelState.IsValid)
            {
                User = users.UpdateUser(User);
            }
            return User;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            users.DeleteUser(id);
        }

        // GET api/<controller>/name/x
        [HttpGet("name/{name}")]
        public User GetByName(string name)
        {
            return users.GetUser(name);
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        public async Task<User> Login([FromBody] User User)
        {
            User user = new Models.User { Username = User.Username, IsAuthenticated = false };

            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await identityUserManager.FindByNameAsync(User.Username);
                if (identityuser != null)
                {
                    var result = await identitySignInManager.CheckPasswordSignInAsync(identityuser, User.Password, false);
                    if (result.Succeeded)
                    {
                        user = users.GetUser(identityuser.UserName);
                        if (user != null)
                        {
                            SiteUser siteuser = siteusers.GetSiteUsers(User.SiteId, user.UserId).FirstOrDefault();
                            if (siteuser.IsAuthorized)
                            {
                                await identitySignInManager.SignInAsync(identityuser, User.IsPersistent);
                                user.IsAuthenticated = true;
                            }
                        }
                    }
                }
            }

            return user;
        }

        // POST api/<controller>/logout
        [HttpPost("logout")]
        public async Task Logout([FromBody] User User)
        {
            await identitySignInManager.SignOutAsync();
        }

        // GET api/<controller>/current
        [HttpGet("authenticate")]
        public User Authenticate()
        {
            return new User { Username = User.Identity.Name, IsAuthenticated = User.Identity.IsAuthenticated };
        }
    }
}
