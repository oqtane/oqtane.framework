using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
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
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<Role> Get(string siteid)
        {
            return _roles.GetRoles(int.Parse(siteid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Role Get(int id)
        {
            return _roles.GetRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public IActionResult Post([FromBody] Role role)
        {
            if (ModelState.IsValid)
            {
                role.Description = role.Description ?? string.Empty;
                role = _roles.AddRole(role);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Role Added {Role}", role);
                return Ok(role);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public IActionResult Put(int id, [FromBody] Role role)
        {
            if (ModelState.IsValid)
            {
                role.Description = role.Description ?? string.Empty;
                role = _roles.UpdateRole(role);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Role Updated {Role}", role);
                return Ok(role);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _roles.DeleteRole(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Role Deleted {RoleId}", id);
        }
    }
}
