using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Extensions;
using Oqtane.Managers;
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
                            var idclaim = "nameid";
                            var nameclaim = "unique_name";
                            var legacynameclaim = "name"; // this was a breaking change in System.IdentityModel.Tokens.Jwt in .NET 7

                            // get jwt claims for userid and username
                            var userid = identity.Claims.FirstOrDefault(item => item.Type == idclaim)?.Value;
                            if (userid != null)
                            {
                                if (!int.TryParse(userid, out _))
                                {
                                    userid = null;
                                }
                            }
                            var username = identity.Claims.FirstOrDefault(item => item.Type == nameclaim)?.Value;
                            if (username == null)
                            {
                                // fallback for legacy clients
                                username = identity.Claims.FirstOrDefault(item => item.Type == legacynameclaim)?.Value;
                            }

                            if (userid != null && username != null)
                            {
                                var _users = context.RequestServices.GetService(typeof(IUserManager)) as IUserManager;
                                var user = _users.GetUser(int.Parse(userid), alias.SiteId); // cached
                                if (user != null && !user.IsDeleted)
                                {
                                    var claimsidentity = UserSecurity.CreateClaimsIdentity(alias, user);
                                    context.User = new ClaimsPrincipal(claimsidentity);
                                    logger.Log(alias.SiteId, LogLevel.Information, "TokenValidation", Enums.LogFunction.Security, "Token Validated For User {Username}", user.Username);
                                }
                                else
                                {
                                    logger.Log(alias.SiteId, LogLevel.Error, "TokenValidation", Enums.LogFunction.Security, "Token Validated But User {Username} Does Not Exist Or Is Deleted", user.Username);
                                }
                            }
                            else
                            {
                                logger.Log(alias.SiteId, LogLevel.Error, "TokenValidation", Enums.LogFunction.Security, "Token Validated But Could Not Locate UserId Or Username In Claims {Claims}", identity.Claims.ToString());
                            }
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
