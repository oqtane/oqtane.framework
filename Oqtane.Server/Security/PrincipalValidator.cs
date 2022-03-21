using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Security
{
    public static class PrincipalValidator
    {
        public static Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context != null && context.Principal.Identity.IsAuthenticated)
            {
                // check if framework is installed
                var config = context.HttpContext.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
                if (config.IsInstalled())
                {
                    var tenantManager = context.HttpContext.RequestServices.GetService(typeof(ITenantManager)) as ITenantManager;
                    var alias = tenantManager.GetAlias();
                    if (alias != null)
                    {
                        // verify principal was authenticated for current tenant
                        if (context.Principal.Claims.FirstOrDefault(item => item.Type == ClaimTypes.GroupSid)?.Value != alias.SiteKey)
                        {
                            // tenant agnostic requests must be ignored 
                            string path = context.Request.Path.ToString().ToLower();
                            if (path.StartsWith("/_blazor") || path.StartsWith("/api/installation/"))
                            {
                                return Task.CompletedTask;
                            }

                            // refresh principal
                            var userRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRepository)) as IUserRepository;
                            var userRoleRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRoleRepository)) as IUserRoleRepository;

                            if (context.Principal.Identity.Name != null)
                            {
                                User user = userRepository.GetUser(context.Principal.Identity.Name);
                                if (user != null)
                                {
                                    List<UserRole> userroles = userRoleRepository.GetUserRoles(user.UserId, alias.SiteId).ToList();
                                    var identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                                    context.ReplacePrincipal(new ClaimsPrincipal(identity));
                                    context.ShouldRenew = true;
                                }
                                else
                                {
                                    context.RejectPrincipal();
                                }
                            }
                        }
                    }
                    else
                    {
                        context.RejectPrincipal();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
