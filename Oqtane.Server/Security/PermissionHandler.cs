using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;


namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly IUserPermissions UserPermissions;

        public PermissionHandler(IHttpContextAccessor HttpContextAccessor, IUserPermissions UserPermissions)
        {
            this.HttpContextAccessor = HttpContextAccessor;
            this.UserPermissions = UserPermissions;
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
            }
            return Task.CompletedTask;
        }
    }
}