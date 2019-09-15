using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleRepository UserRoles;

        public UserRoleController(IUserRoleRepository UserRoles)
        {
            this.UserRoles = UserRoles;
        }

        // GET: api/<controller>?userid=x
        [HttpGet]
        public IEnumerable<UserRole> Get(string userid)
        {
            if (userid == "")
            {
                return UserRoles.GetUserRoles();
            }
            else
            {
                return UserRoles.GetUserRoles(int.Parse(userid));
            }
        }
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        public UserRole Get(int id)
        {
            return UserRoles.GetUserRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Post([FromBody] UserRole UserRole)
        {
            if (ModelState.IsValid)
            {
                UserRole = UserRoles.AddUserRole(UserRole);
            }
            return UserRole;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Put(int id, [FromBody] UserRole UserRole)
        {
            if (ModelState.IsValid)
            {
                UserRole = UserRoles.UpdateUserRole(UserRole);
            }
            return UserRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            UserRoles.DeleteUserRole(id);
        }
    }
}
