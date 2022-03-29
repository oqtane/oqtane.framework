using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Extensions;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    internal class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var alias = context.GetAlias();
                if (alias != null)
                {
                    var sitesettings = context.GetSiteSettings();
                    var secret = sitesettings.GetValue("JwtOptions:Secret", "");
                    if (!string.IsNullOrEmpty(secret))
                    {
                        var logger = context.RequestServices.GetService(typeof(ILogManager)) as ILogManager;
                        var jwtManager = context.RequestServices.GetService(typeof(IJwtManager)) as IJwtManager;

                        var token = context.Request.Headers["Authorization"].First().Split(" ").Last();
                        var user = jwtManager.ValidateToken(token, secret, sitesettings.GetValue("JwtOptions:Issuer", ""), sitesettings.GetValue("JwtOptions:Audience", ""));
                        if (user != null)
                        {
                            // populate principal (reload user roles to ensure most accurate permission assigments)
                            var _userRoles = context.RequestServices.GetService(typeof(IUserRoleRepository)) as IUserRoleRepository;
                            var principal = (ClaimsIdentity)context.User.Identity;
                            UserSecurity.ResetClaimsIdentity(principal);
                            var identity = UserSecurity.CreateClaimsIdentity(alias, user, _userRoles.GetUserRoles(user.UserId, alias.SiteId).ToList());
                            principal.AddClaims(identity.Claims);
                            logger.Log(alias.SiteId, LogLevel.Information, "TokenValidation", Enums.LogFunction.Security, "Token Validated For User {Username}", user.Username);
                        }
                        else
                        {
                            logger.Log(alias.SiteId, LogLevel.Error, "TokenValidation", Enums.LogFunction.Security, "Token Validation Error");
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
