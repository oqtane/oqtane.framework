using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Enums;
using System.Net;
using Oqtane.Repository;
using Oqtane.Extensions;
using System.Reflection;
using System;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ApiController : Controller
    {
        private readonly IPermissionRepository _permissions;
        private readonly IRoleRepository _roles;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ApiController(IPermissionRepository permissions, IRoleRepository roles, ILogManager logger, ITenantManager tenantManager)
        {
            _permissions = permissions;
            _roles = roles;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public List<Api> Get(string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                var apis = new List<Api>();

                var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
                foreach (var assembly in assemblies)
                {
                    // iterate controllers
                    foreach (var type in assembly.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)))
                    {
                        // iterate controller methods with authorize attribute
                        var actions = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => m.GetCustomAttributes<AuthorizeAttribute>().Any());
                        foreach(var action in actions)
                        {
                            // get policy
                            var policy = action.GetCustomAttribute<AuthorizeAttribute>().Policy;
                            if (!string.IsNullOrEmpty(policy) && policy.Contains(":") && !policy.Contains(Constants.RequireEntityId))
                            {
                                // parse policy
                                var segments = policy.Split(':');
                                if (!apis.Any(item => item.EntityName == segments[0]))
                                {
                                    apis.Add(new Api { SiteId = SiteId, EntityName = segments[0], Permissions = segments[1] });
                                }
                                else
                                {
                                    // concatenate permissions
                                    var permissions = apis.SingleOrDefault(item => item.EntityName == segments[0]).Permissions;
                                    if (!permissions.Split(',').Contains(segments[1]))
                                    {
                                        apis.SingleOrDefault(item => item.EntityName == segments[0]).Permissions += "," + segments[1];
                                    }
                                }
                            }
                        }
                    }
                }

                return apis;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Api Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET: api/<controller>/1/user
        [HttpGet("{siteid}/{entityname}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Api Get(int siteid, string entityname)
        {
            if (siteid == _alias.SiteId)
            {
                var permissions = _permissions.GetPermissions(siteid, entityname);
                if (permissions == null || permissions.ToList().Count == 0)
                {
                    permissions = GetPermissions(siteid, entityname);
                }
                return new Api { SiteId = siteid, EntityName = entityname, Permissions = permissions.EncodePermissions() };
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Api Get Attempt {SiteId} {EntityName}", siteid, entityname);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST: api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public void Post([FromBody] Api api)
        {
            if (ModelState.IsValid && api.SiteId == _alias.SiteId)
            {
                _permissions.UpdatePermissions(api.SiteId, api.EntityName, -1, api.Permissions);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Api Updated {Api}", api);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Api Post Attempt {Api}", api);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private List<Permission> GetPermissions(int siteid, string entityname)
        {
            var permissions = new List<Permission>();

            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // iterate controllers
                foreach (var type in assembly.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)))
                {
                    // iterate controller methods with authorize attribute
                    var actions = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.GetCustomAttributes<AuthorizeAttribute>().Any());
                    foreach (var action in actions)
                    {
                        // get policy
                        var policy = action.GetCustomAttribute<AuthorizeAttribute>().Policy;
                        if (!string.IsNullOrEmpty(policy) && policy.Contains(":") && !policy.Contains(Constants.RequireEntityId))
                        {
                            // parse policy
                            var segments = policy.Split(':');
                            // entity match
                            if (segments[0] == entityname && segments.Length > 2)
                            {
                                var roles = _roles.GetRoles(siteid);
                                foreach (var rolename in (segments[2]).Split(','))
                                {
                                    var role = roles.FirstOrDefault(item => item.Name == rolename);
                                    if (role != null)
                                    {
                                        if (!permissions.Any(item => item.EntityName == entityname && item.PermissionName == segments[1] && item.RoleId == role.RoleId))
                                        {
                                            permissions.Add(new Permission { SiteId = siteid, EntityName = entityname, EntityId = -1, PermissionName = segments[1], RoleId = role.RoleId, Role = role, UserId = null, IsAuthorized = true });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return permissions;
        }
    }
}
