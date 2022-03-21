using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Oqtane.Repository;
using System.IO;
using System.Collections.Generic;
using Oqtane.Security;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteAuthenticationBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthentication<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            builder.WithSiteAuthenticationOptions();

            return builder;
        }

        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthenticationOptions<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            // site OpenIdConnect options
            builder.AddSiteOptions<OpenIdConnectOptions>((options, alias) =>
            {
                // default options
                options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                options.RequireHttpsMetadata = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-oidc" : "/" + alias.Path + "/signin-oidc";
                options.ResponseType = OpenIdConnectResponseType.Code; // authorization code flow
                options.ResponseMode = OpenIdConnectResponseMode.FormPost; // recommended as most secure
                options.UsePkce = true;
                options.Scope.Add("openid"); // core claims
                options.Scope.Add("profile");  // name claims
                options.Scope.Add("email"); // email claim
                //options.Scope.Add("offline_access"); // refresh token

                // cookie config is required to avoid Correlation Failed errors
                options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                // site options
                options.Authority = alias.SiteSettings.GetValue("OpenIdConnectOptions:Authority", options.Authority);
                options.ClientId = alias.SiteSettings.GetValue("OpenIdConnectOptions:ClientId", options.ClientId);
                options.ClientSecret = alias.SiteSettings.GetValue("OpenIdConnectOptions:ClientSecret", options.ClientSecret);
                options.MetadataAddress = alias.SiteSettings.GetValue("OpenIdConnectOptions:MetadataAddress", options.MetadataAddress);

                // openid connect events
                options.Events.OnTokenValidated = OnTokenValidated;
                options.Events.OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut;
                options.Events.OnAccessDenied = OnAccessDenied;
                options.Events.OnRemoteFailure = OnRemoteFailure;
            });

            // site ChallengeScheme options 
            builder.AddSiteOptions<AuthenticationOptions>((options, alias) =>
            {
                if (alias.SiteSettings.GetValue("OpenIdConnectOptions:Authority", "") != "")
                {
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }
            });

            return builder;
        }

        private static async Task OnTokenValidated(TokenValidatedContext context)
        {
            var providerKey = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginProvider = context.HttpContext.GetAlias().SiteSettings.GetValue("OpenIdConnectOptions:Authority", "");
            var emailClaimType = context.HttpContext.GetAlias().SiteSettings.GetValue("OpenIdConnectOptions:EmailClaimType", "");
            if (string.IsNullOrEmpty(emailClaimType))
            {
                emailClaimType = ClaimTypes.Email;
            }
            var alias = context.HttpContext.GetAlias();
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();

            // custom logic may be needed here to manipulate Principal sent by Provider - use interface similar to IClaimsTransformation

            var email = context.Principal.FindFirstValue(emailClaimType);

            // validate email claim
            if (email == null || !email.Contains("@") || !email.Contains("."))
            {
                var emailclaimtype = context.Principal.Claims.FirstOrDefault(item => item.Value.Contains("@") && item.Value.Contains("."));
                if (emailclaimtype != null)
                {
                    email = emailclaimtype.Value;
                    _logger.Log(LogLevel.Information, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "Please Update The Email Claim Type For The OpenID Connect Provider To {EmailClaimType} In Site Settings", emailclaimtype.Type);
                }
                else
                {
                    email = null;
                }
            }

            if (email != null)
            {
                var _identityUserManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var _users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var _userRoles = context.HttpContext.RequestServices.GetRequiredService<IUserRoleRepository>();
                User user = null;

                var identityuser = await _identityUserManager.FindByEmailAsync(email);
                if (identityuser == null)
                {
                    identityuser = new IdentityUser();
                    identityuser.UserName = email;
                    identityuser.Email = email;
                    identityuser.EmailConfirmed = true;
                    var result = await _identityUserManager.CreateAsync(identityuser, DateTime.UtcNow.ToString("yyyy-MMM-dd-HH-mm-ss"));
                    if (result.Succeeded)
                    {
                        // add user login
                        await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(loginProvider, providerKey, email));

                        user = new User();
                        user.SiteId = alias.SiteId;
                        user.Username = email;
                        user.DisplayName = email;
                        user.Email = email;
                        user.LastLoginOn = null;
                        user.LastIPAddress = "";
                        user = _users.AddUser(user);

                        // add folder for user
                        var _folders = context.HttpContext.RequestServices.GetRequiredService<IFolderRepository>();
                        Folder folder = _folders.GetFolder(user.SiteId, Utilities.PathCombine("Users", Path.DirectorySeparatorChar.ToString()));
                        if (folder != null)
                        {
                            _folders.AddFolder(new Folder
                            {
                                SiteId = folder.SiteId,
                                ParentId = folder.FolderId,
                                Name = "My Folder",
                                Type = FolderTypes.Private,
                                Path = Utilities.PathCombine(folder.Path, user.UserId.ToString(), Path.DirectorySeparatorChar.ToString()),
                                Order = 1,
                                ImageSizes = "",
                                Capacity = Constants.UserFolderCapacity,
                                IsSystem = true,
                                Permissions = new List<Permission>
                                {
                                    new Permission(PermissionNames.Browse, user.UserId, true),
                                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                    new Permission(PermissionNames.Edit, user.UserId, true)
                                }.EncodePermissions()
                            });
                        }

                        // add auto assigned roles to user for site
                        var _roles = context.HttpContext.RequestServices.GetRequiredService<IRoleRepository>();
                        List<Role> roles = _roles.GetRoles(user.SiteId).Where(item => item.IsAutoAssigned).ToList();
                        foreach (Role role in roles)
                        {
                            UserRole userrole = new UserRole();
                            userrole.UserId = user.UserId;
                            userrole.RoleId = role.RoleId;
                            userrole.EffectiveDate = null;
                            userrole.ExpiryDate = null;
                            _userRoles.AddUserRole(userrole);
                        }
                    }
                }
                else
                {
                    var logins = await _identityUserManager.GetLoginsAsync(identityuser);
                    var login = logins.FirstOrDefault(item => item.LoginProvider == loginProvider);
                    if (login != null)
                    {
                        if (login.ProviderKey == providerKey)
                        {
                            user = _users.GetUser(identityuser.UserName);
                        }
                        else
                        {
                            // provider keys do not match
                            _logger.Log(LogLevel.Error, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "OpenId Connect Provider Key Does Not Match For User {Email}. Login Denied.", email);
                        }
                    }
                    else
                    {
                        // add user login
                        await _identityUserManager.AddLoginAsync(identityuser, new UserLoginInfo(loginProvider, providerKey, identityuser.UserName));
                        user = _users.GetUser(identityuser.UserName);
                    }
                }

                // add claims to principal
                if (user != null)
                {
                    // update user
                    user.LastLoginOn = DateTime.UtcNow;
                    user.LastIPAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                    _users.UpdateUser(user);
                    _logger.Log(LogLevel.Information, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "User Login Successful {Username}", user.Username);

                    var principal = (ClaimsIdentity)context.Principal.Identity;

                    // remove the name claim if it exists in the principal
                    var nameclaim = principal.Claims.FirstOrDefault(item => item.Type == ClaimTypes.Name);
                    if (nameclaim != null)
                    {
                        principal.RemoveClaim(nameclaim);
                    }

                    // add Oqtane claims
                    List<UserRole> userroles = _userRoles.GetUserRoles(user.UserId, user.SiteId).ToList();
                    var identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                    principal.AddClaims(identity.Claims);

                    // add provider
                    principal.AddClaim(new Claim("Provider", context.HttpContext.GetAlias().SiteSettings["OpenIdConnectOptions:Authority"]));
                }
            }
            else // no email claim
            {
                _logger.Log(LogLevel.Error, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "OpenID Connect Provider Did Not Return An Email Claim To Uniquely Identify The User");
            }
        }

        private static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            var logoutUrl = context.HttpContext.GetAlias().SiteSettings.GetValue("OpenIdConnectOptions:LogoutUrl", "");
            if (logoutUrl != "")
            {
                var postLogoutUri = context.Properties.RedirectUri;
                if (!string.IsNullOrEmpty(postLogoutUri))
                {
                    if (postLogoutUri.StartsWith("/"))
                    {
                        var request = context.Request;
                        postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                    }
                    logoutUrl += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                }
                context.Response.Redirect(logoutUrl);
                context.HandleResponse();
            }
            return Task.CompletedTask;
        }

        private static Task OnAccessDenied(AccessDeniedContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Information, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "OpenID Connect Access Denied - User May Have Cancelled Their External Login Attempt");
            // redirect to login page
            var alias = context.HttpContext.GetAlias();
            context.Response.Redirect(alias.Path + "/login?returnurl=" + context.Properties.RedirectUri);
            context.HandleResponse();
            return Task.CompletedTask;
        }

        private static Task OnRemoteFailure(RemoteFailureContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
            _logger.Log(LogLevel.Error, nameof(OqtaneSiteAuthenticationBuilderExtensions), Enums.LogFunction.Security, "OpenID Connect Remote Failure - {Error}", context.Failure.Message);
            // redirect to login page
            var alias = context.HttpContext.GetAlias();
            context.Response.Redirect(alias.Path + "/login?returnurl=" + context.Properties.RedirectUri);
            context.HandleResponse();
            return Task.CompletedTask;
        }
    }
}
