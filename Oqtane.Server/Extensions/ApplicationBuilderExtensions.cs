using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;

namespace Oqtane.Extensions
{
    public static class ApplicationBuilderExtensions
    {
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

        public static IApplicationBuilder UseOqtaneLocalization(this IApplicationBuilder app)
        {
            var localizationManager = app.ApplicationServices.GetService<ILocalizationManager>();
            var defaultCulture = localizationManager.GetDefaultCulture();
            var supportedCultures = localizationManager.GetSupportedCultures();

            CultureInfo.CurrentUICulture = new CultureInfo(defaultCulture);

            app.UseRequestLocalization(options => {
                options.SetDefaultCulture(defaultCulture)
                    .AddSupportedUICultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            return app;
        }
    }
}
