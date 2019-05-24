using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Filters;
using System.IO;
using System.Runtime.Loader;
using Oqtane.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Client;
using Oqtane.Shared;

namespace Oqtane.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(env.ContentRootPath, "Data"));
        }

#if DEBUG || RELEASE
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // server-side Blazor does not register HttpClient by default
            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                // setup HttpClient for server side in a client side compatible fashion
                services.AddScoped<HttpClient>(s =>
                {
                    // creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var uriHelper = s.GetRequiredService<IUriHelper>();
                    return new HttpClient
                    {
                        BaseAddress = new Uri(uriHelper.GetBaseUri())
                    };
                });
            }

            // register scoped core services
            services.AddScoped<SiteState>();
            services.AddScoped<IModuleDefinitionService, ModuleDefinitionService>();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<IAliasService, AliasService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IPageModuleService, PageModuleService>();
            services.AddScoped<IUserService, UserService>();

            // dynamically register module contexts and repository services
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] implementationtypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (Type implementationtype in implementationtypes)
                {
                    Type servicetype = Type.GetType(implementationtype.FullName.Replace(implementationtype.Name, "I" + implementationtype.Name));
                    if (servicetype != null)
                    {
                        services.AddScoped(servicetype, implementationtype); // traditional service interface
                    }
                    else
                    {
                        services.AddScoped(implementationtype, implementationtype); // no interface defined for service
                    }
                }
            }

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<HostContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
                ));
            services.AddDbContext<TenantContext>(options => { });

            services.AddMemoryCache();

            services.AddMvc().AddNewtonsoftJson();

            // register database install/upgrade filter
            services.AddTransient<IStartupFilter, UpgradeFilter>();

            // register singleton scoped core services
            services.AddSingleton<IModuleDefinitionRepository, ModuleDefinitionRepository>();
            services.AddSingleton<IThemeRepository, ThemeRepository>();

            // register transient scoped core services
            services.AddTransient<ITenantResolver, TenantResolver>();
            services.AddTransient<IAliasRepository, AliasRepository>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<ISiteRepository, SiteRepository>();
            services.AddTransient<IPageRepository, PageRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<IPageModuleRepository, PageModuleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            // get list of loaded assemblies
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // get path to /bin
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo folder = new DirectoryInfo(path);
            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("Oqtane.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
                }
            }
            
            // dynamically register module contexts and repository services
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] implementationtypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (Type implementationtype in implementationtypes)
                {
                    Type servicetype = Type.GetType(implementationtype.FullName.Replace(implementationtype.Name, "I" + implementationtype.Name));
                    if (servicetype != null)
                    {
                        services.AddScoped(servicetype, implementationtype); // traditional service interface
                    }
                    else
                    {
                        services.AddScoped(implementationtype, implementationtype); // no interface defined for service
                    }
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
#endif

#if WASM
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<HostContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
                ));
            services.AddDbContext<TenantContext>(options => { });

            services.AddMemoryCache();

            services.AddMvc().AddNewtonsoftJson();

            // register database install/upgrade filter
            services.AddTransient<IStartupFilter, UpgradeFilter>();

            // register singleton scoped core services
            services.AddSingleton<IModuleDefinitionRepository, ModuleDefinitionRepository>();
            services.AddSingleton<IThemeRepository, ThemeRepository>();

            // register transient scoped core services
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<ISiteRepository, SiteRepository>();
            services.AddTransient<IPageRepository, PageRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<IPageModuleRepository, PageModuleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            // get list of loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // get path to /bin
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo folder = new DirectoryInfo(path);
            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("Oqtane.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
                }
            }
            
            // dynamically register module contexts and repository services
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] implementationtypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (Type implementationtype in implementationtypes)
                {
                    Type servicetype = Type.GetType(implementationtype.FullName.Replace(implementationtype.Name, "I" + implementationtype.Name));
                    if (servicetype != null)
                    {
                        services.AddScoped(servicetype, implementationtype); // traditional service interface
                    }
                    else
                    {
                        services.AddScoped(implementationtype, implementationtype); // no interface defined for service
                    }
                }
            }

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            app.UseBlazor<Client.Startup>();
        }
#endif
    }
}
