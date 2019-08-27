using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly IPermissionRepository Permissions;

        public PermissionHandler(IHttpContextAccessor HttpContextAccessor, IPermissionRepository Permissions)
        {
            this.HttpContextAccessor = HttpContextAccessor;
            this.Permissions = Permissions;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission is scoped based on EntityId which must be passed as a querystring parameter
            var ctx = HttpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Query.ContainsKey("entityid"))
            {
                int EntityId = int.Parse(ctx.Request.Query["entityid"]);
                string permissions = Permissions.EncodePermissions(EntityId, Permissions.GetPermissions(requirement.EntityName, EntityId, requirement.PermissionName).ToList());

                User user = new User();
                user.UserId = -1;
                user.Roles = "";

                if (context.User != null)
                {
                    var idclaim = context.User.Claims.Where(item => item.Type == ClaimTypes.PrimarySid).FirstOrDefault();
                    if (idclaim != null)
                    {
                        user.UserId = int.Parse(idclaim.Value);
                        foreach (var claim in context.User.Claims.Where(item => item.Type == ClaimTypes.Role))
                        {
                            user.Roles += claim.Value + ";";
                        }
                        if (user.Roles != "") user.Roles = ";" + user.Roles;
                    }
                }

                if (UserSecurity.IsAuthorized(user, requirement.PermissionName, permissions))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}