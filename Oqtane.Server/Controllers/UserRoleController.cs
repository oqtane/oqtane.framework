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
        private readonly IUserRoleRepository _userRoles;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public UserRoleController(IUserRoleRepository userRoles, ISyncManager syncManager, ILogManager logger)
        {
            _userRoles = userRoles;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET: api/<controller>?userid=x
        [HttpGet]
        [Authorize]
        public IEnumerable<UserRole> Get(string siteid)
        {
            return _userRoles.GetUserRoles(int.Parse(siteid));
        }
        
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize]
        public UserRole Get(int id)
        {
            return _userRoles.GetUserRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public UserRole Post([FromBody] UserRole UserRole)
        {
            if (ModelState.IsValid)
            {
                UserRole = _userRoles.AddUserRole(UserRole);
                _syncManager.AddSyncEvent("User", UserRole.UserId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "User Role Added {UserRole}", UserRole);
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
                UserRole = _userRoles.UpdateUserRole(UserRole);
                _syncManager.AddSyncEvent("User", UserRole.UserId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User Role Updated {UserRole}", UserRole);
            }
            return UserRole;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            UserRole userRole = _userRoles.GetUserRole(id);
            _userRoles.DeleteUserRole(id);
            _syncManager.AddSyncEvent("User", userRole.UserId);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "User Role Deleted {UserRole}", userRole);
        }
    }
}
