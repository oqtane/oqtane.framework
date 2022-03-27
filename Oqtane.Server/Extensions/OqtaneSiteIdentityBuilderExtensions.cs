using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace Oqtane.Extensions
{
    public static class OqtaneSiteIdentityBuilderExtensions
    {
        public static OqtaneSiteOptionsBuilder WithSiteIdentity(this OqtaneSiteOptionsBuilder builder)
        {
            // site identity options
            builder.AddSiteOptions<IdentityOptions>((options, alias, sitesettings) =>
            {
                // password options
                options.Password.RequiredLength = int.Parse(sitesettings.GetValue("IdentityOptions:Password:RequiredLength", options.Password.RequiredLength.ToString()));
                options.Password.RequiredUniqueChars = int.Parse(sitesettings.GetValue("IdentityOptions:Password:RequiredUniqueChars", options.Password.RequiredUniqueChars.ToString()));
                options.Password.RequireDigit = bool.Parse(sitesettings.GetValue("IdentityOptions:Password:RequireDigit", options.Password.RequireDigit.ToString()));
                options.Password.RequireUppercase = bool.Parse(sitesettings.GetValue("IdentityOptions:Password:RequireUppercase", options.Password.RequireUppercase.ToString()));
                options.Password.RequireLowercase = bool.Parse(sitesettings.GetValue("IdentityOptions:Password:RequireLowercase", options.Password.RequireLowercase.ToString()));
                options.Password.RequireNonAlphanumeric = bool.Parse(sitesettings.GetValue("IdentityOptions:Password:RequireNonAlphanumeric", options.Password.RequireNonAlphanumeric.ToString()));

                // lockout options
                options.Lockout.MaxFailedAccessAttempts = int.Parse(sitesettings.GetValue("IdentityOptions:Password:MaxFailedAccessAttempts", options.Lockout.MaxFailedAccessAttempts.ToString()));
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Parse(sitesettings.GetValue("IdentityOptions:Password:DefaultLockoutTimeSpan", options.Lockout.DefaultLockoutTimeSpan.ToString()));
            });

            return builder;
        }
    }
}
