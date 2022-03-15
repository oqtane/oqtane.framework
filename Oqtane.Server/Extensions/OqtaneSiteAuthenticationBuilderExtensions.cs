using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

namespace Oqtane.Extensions
{
    public static class OqtaneSiteAuthenticationBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthentication<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            builder.WithSiteAuthenticationCore();
            builder.WithSiteAuthenticationOptions();

            return builder;
        }

        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthenticationCore<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            builder.Services.DecorateService<IAuthenticationService, SiteAuthenticationService<TAlias>>();
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, SiteAuthenticationSchemeProvider>());

            return builder;
        }

        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteAuthenticationOptions<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            // site OpenIdConnect options
            builder.AddSiteOptions<OpenIdConnectOptions>((options, alias) =>
            {
                if (alias.SiteSettings.ContainsKey("OpenIdConnectOptions:Authority"))
                {
                    options.Authority = alias.SiteSettings["OpenIdConnectOptions:Authority"];
                }
                if (alias.SiteSettings.ContainsKey("OpenIdConnectOptions:ClientId"))
                {
                    options.ClientId = alias.SiteSettings["OpenIdConnectOptions:ClientId"];
                }
                if (alias.SiteSettings.ContainsKey("OpenIdConnectOptions:ClientSecret"))
                {
                    options.ClientSecret = alias.SiteSettings["OpenIdConnectOptions:ClientSecret"];
                }

                // default options
                options.SignInScheme = Constants.AuthenticationScheme; // identity cookie
                options.RequireHttpsMetadata = true;
                options.UsePkce = true;
                options.Scope.Add("openid"); // core claims
                options.Scope.Add("profile");  // name claims
                options.Scope.Add("email"); // email claim
                //options.Scope.Add("offline_access"); // get refresh token
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.CallbackPath = string.IsNullOrEmpty(alias.Path) ? "/signin-oidc" : "/" + alias.Path + "/signin-oidc";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Events.OnTokenValidated = OnTokenValidated;
            });

            // site ChallengeScheme options 
            builder.AddSiteOptions<AuthenticationOptions>((options, alias) =>
            {
                if (alias.SiteSettings.ContainsKey("OpenIdConnectOptions:Authority") && !string.IsNullOrEmpty(alias.SiteSettings["OpenIdConnectOptions:Authority"]))
                {
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }
            });

            return builder;
        }

        private static async Task OnTokenValidated(TokenValidatedContext context)
        {
            var email = context.Principal.Identity.Name;
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
                        user = new User();
                        user.SiteId = context.HttpContext.GetAlias().SiteId;
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
                    email = identityuser.UserName;
                }

                // add claims to principal
                user = _users.GetUser(email);
                if (user != null)
                {
                    var principal = (ClaimsIdentity)context.Principal.Identity;

                    // remove the name claim if it exists in the principal
                    var nameclaim = principal.Claims.FirstOrDefault(item => item.Type == ClaimTypes.Name);
                    if (nameclaim != null)
                    {
                        principal.RemoveClaim(nameclaim);
                    }

                    // add Oqtane claims
                    List<UserRole> userroles = _userRoles.GetUserRoles(user.UserId, context.HttpContext.GetAlias().SiteId).ToList();
                    var identity = UserSecurity.CreateClaimsIdentity(context.HttpContext.GetAlias(), user, userroles);
                    principal.AddClaims(identity.Claims);
                }
            }
            else
            {
                var _logger = context.HttpContext.RequestServices.GetRequiredService<ILogManager>();
                _logger.Log(LogLevel.Information, "OqtaneSiteAuthenticationBuilderExtensions", Enums.LogFunction.Security, "OpenId Connect Server Did Not Return An Email For User");
            }
        }

        public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
        {
            var existingService = services.SingleOrDefault(s => s.ServiceType == typeof(TService));
            if (existingService == null)
                return false;

            var newService = new ServiceDescriptor(existingService.ServiceType,
                sp =>
                {
                    TService inner = (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType!);

                    var parameters2 = new object[parameters.Length + 1];
                    Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                    parameters2[0] = inner;

                    return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2)!;
                },
                existingService.Lifetime);

            if (existingService.ImplementationInstance != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                    sp =>
                    {
                        TService inner = (TService)existingService.ImplementationInstance;
                        return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                    },
                    existingService.Lifetime);
            }
            else if (existingService.ImplementationFactory != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                    sp =>
                    {
                        TService inner = (TService)existingService.ImplementationFactory(sp);
                        return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                    },
                    existingService.Lifetime);
            }

            services.Remove(existingService);
            services.Add(newService);

            return true;
        }
    }
}
