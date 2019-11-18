using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression; // needed for WASM
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
using System.IO;
using System.Runtime.Loader;
using Oqtane.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Oqtane.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Infrastructure;

namespace Oqtane.Server
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

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
#if DEBUG
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
#endif
#if RELEASE
            services.AddServerSideBlazor();
#endif
            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                services.AddScoped<HttpClient>(s =>
                {
                    // creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var NavigationManager = s.GetRequiredService<NavigationManager>();
                    var httpContextAccessor = s.GetRequiredService<IHttpContextAccessor>();
                    var authToken = httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
                    var client = new HttpClient(new HttpClientHandler { UseCookies = false });
                    if (authToken != null)
                    {
                        client.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Identity.Application=" + authToken);
                    }
                    client.BaseAddress = new Uri(NavigationManager.Uri);
                    return client;
                });
            }

            // register authorization services
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("ViewPage", policy => policy.Requirements.Add(new PermissionRequirement("Page", "View")));
                options.AddPolicy("EditPage", policy => policy.Requirements.Add(new PermissionRequirement("Page", "Edit")));
                options.AddPolicy("ViewModule", policy => policy.Requirements.Add(new PermissionRequirement("Module", "View")));
                options.AddPolicy("EditModule", policy => policy.Requirements.Add(new PermissionRequirement("Module", "Edit")));
            });

            // register scoped core services
            services.AddScoped<SiteState>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddScoped<IModuleDefinitionService, ModuleDefinitionService>();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<IAliasService, AliasService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IPageModuleService, PageModuleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IJobLogService, JobLogService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<MasterDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
                ));
            services.AddDbContext<TenantDBContext>(options => { });

            services.AddIdentityCore<IdentityUser>(options => { })
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            
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

            // register singleton scoped core services
            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSingleton<IInstallationManager, InstallationManager>();

            // register transient scoped core services
            services.AddTransient<IModuleDefinitionRepository, ModuleDefinitionRepository>();
            services.AddTransient<IThemeRepository, ThemeRepository>();
            services.AddTransient<IUserPermissions, UserPermissions>();
            services.AddTransient<ITenantResolver, TenantResolver>();
            services.AddTransient<IAliasRepository, AliasRepository>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<ISiteRepository, SiteRepository>();
            services.AddTransient<IPageRepository, PageRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<IPageModuleRepository, PageModuleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IProfileRepository, ProfileRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IPermissionRepository, PermissionRepository>();
            services.AddTransient<ISettingRepository, SettingRepository>();
            services.AddTransient<ILogRepository, LogRepository>();
            services.AddTransient<ILogManager, LogManager>();
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<IJobLogRepository, JobLogRepository>();

            // get list of loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo folder = new DirectoryInfo(path);
            List<Assembly> moduleassemblies = new List<Assembly>();

            // iterate through Oqtane module assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("*.Module.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                    moduleassemblies.Add(assembly);
                }
            }

            // iterate through Oqtane theme assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("*.Theme.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                }
            }

            services.AddMvc().AddModuleAssemblies(moduleassemblies).AddNewtonsoftJson();

            // dynamically register module services, contexts, and repository classes
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                Type[] implementationtypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (Type implementationtype in implementationtypes)
                {
                    Type servicetype = Type.GetType(implementationtype.AssemblyQualifiedName.Replace(implementationtype.Name, "I" + implementationtype.Name));
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

            // dynamically register hosted services
            foreach (Assembly assembly in assemblies)
            {
                Type[] servicetypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IHostedService)))
                    .ToArray();
                foreach (Type servicetype in servicetypes)
                {
                    if (servicetype.Name != "HostedServiceBase")
                    {
                        services.AddSingleton(typeof(IHostedService), servicetype);
                    }
                }
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Oqtane", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IInstallationManager InstallationManager)
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

            // install any modules or themes
            InstallationManager.InstallPackages("Modules,Themes", false);

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oqtane V1");
            });

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
            // register authorization services
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("ViewPage", policy => policy.Requirements.Add(new PermissionRequirement("Page", "View")));
                options.AddPolicy("EditPage", policy => policy.Requirements.Add(new PermissionRequirement("Page", "Edit")));
                options.AddPolicy("ViewModule", policy => policy.Requirements.Add(new PermissionRequirement("Module", "View")));
                options.AddPolicy("EditModule", policy => policy.Requirements.Add(new PermissionRequirement("Module", "Edit")));
            });

            // register scoped core services
            services.AddScoped<SiteState>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<MasterDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
                ));
            services.AddDbContext<TenantDBContext>(options => { });

            services.AddIdentityCore<IdentityUser>(options => { })
                .AddEntityFrameworkStores<TenantDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

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

            // register singleton scoped core services
            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSingleton<IInstallationManager, InstallationManager>();

            // register transient scoped core services
            services.AddTransient<IModuleDefinitionRepository, ModuleDefinitionRepository>();
            services.AddTransient<IThemeRepository, ThemeRepository>();
            services.AddTransient<IUserPermissions, UserPermissions>();
            services.AddTransient<ITenantResolver, TenantResolver>();
            services.AddTransient<IAliasRepository, AliasRepository>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<ISiteRepository, SiteRepository>();
            services.AddTransient<IPageRepository, PageRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<IPageModuleRepository, PageModuleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IProfileRepository, ProfileRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IPermissionRepository, PermissionRepository>();
            services.AddTransient<ISettingRepository, SettingRepository>();
            services.AddTransient<ILogRepository, LogRepository>();
            services.AddTransient<ILogManager, LogManager>();
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<IJobLogRepository, JobLogRepository>();

            // get list of loaded assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo folder = new DirectoryInfo(path);
            List<Assembly> moduleassemblies = new List<Assembly>();

            // iterate through Oqtane module assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("*.Module.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                }
            }

            // iterate through Oqtane theme assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (FileInfo file in folder.EnumerateFiles("*.Theme.*.dll"))
            {
                // check if assembly is already loaded
                Assembly assembly = assemblies.Where(item => item.Location == file.FullName).FirstOrDefault();
                if (assembly == null)
                {
                    // load assembly from stream to prevent locking file ( as long as dependencies are in /bin they will load as well )
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(file.FullName)));
                }
            }

            services.AddMvc().AddModuleAssemblies(moduleassemblies).AddNewtonsoftJson();
           
            // dynamically register module services, contexts, and repository classes
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName.StartsWith("Oqtane.") || item.FullName.Contains(".Module.")).ToArray();
            foreach (Assembly assembly in assemblies)
            {
                Type[] implementationtypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)))
                    .ToArray();
                foreach (Type implementationtype in implementationtypes)
                {
                    Type servicetype = Type.GetType(implementationtype.AssemblyQualifiedName.Replace(implementationtype.Name, "I" + implementationtype.Name));
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

            // dynamically register hosted services
            foreach (Assembly assembly in assemblies)
            {
                Type[] servicetypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IHostedService)))
                    .ToArray();
                foreach (Type servicetype in servicetypes)
                {
                    if (servicetype.Name != "HostedServiceBase")
                    {
                        services.AddSingleton(typeof(IHostedService), servicetype);
                    }
                }
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Oqtane", Version = "v1" });
            });

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IInstallationManager InstallationManager)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            // install any modules or themes
            InstallationManager.InstallPackages("Modules,Themes", false);

            app.UseClientSideBlazorFiles<Client.Startup>();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oqtane V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }
#endif

    }
}
