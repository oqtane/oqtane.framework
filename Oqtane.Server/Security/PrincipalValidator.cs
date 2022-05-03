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
                        var claims = context.Principal.Claims;

                        // check if principal has roles and matches current site
                        if (!claims.Any(item => item.Type == ClaimTypes.Role) || claims.FirstOrDefault(item => item.Type == "sitekey")?.Value != alias.SiteKey)
                        {
                            var userRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRepository)) as IUserRepository;
                            var userRoleRepository = context.HttpContext.RequestServices.GetService(typeof(IUserRoleRepository)) as IUserRoleRepository;
                            var _logger = context.HttpContext.RequestServices.GetService(typeof(ILogManager)) as ILogManager;
                            string path = context.Request.Path.ToString().ToLower();

                            User user = userRepository.GetUser(context.Principal.Identity.Name);
                            if (user != null)
                            {
                                // replace principal with roles for current site
                                List<UserRole> userroles = userRoleRepository.GetUserRoles(user.UserId, alias.SiteId).ToList();
                                if (userroles.Any())
                                {
                                    var identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                                    context.ReplacePrincipal(new ClaimsPrincipal(identity));
                                    context.ShouldRenew = true;
                                    Log(_logger, alias, "Permissions Updated For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                                }
                                else
                                {
                                    // user has no roles - remove principal
                                    Log(_logger, alias, "Permissions Removed For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                                    context.RejectPrincipal();
                                }
                            }
                            else
                            {
                                // user does not exist - remove principal
                                Log(_logger, alias, "Permissions Removed For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                                context.RejectPrincipal();
                            }
                        }
                    }
                    else
                    {
                        // user is signed in but tenant cannot be determined
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static void Log (ILogManager logger, Alias alias, string message, string username, string path)
        {
            if (!path.StartsWith("/api/")) // reduce log verbosity
            {
                logger.Log(alias.SiteId, LogLevel.Information, "LoginValidation", Enums.LogFunction.Security, message, username, path);
            }
        }
    }
}
