using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Extensions;
using Oqtane.Shared;
using Oqtane.Managers;
using Microsoft.AspNetCore.Authentication;

namespace Oqtane.Security
{
    public static class PrincipalValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context != null && context.Principal.Identity.IsAuthenticated && context.Principal.Identity.Name != null)
            {
                var config = context.HttpContext.RequestServices.GetService(typeof(IConfigManager)) as IConfigManager;
                string path = context.Request.Path.ToString().ToLower();

                // check if framework is installed
                if (config.IsInstalled() && !path.StartsWith("/_")) // ignore Blazor framework requests
                {
                    var _logger = context.HttpContext.RequestServices.GetService(typeof(ILogManager)) as ILogManager;

                    var alias = context.HttpContext.GetAlias();
                    if (alias != null)
                    {
                        var userManager = context.HttpContext.RequestServices.GetService(typeof(IUserManager)) as IUserManager;
                        var user = userManager.GetUser(context.Principal.UserId(), alias.SiteId); // cached

                        // check if user is valid, not deleted, has roles, and security stamp has not changed for this tenant
                        if (user != null && !user.IsDeleted && !string.IsNullOrEmpty(user.Roles) && (context.Principal.SecurityStamp() == user.SecurityStamp || context.Principal.SiteKey() != alias.SiteKey))
                        {
                            // validate security stamp and sitekey (in case user has changed tenants/sites in installation)
                            if (context.Principal.SecurityStamp() != user.SecurityStamp || context.Principal.SiteKey() != alias.SiteKey || !context.Principal.Roles().Any())
                            {
                                // refresh principal
                                var identity = UserSecurity.CreateClaimsIdentity(alias, user);
                                context.ReplacePrincipal(new ClaimsPrincipal(identity));
                                context.ShouldRenew = true;
                                Log(_logger, alias, "Permissions Refreshed For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                            }
                        }
                        else
                        {
                            // remove principal (ie. log user out)
                            Log(_logger, alias, "Permissions Removed For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(Constants.AuthenticationScheme);
                        }
                    }
                    else
                    {
                        // user is signed in but site cannot be determined
                        Log(_logger, alias, "Alias Could Not Be Resolved For User {Username} Accessing {Url}", context.Principal.Identity.Name, path);
                    }
                }
            }
        }

        private static void Log (ILogManager logger, Alias alias, string message, string username, string path)
        {
            if (!path.StartsWith("/api/")) // reduce log verbosity
            {
                var siteId = (alias != null) ? alias.SiteId : -1;
                logger.Log(siteId, LogLevel.Information, "UserValidation", Enums.LogFunction.Security, message, username, path);
            }
        }
    }
}
