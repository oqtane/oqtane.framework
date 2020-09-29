using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Infrastructure.Localization;

namespace Oqtane.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly string DefaultCultureKey = "Localization:DefaultCulture";
        private static readonly string SupportedCulturesKey = "Localization:SupportedCultures";

        public static IApplicationBuilder UseOqtaneLocalization(this IApplicationBuilder app)
        {
            var configuration = app.ApplicationServices.GetService<IConfiguration>();
            var defaultCulture = configuration.GetSection(DefaultCultureKey).Value;
            var supportedCultures = configuration.GetSection(SupportedCulturesKey).Get<string[]>();
            if (defaultCulture == CultureInfo.InstalledUICulture.Name)
            {
                LocalizationSettings.DefaultCulture = defaultCulture;
            }

            if (supportedCultures.Length > 0)
            {
                LocalizationSettings.SupportedCultures.AddRange(supportedCultures);
            }

            CultureInfo.CurrentUICulture = new CultureInfo(defaultCulture);

            app.UseRequestLocalization(options => {
                options.SetDefaultCulture(defaultCulture)
                    .AddSupportedUICultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });
            
            return app;
        }

        public static IApplicationBuilder ConfigureOqtaneAssemblies(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var startUps = AppDomain.CurrentDomain
                .GetOqtaneAssemblies()
                .SelectMany(x => x.GetInstances<IServerStartup>());

            foreach (var startup in startUps)
            {
                startup.Configure(app, env);
            }
            
            return app;
        }
    }
}
