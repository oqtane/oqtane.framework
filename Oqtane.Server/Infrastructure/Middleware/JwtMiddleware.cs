using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Extensions;
using Oqtane.Models;
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
                        var identity = jwtManager.ValidateToken(token, secret, sitesettings.GetValue("JwtOptions:Issuer", ""), sitesettings.GetValue("JwtOptions:Audience", ""));
                        if (identity != null && identity.Claims.Any())
                        {
                            // create user identity using jwt claims (note the difference in claimtype names)
                            var user = new User
                            {
                                UserId = int.Parse(identity.Claims.FirstOrDefault(item => item.Type == "nameid")?.Value),
                                Username = identity.Claims.FirstOrDefault(item => item.Type == "name")?.Value
                            };
                            // jwt already contains the roles - we are reloading to ensure most accurate permissions
                            var _userRoles = context.RequestServices.GetService(typeof(IUserRoleRepository)) as IUserRoleRepository;

                            // set claims identity
                            var claimsidentity = UserSecurity.CreateClaimsIdentity(alias, user, _userRoles.GetUserRoles(user.UserId, alias.SiteId).ToList());
                            context.User = new ClaimsPrincipal(claimsidentity);

                            logger.Log(alias.SiteId, LogLevel.Information, "TokenValidation", Enums.LogFunction.Security, "Token Validated For User {Username}", user.Username);
                        }
                        else
                        {
                            logger.Log(alias.SiteId, LogLevel.Error, "TokenValidation", Enums.LogFunction.Security, "Token Validation Error");
                        }
                    }
                }
            }

            // continue processing
            if (_next != null) await _next(context);
        }
    }
}
