using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public PermissionHandler(IHttpContextAccessor HttpContextAccessor, IUserPermissions UserPermissions, ILogManager logger)
        {
            this._httpContextAccessor = HttpContextAccessor;
            this._userPermissions = UserPermissions;
            this._logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission is scoped based on EntityId which must be passed as a querystring parameter
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Query.ContainsKey("entityid"))
            {
                int EntityId = int.Parse(ctx.Request.Query["entityid"]);
                if (_userPermissions.IsAuthorized(context.User, requirement.EntityName, EntityId, requirement.PermissionName))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "User {User} Does Not Have {PermissionName} Permission For {EntityName}:{EntityId}", context.User, requirement.PermissionName, requirement.EntityName, EntityId);
                }
            }
            return Task.CompletedTask;
        }
    }
}