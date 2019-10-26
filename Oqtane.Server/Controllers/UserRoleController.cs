using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleRepository UserRoles;
        private readonly ILogManager logger;

        public UserRoleController(IUserRoleRepository UserRoles, ILogManager logger)
        {
            this.UserRoles = UserRoles;
            this.logger = logger;
        }

        // GET: api/<controller>?userid=x
        [HttpGet]
        public IEnumerable<UserRole> Get(string siteid)
        {
            if (siteid == "")
            {
                return UserRoles.GetUserRoles();
            }
            else
            {
                return UserRoles.GetUserRoles(int.Parse(siteid));
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
                logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", UserRole);
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
                logger.Log(LogLevel.Information, this, LogFunction.Update, "User Role Updated {UserRole}", UserRole);
            }
            return UserRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            UserRoles.DeleteUserRole(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRoleId}", id);
        }
    }
}
