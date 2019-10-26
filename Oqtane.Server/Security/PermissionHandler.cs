using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public PermissionHandler(IHttpContextAccessor HttpContextAccessor, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.HttpContextAccessor = HttpContextAccessor;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission is scoped based on EntityId which must be passed as a querystring parameter
            var ctx = HttpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Query.ContainsKey("entityid"))
            {
                int EntityId = int.Parse(ctx.Request.Query["entityid"]);
                if (UserPermissions.IsAuthorized(context.User, requirement.EntityName, EntityId, requirement.PermissionName))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    logger.Log(LogLevel.Error, this, LogFunction.Security, "User {User} Does Not Have {PermissionName} Permission For {EntityName}:{EntityId}", context.User, requirement.PermissionName, requirement.EntityName, EntityId);
                }
            }
            return Task.CompletedTask;
        }
    }
}