using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roles;
        private readonly ILogManager _logger;

        public RoleController(IRoleRepository roles, ILogManager logger)
        {
            _roles = roles;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Role> Get(string siteid)
        {
            return _roles.GetRoles(int.Parse(siteid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Role Get(int id)
        {
            return _roles.GetRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public Role Post([FromBody] Role role)
        {
            if (ModelState.IsValid)
            {
                role = _roles.AddRole(role);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Role Added {Role}", role);
            }
            return role;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Role Put(int id, [FromBody] Role role)
        {
            if (ModelState.IsValid)
            {
                role = _roles.UpdateRole(role);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Role Updated {Role}", role);
            }
            return role;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            _roles.DeleteRole(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Role Deleted {RoleId}", id);
        }
    }
}
