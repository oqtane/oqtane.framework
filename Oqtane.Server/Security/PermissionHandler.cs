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
            // permission is scoped based on EntityId which must be passed as a querystring parameter
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Query.ContainsKey("entityid"))
            {
                int entityId = int.Parse(ctx.Request.Query["entityid"]);
                if (_userPermissions.IsAuthorized(context.User, requirement.EntityName, entityId, requirement.PermissionName))
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