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

namespace Oqtane
{
    public class Startup
    {
        private readonly Runtime _runtime;
        private readonly bool _useSwagger;
        private readonly IWebHostEnvironment _env;
        private readonly string[] _supportedCultures;

        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env, ILocalizationManager localizationManager)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            _supportedCultures = localizationManager.GetSupportedCultures();
            _runtime = (Configuration.GetSection("Runtime").Value == "WebAssembly") ? Runtime.WebAssembly : Runtime.Server;

            //add possibility to switch off swagger on production.
            _useSwagger = Configuration.GetSection("UseSwagger").Value != "false";

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(env.ContentRootPath, "Data"));

            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Register localization services
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddOptions<List<Database>>().Bind(Configuration.GetSection(SettingKeys.AvailableDatabasesSection));

            services.AddServerSideBlazor()
                .AddCircuitOptions(options =>
                {
                    if (_env.IsDevelopment())
                    {
                        options.DetailedErrors = true;
                    }
                });

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            services.TryAddHttpClientWithAuthenticationCookie();

            // register custom authorization policies
            services.AddOqtaneAuthorizationPolicies();

            // register scoped core services
            services.AddScoped<IAuthorizationHandler, PermissionHandler>()
                .AddOqtaneScopedServices();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddIdentityCore<IdentityUser>(options => { })
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ClaimsPrincipalFactory<IdentityUser>>(); // role claims

            services.ConfigureOqtaneIdentityOptions();

            services.AddAuthentication(Constants.AuthenticationScheme)
                .AddCookie(Constants.AuthenticationScheme);

            services.ConfigureOqtaneCookieOptions();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = Constants.AntiForgeryTokenHeaderName;
                options.Cookie.HttpOnly = false;
                options.Cookie.Name = Constants.AntiForgeryTokenCookieName;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // register singleton scoped core services
            services.AddSingleton(Configuration)
                .AddOqtaneSingletonServices();

            // install any modules or themes ( this needs to occur BEFORE the assemblies are loaded into the app domain )
            InstallationManager.InstallPackages(_env.WebRootPath, _env.ContentRootPath);

            // register transient scoped core services
            services.AddOqtaneTransientServices();

            // load the external assemblies into the app domain, install services
            services.AddOqtane(_runtime, _supportedCultures);
            services.AddOqtaneDbContext();


            services.AddMvc()
                .AddNewtonsoftJson()
                .AddOqtaneApplicationParts() // register any Controllers from custom modules
                .ConfigureOqtaneMvc(); // any additional configuration from IStart classes.

            services.TryAddSwagger(_useSwagger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISyncManager sync)
        {
            ServiceActivator.Configure(app.ApplicationServices);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // execute any IServerStartup logic
            app.ConfigureOqtaneAssemblies(env);

            // Allow oqtane localization middleware
            app.UseOqtaneLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseTenantResolution();
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

            // create a sync event to identify server application startup
            sync.AddSyncEvent(-1, "Application", -1);
        }
    }
}
