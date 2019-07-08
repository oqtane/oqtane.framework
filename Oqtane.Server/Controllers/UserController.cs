using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository users;
        private readonly UserManager<IdentityUser> identityUserManager;
        private readonly SignInManager<IdentityUser> identitySignInManager;

        public UserController(IUserRepository Users, UserManager<IdentityUser> IdentityUserManager, SignInManager<IdentityUser> IdentitySignInManager)
        {
            users = Users;
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
        public async Task Post([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await identityUserManager.FindByNameAsync(user.Username);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = user.Username;
                    identityuser.Email = user.Username;
                    var result = await identityUserManager.CreateAsync(identityuser, user.Password);
                    if (result.Succeeded)
                    {
                        users.AddUser(user);
                    }
                }
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                users.UpdateUser(user);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            users.DeleteUser(id);
        }

        // GET api/<controller>/current
        [HttpGet("current")]
        public User Current()
        {
            User user = null;
            if (User.Identity.IsAuthenticated) 
            {
                user = users.GetUser(User.Identity.Name);
                user.IsAuthenticated = true;
            }
            return user;
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        public async Task<User> Login([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                // seed host user - this logic should be moved to installation
                IdentityUser identityuser = await identityUserManager.FindByNameAsync("host");
                if (identityuser == null)
                {
                    var result = await identityUserManager.CreateAsync(new IdentityUser { UserName = "host", Email = "host" }, "password");
                    if (result.Succeeded)
                    {
                        users.AddUser(new Models.User { Username = "host", DisplayName = "host", IsSuperUser = true, Roles = "" });
                    }
                }

                identityuser = await identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    var result = await identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                    if (result.Succeeded)
                    {
                        await identitySignInManager.SignInAsync(identityuser, false);
                        user = users.GetUser(identityuser.UserName);
                        user.IsAuthenticated = true;
                    }
                    else
                    {
                        user = null;
                    }
                }
                else
                {
                    user = null;
                }
            }
            return user;
        }

        // POST api/<controller>/logout
        [HttpPost("logout")]
        public async Task Logout([FromBody] User user)
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
