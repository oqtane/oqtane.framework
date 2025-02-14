using System;
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

            app.UseRequestLocalization(options => {
                options.SetDefaultCulture(defaultCulture)
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                for  (var i = 0; i < options.SupportedCultures.Count; i++)
                {
                    if (options.SupportedCultures[i].Name.Equals("fa-IR", StringComparison.OrdinalIgnoreCase))
                    {
                        options.SupportedCultures[i] = PersianCulture.GetPersianCultureInfo();
                        break;
                    }
                }
            });

            return app;
        }

        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
          => builder.UseMiddleware<TenantMiddleware>();

        public static IApplicationBuilder UseJwtAuthorization(this IApplicationBuilder builder)
          => builder.UseMiddleware<JwtMiddleware>();

        public static IApplicationBuilder UseExceptionMiddleWare(this IApplicationBuilder builder)
          => builder.UseMiddleware<ExceptionMiddleware>();

    }
}
