using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public PermissionHandler(IHttpContextAccessor accessor, IUserPermissions userPermissions, ILogManager logger)
        {
            _accessor = accessor;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission is scoped based on entity name and in some cases entity id
            var ctx = _accessor.HttpContext;
            if (ctx != null)
            {
                int siteId = -1;
                if (ctx.GetAlias() != null)
                {
                    siteId = ctx.GetAlias().SiteId;
                }

                int entityId = -1;

                // get entityid from querystring based on a parameter format of auth{entityname}id (ie. authmoduleid ) 
                if (ctx.Request.Query.ContainsKey("auth" + requirement.EntityName.ToLower() + "id"))
                {
                    if (!int.TryParse(ctx.Request.Query["auth" + requirement.EntityName.ToLower() + "id"], out entityId))
                    {
                        entityId = -1;
                    }
                }

                // legacy support for deprecated CreateAuthorizationPolicyUrl(string url, int entityId)
                if (entityId == -1)
                {
                    if (ctx.Request.Query.ContainsKey("entityid"))
                    {
                        if (!int.TryParse(ctx.Request.Query["entityid"], out entityId))
                        {
                            entityId = -1;
                        }
                    }
                }

                // validate permissions
                if (_userPermissions.IsAuthorized(context.User, siteId, requirement.EntityName, entityId, requirement.PermissionName, requirement.Roles))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    if (entityId == -1)
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "User {User} Does Not Have {PermissionName} Permission For {EntityName} Entity", context.User.Identity.Name, requirement.PermissionName, requirement.EntityName);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "User {User} Does Not Have {PermissionName} Permission For {EntityName} Entity With ID {EntityId}", context.User.Identity.Name, requirement.PermissionName, requirement.EntityName, entityId);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
