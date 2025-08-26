using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Oqtane.Components;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.UI;
using OqtaneSSR.Extensions;

namespace Oqtane.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOqtane(this IApplicationBuilder app, IConfigurationRoot configuration, IWebHostEnvironment environment, ICorsService corsService, ICorsPolicyProvider corsPolicyProvider, ISyncManager sync)
        {
            ServiceActivator.Configure(app.ApplicationServices);

            if (environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseForwardedHeaders();
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // allow oqtane localization middleware
            app.UseOqtaneLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = (ctx) =>
                {
                    // static asset caching
                    var cachecontrol = configuration.GetSection("CacheControl");
                    if (!string.IsNullOrEmpty(cachecontrol.Value))
                    {
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, cachecontrol.Value);
                    }
                    // CORS headers for .NET MAUI clients
                    var policy = corsPolicyProvider.GetPolicyAsync(ctx.Context, Constants.MauiCorsPolicy)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    corsService.ApplyResult(corsService.EvaluatePolicy(ctx.Context, policy), ctx.Context.Response);
                }
            });
            app.UseExceptionMiddleWare();
            app.UseTenantResolution();
            app.UseJwtAuthorization();
            app.UseRouting();
            app.UseCors();
            app.UseOutputCache();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            // execute any IServerStartup logic
            app.ConfigureOqtaneAssemblies(environment);

            if (configuration.GetSection("UseSwagger").Value != "false")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/" + Constants.Version + "/swagger.json", Constants.PackageId + " " + Constants.Version); });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode()
                    .AddInteractiveWebAssemblyRenderMode()
                    .AddAdditionalAssemblies(typeof(SiteRouter).Assembly);
            });

            // simulate the fallback routing approach of traditional Blazor - allowing the custom SiteRouter to handle all routing concerns
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallback();
            });

            // create a global sync event to identify server application startup
            sync.AddSyncEvent(-1, -1, EntityNames.Host, -1, SyncEventActions.Reload);

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

        public static IApplicationBuilder UseOqtaneLocalization(this IApplicationBuilder app)
        {
            var localizationManager = app.ApplicationServices.GetService<ILocalizationManager>();
            var defaultCulture = localizationManager.GetDefaultCulture();
            var supportedCultures = localizationManager.GetSupportedCultures();

            app.UseRequestLocalization(options => {
                options.SetDefaultCulture(defaultCulture)
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                foreach(var culture in options.SupportedCultures)
                {
                    if (culture.TextInfo.IsRightToLeft)
                    {
                        RightToLeftCulture.ResolveFormat(culture);
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
