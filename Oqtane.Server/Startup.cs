using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Security;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane
{
    public class Startup
    {
        private static readonly string[] DefaultSupportedCultures = new[] { Constants.DefaultCulture };

        private string _webRoot;
        private string _connectionString;
        private Runtime _runtime;
        private bool _useSwagger;
        private IWebHostEnvironment _env;

        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            _runtime = (Configuration.GetSection("Runtime").Value == "WebAssembly") ? Runtime.WebAssembly : Runtime.Server;

            //add possibility to switch off swagger on production.
            _useSwagger = Configuration.GetSection("UseSwagger").Value != "false";

            _webRoot = env.WebRootPath;

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(env.ContentRootPath, "Data"));
            
            _connectionString = Configuration.GetConnectionString("DefaultConnection").Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Register localization services
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddServerSideBlazor().AddCircuitOptions(options =>
            {
                if (_env.IsDevelopment())
                {
                    options.DetailedErrors = true;
                }
            });

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            services.AddHttpClientWithAuthCookie();

            services.AddOqtaneAuthorizationPolicies();

            services.AddOqtaneScopedServices();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.UseOqtaneSqlServerDatabase(_connectionString);

            var localizationSection = Configuration.GetSection("Localization");
            var localizationOptions = localizationSection.Get<LocalizationOptions>();

            services.Configure<LocalizationOptions>(localizationSection);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = false;
            });

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme);

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = false;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // register custom claims principal factory for role claims
            services.AddTransient<IUserClaimsPrincipalFactory<IdentityUser>, ClaimsPrincipalFactory<IdentityUser>>();

            services.AddOqtaneSingletonServices();

            // install any modules or themes ( this needs to occur BEFORE the assemblies are loaded into the app domain )
            InstallationManager.InstallPackages("Modules,Themes", _webRoot);

            services.AddOqtaneTransientServices();

            // load the external assemblies into the app domain, install services 
            services.AddOqtane(_runtime,
                localizationOptions.SupportedCultures.IsNullOrEmpty()
                    ? DefaultSupportedCultures
                    : localizationOptions.SupportedCultures);

            services.AddMvc()
                .AddNewtonsoftJson()
                .AddOqtaneApplicationParts() // register any Controllers from custom modules
                .AddOqtaneMvcConfiguration(); // any additional configuration from IStart classes.

            services.AddOqtaneSwaggerDocs(options =>
            {
                options.Enable = _useSwagger;
                options.Name = "Oqtane";
                options.Title = options.Version = "v1";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            // to allow install middleware it should be moved up
            app.UseOqtaneConfiguration(env);

            // Allow oqtane localization middleware
            app.UseOqtaneLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            if (_useSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oqtane V1"); });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
