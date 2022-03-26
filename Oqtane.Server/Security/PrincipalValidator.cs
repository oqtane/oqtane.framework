using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Extensions;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public static class PrincipalValidator
    {
        public static Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context != null && context.Principal.Identity.IsAuthenticated && context.Principal.Identity.Name != null)
            {
                // check if framework is installed
                var config = context.HttpContext.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
                if (config.IsInstalled())
                {
                    // get current site
                    var alias = context.HttpContext.GetAlias();
                    if (alias != null)
                    {
                        // check if principal matches current site
                        if (context.Principal.Claims.FirstOrDefault(item => item.Type == ClaimTypes.GroupSid)?.Value != alias.SiteKey)
                        {
                            // principal does not match site
                            var userRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRepository)) as IUserRepository;
                            var userRoleRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRoleRepository)) as IUserRoleRepository;
                            var _logger = context.HttpContext.RequestServices.GetService(typeof(ILogManager)) as ILogManager;
                            string path = context.Request.Path.ToString().ToLower();

                            User user = userRepository.GetUser(context.Principal.Identity.Name);
                            if (user != null)
                            {
                                // replace principal with roles for current site
                                List<UserRole> userroles = userRoleRepository.GetUserRoles(user.UserId, alias.SiteId).ToList();
                                var identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                                context.ReplacePrincipal(new ClaimsPrincipal(identity));
                                context.ShouldRenew = true;
                                if (!path.StartsWith("/api/")) // reduce log verbosity
                                {
                                    _logger.Log(alias.SiteId, LogLevel.Information, "LoginValidation", Enums.LogFunction.Security, "Permissions Updated For User {Username} Accessing Resource {Url}", context.Principal.Identity.Name, path);
                                }
                            }
                            else
                            {
                                // user has no roles for site - remove principal
                                context.RejectPrincipal();
                                if (!path.StartsWith("/api/")) // reduce log verbosity
                                {
                                    _logger.Log(alias.SiteId, LogLevel.Information, "LoginValidation", Enums.LogFunction.Security, "Permissions Removed For User {Username} Accessing Resource {Url}", context.Principal.Identity.Name, path);
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
