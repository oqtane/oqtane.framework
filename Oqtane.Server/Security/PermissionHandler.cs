using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public PermissionHandler(IHttpContextAccessor httpContextAccessor, IUserPermissions userPermissions, ILogManager logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission is scoped based on entitynames and ids passed as querystring parameters or headers
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx != null)
            {
                // get entityid based on a parameter format of auth{entityname}id (ie. authmoduleid ) 
                int entityId = -1;
                if (ctx.Request.Query.ContainsKey("auth" + requirement.EntityName.ToLower() + "id"))
                {
                    if (!int.TryParse(ctx.Request.Query["auth" + requirement.EntityName.ToLower() + "id"], out entityId))
                    {
                        entityId = -1;
                    }
                }

                // legacy support
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
                if (entityId != -1 && _userPermissions.IsAuthorized(context.User, requirement.EntityName, entityId, requirement.PermissionName))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "User {User} Does Not Have {PermissionName} Permission For {EntityName}:{EntityId}", context.User, requirement.PermissionName, requirement.EntityName, entityId);
                }
            }
            return Task.CompletedTask;
        }
    }
}
