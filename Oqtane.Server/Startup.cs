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
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10)); // increase from default of 5 seconds

            services.AddServerSideBlazor()
                .AddCircuitOptions(options =>
                {
                    if (_env.IsDevelopment())
                    {
                        options.DetailedErrors = true;
                    }
                });

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            services.AddHttpClients();

            // register scoped core services
            services.AddScoped<IAuthorizationHandler, PermissionHandler>()
                .AddOqtaneScopedServices()
                .AddOqtaneServerScopedServices();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.AddIdentityCore<IdentityUser>(options => { })
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ClaimsPrincipalFactory<IdentityUser>>(); // role claims

            services.ConfigureOqtaneIdentityOptions(Configuration);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = Constants.AuthenticationScheme;
                    options.DefaultChallengeScheme = Constants.AuthenticationScheme;
                    options.DefaultSignOutScheme = Constants.AuthenticationScheme;
                })
                .AddCookie(Constants.AuthenticationScheme)
                .AddOpenIdConnect(AuthenticationProviderTypes.OpenIDConnect, options => { })
                .AddOAuth(AuthenticationProviderTypes.OAuth2, options => { });

            services.ConfigureOqtaneCookieOptions();
            services.ConfigureOqtaneAuthenticationOptions(Configuration);

            services.AddOqtaneSiteOptions()
                .WithSiteIdentity()
                .WithSiteAuthentication();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddOqtaneApplicationParts() // register any Controllers from custom modules
            .ConfigureOqtaneMvc(); // any additional configuration from IStartup classes

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.ToString()); // Handle SchemaId already used for different type
            });
            services.TryAddSwagger(_useSwagger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISyncManager sync, ILogger<Startup> logger)
        {
            if (!string.IsNullOrEmpty(_configureServicesErrors))
            {
                logger.LogError(_configureServicesErrors);
            }

            ServiceActivator.Configure(app.ApplicationServices);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // execute any IServerStartup logic
            app.ConfigureOqtaneAssemblies(env);

            // allow oqtane localization middleware
            app.UseOqtaneLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseTenantResolution();
            app.UseJwtAuthorization();
            app.UseBlazorFrameworkFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (_useSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/" + Constants.Version + "/swagger.json", Constants.PackageId + " " + Constants.Version); });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });

            // create a global sync event to identify server application startup
            sync.AddSyncEvent(-1, EntityNames.Host, -1, SyncEventActions.Reload);
        }
    }
}
