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
                options.Password.RequiredLength = int.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequiredLength", options.Password.RequiredLength.ToString()));
                options.Password.RequiredUniqueChars = int.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequiredUniqueChars", options.Password.RequiredUniqueChars.ToString()));
                options.Password.RequireDigit = bool.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequireDigit", options.Password.RequireDigit.ToString()));
                options.Password.RequireUppercase = bool.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequireUppercase", options.Password.RequireUppercase.ToString()));
                options.Password.RequireLowercase = bool.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequireLowercase", options.Password.RequireLowercase.ToString()));
                options.Password.RequireNonAlphanumeric = bool.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:RequireNonAlphanumeric", options.Password.RequireNonAlphanumeric.ToString()));

                // lockout options
                options.Lockout.MaxFailedAccessAttempts = int.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:MaxFailedAccessAttempts", options.Lockout.MaxFailedAccessAttempts.ToString()));
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Parse(alias.SiteSettings.GetValue("IdentityOptions:Password:DefaultLockoutTimeSpan", options.Lockout.DefaultLockoutTimeSpan.ToString()));
            });

            return builder;
        }
    }
}
