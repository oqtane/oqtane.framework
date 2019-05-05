using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository users;

        public UserController(IUserRepository Users)
        {
            users = Users;
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
        public void Post([FromBody] User user)
        {
            if (ModelState.IsValid)
                users.AddUser(user);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] User user)
        {
            if (ModelState.IsValid)
                users.UpdateUser(user);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            users.DeleteUser(id);
        }
    }
}
