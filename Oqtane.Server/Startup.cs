using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Oqtane.Components;
using Oqtane.UI;
using OqtaneSSR.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Oqtane.Providers;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Oqtane
{
    public class Startup
    {
        private readonly bool _useSwagger;
        private readonly IWebHostEnvironment _env;
        private readonly string[] _installedCultures;
        private string _configureServicesErrors;

        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env, ILocalizationManager localizationManager)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            _installedCultures = localizationManager.GetInstalledCultures();

            //add possibility to switch off swagger on production.
            _useSwagger = Configuration.GetSection("UseSwagger").Value != "false";

            AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(env.ContentRootPath, "Data"));

            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // process forwarded headers on load balancers and proxy servers
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // register localization services
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddOptions<List<Database>>().Bind(Configuration.GetSection(SettingKeys.AvailableDatabasesSection));

            // register scoped core services
            services.AddScoped<IAuthorizationHandler, PermissionHandler>()
                .AddOqtaneServerScopedServices();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            services.AddHttpClients();

            // register singleton scoped core services
            services.AddSingleton(Configuration)
                .AddOqtaneSingletonServices();

            // install any modules or themes ( this needs to occur BEFORE the assemblies are loaded into the app domain )
            _configureServicesErrors += InstallationManager.InstallPackages(_env.WebRootPath, _env.ContentRootPath);

            // register transient scoped core services
            services.AddOqtaneTransientServices();

            // load the external assemblies into the app domain, install services
            services.AddOqtane(_installedCultures);
            services.AddOqtaneDbContext();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = Constants.AntiForgeryTokenHeaderName;
                options.Cookie.Name = Constants.AntiForgeryTokenCookieName;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
            });

            services.AddIdentityCore<IdentityUser>(options => { })
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ClaimsPrincipalFactory<IdentityUser>>(); // role claims

            services.ConfigureOqtaneIdentityOptions(Configuration);

            services.AddCascadingAuthenticationState();
            services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            services.AddAuthorization();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = Constants.AuthenticationScheme;
            })
            .AddCookie(Constants.AuthenticationScheme)
            .AddOpenIdConnect(AuthenticationProviderTypes.OpenIDConnect, options => { })
            .AddOAuth(AuthenticationProviderTypes.OAuth2, options => { });

            services.ConfigureOqtaneCookieOptions();
            services.ConfigureOqtaneAuthenticationOptions(Configuration);

            services.AddOqtaneSiteOptions()
                .WithSiteIdentity()
                .WithSiteAuthentication();

            services.AddCors(options =>
            {
                options.AddPolicy(Constants.MauiCorsPolicy,
                    policy =>
                    {
                        // allow .NET MAUI client cross origin calls
                        policy.WithOrigins("https://0.0.0.1", "http://0.0.0.1", "app://0.0.0.1")
                            .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
            });

            services.AddOutputCache();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddOqtaneApplicationParts() // register any Controllers from custom modules
            .ConfigureOqtaneMvc(); // any additional configuration from IStartup classes

            services.AddRazorPages();

            services.AddRazorComponents()
               .AddInteractiveServerComponents(options =>
               {
                   if (_env.IsDevelopment())
                   {
                       options.DetailedErrors = true;
                   }
               }).AddHubOptions(options =>
               {
                   options.MaximumReceiveMessageSize = null; // no limit (for large amounts of data ie. textarea components)
               })
               .AddInteractiveWebAssemblyComponents();

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.ToString()); // Handle SchemaId already used for different type
            });
            services.TryAddSwagger(_useSwagger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISyncManager sync, ICorsService corsService, ICorsPolicyProvider corsPolicyProvider, ILogger<Startup> logger)
        {
            if (!string.IsNullOrEmpty(_configureServicesErrors))
            {
                logger.LogError(_configureServicesErrors);
            }

            ServiceActivator.Configure(app.ApplicationServices);

            if (env.IsDevelopment())
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
                    var cachecontrol = Configuration.GetSection("CacheControl");
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
            app.ConfigureOqtaneAssemblies(env);

            if (_useSwagger)
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
        }
    }
}
