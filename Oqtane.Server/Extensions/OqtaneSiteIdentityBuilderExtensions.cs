using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteIdentityBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder<TAlias> WithSiteIdentity<TAlias>(
            this OqtaneSiteOptionsBuilder<TAlias> builder)
            where TAlias : class, IAlias, new()
        {
            // site identity options
            builder.AddSiteOptions<IdentityOptions>((options, alias) =>
            {
                // password options
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequiredLength"))
                {
                    options.Password.RequiredLength = int.Parse(alias.SiteSettings["IdentityOptions:Password:RequiredLength"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequiredUniqueChars"))
                {
                    options.Password.RequiredUniqueChars = int.Parse(alias.SiteSettings["IdentityOptions:Password:RequiredUniqueChars"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequireDigit"))
                {
                    options.Password.RequireDigit = bool.Parse(alias.SiteSettings["IdentityOptions:Password:RequireDigit"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequireUppercase"))
                {
                    options.Password.RequireUppercase = bool.Parse(alias.SiteSettings["IdentityOptions:Password:RequireUppercase"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequireLowercase"))
                {
                    options.Password.RequireLowercase = bool.Parse(alias.SiteSettings["IdentityOptions:Password:RequireLowercase"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:RequireNonAlphanumeric"))
                {
                    options.Password.RequireNonAlphanumeric = bool.Parse(alias.SiteSettings["IdentityOptions:Password:RequireNonAlphanumeric"]);
                }

                // lockout options
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:MaxFailedAccessAttempts"))
                {
                    options.Lockout.MaxFailedAccessAttempts = int.Parse(alias.SiteSettings["IdentityOptions:Password:MaxFailedAccessAttempts"]);
                }
                if (alias.SiteSettings.ContainsKey("IdentityOptions:Password:DefaultLockoutTimeSpan"))
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Parse(alias.SiteSettings["IdentityOptions:Password:DefaultLockoutTimeSpan"]);
                }
            });

            return builder;
        }
    }
}
