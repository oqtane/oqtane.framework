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
        public async Task<User> Post([FromBody] User User)
        {
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
                        User = users.AddUser(User);
                    }
                }
            }
            return User;
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
        public async Task<User> Login([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityuser = await identityUserManager.FindByNameAsync(user.Username);
                if (identityuser != null)
                {
                    var result = await identitySignInManager.CheckPasswordSignInAsync(identityuser, user.Password, false);
                    if (result.Succeeded)
                    {
                        await identitySignInManager.SignInAsync(identityuser, user.IsPersistent);
                        user = users.GetUser(identityuser.UserName);
                        user.IsAuthenticated = true;
                    }
                    else
                    {
                        user = new Models.User { Username = user.Username, IsAuthenticated = false };
                    }
                }
                else
                {
                    user = new Models.User { Username = user.Username, IsAuthenticated = false };
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
