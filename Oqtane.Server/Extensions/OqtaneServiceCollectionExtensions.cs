using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Managers;
using Oqtane.Modules;
using Oqtane.Providers;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Radzen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        public static IServiceCollection AddOqtane(this IServiceCollection services, IConfigurationRoot configuration, IWebHostEnvironment environment)
        {
            // process forwarded headers on load balancers and proxy servers
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // register localization services
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddOptions<List<Oqtane.Models.Database>>().Bind(configuration.GetSection(SettingKeys.AvailableDatabasesSection));

            // register scoped core services
            services.AddScoped<IAuthorizationHandler, PermissionHandler>()
                .AddOqtaneServerScopedServices();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // setup HttpClient for server side in a client side compatible fashion ( with auth cookie )
            services.AddHttpClients();

            // register singleton scoped core services
            services.AddSingleton(configuration)
                .AddOqtaneSingletonServices();

            // install any modules or themes ( this needs to occur BEFORE the assemblies are loaded into the app domain )
            InstallationManager.InstallPackages(environment.WebRootPath, environment.ContentRootPath);

            // register transient scoped core services
            services.AddOqtaneTransientServices();

            // load the external assemblies into the app domain, install services
            services.AddOqtaneAssemblies();
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

            services.ConfigureOqtaneIdentityOptions(configuration);

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
            services.ConfigureOqtaneAuthenticationOptions(configuration);

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
                   if (environment.IsDevelopment())
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
            services.TryAddSwagger(configuration);

            return services;
        }

        public static IServiceCollection AddOqtaneAssemblies(this IServiceCollection services)
        {
            LoadAssemblies();
            LoadSatelliteAssemblies();
            services.AddOqtaneServices();

            return services;
        }

        public static IServiceCollection AddOqtaneDbContext(this IServiceCollection services)
        {
            services.AddDbContext<MasterDBContext>(options => { }, ServiceLifetime.Transient);
            services.AddDbContext<TenantDBContext>(options => { }, ServiceLifetime.Transient);
            services.AddDbContextFactory<TenantDBContext>(opt => { }, ServiceLifetime.Transient);
            return services;
        }

        public static OqtaneSiteOptionsBuilder AddOqtaneSiteOptions(this IServiceCollection services)
        {
            return new OqtaneSiteOptionsBuilder(services);
        }

        public static IServiceCollection AddOqtaneSingletonServices(this IServiceCollection services)
        {
            services.AddSingleton<IInstallationManager, InstallationManager>();
            services.AddSingleton<ISyncManager, SyncManager>();
            services.AddSingleton<IDatabaseManager, DatabaseManager>();
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            services.AddSingleton<AutoValidateAntiforgeryTokenFilter>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IServerStateManager, ServerStateManager>();
            return services;
        }

        public static IServiceCollection AddOqtaneServerScopedServices(this IServiceCollection services)
        {
            services.AddScoped<Oqtane.Shared.SiteState>();
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddScoped<IModuleDefinitionService, ModuleDefinitionService>();
            services.AddScoped<IThemeService, Oqtane.Services.ThemeService>();
            services.AddScoped<IAliasService, AliasService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IPageModuleService, PageModuleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IJobLogService, JobLogService>();
            services.AddScoped<INotificationService, Oqtane.Services.NotificationService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISiteTemplateService, SiteTemplateService>();
            services.AddScoped<ISqlService, SqlService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IUrlMappingService, UrlMappingService>();
            services.AddScoped<IVisitorService, VisitorService>();
            services.AddScoped<ISyncService, SyncService>();
            services.AddScoped<ISearchResultsService, SearchResultsService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ISearchProvider, DatabaseSearchProvider>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ICookieConsentService, ServerCookieConsentService>();
            services.AddScoped<ITimeZoneService, TimeZoneService>();
            services.AddScoped<IMigrationHistoryService, MigrationHistoryService>();

            // providers
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.QuillJSTextEditor>();
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.TextAreaTextEditor>();
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.RadzenTextEditor>();

            services.AddRadzenComponents();

            var localizer = services.BuildServiceProvider().GetService<IStringLocalizer<Oqtane.Modules.Controls.RadzenTextEditor>>();
            Oqtane.Modules.Controls.RadzenEditorDefinitions.Localizer = localizer;

            return services;
        }

        public static IServiceCollection AddOqtaneTransientServices(this IServiceCollection services)
        {
            // services
            services.AddTransient<ISiteService, ServerSiteService>();
            services.AddTransient<ILocalizationCookieService, ServerLocalizationCookieService>();
            services.AddTransient<IOutputCacheService, ServerOutputCacheService>();

            // repositories
            services.AddTransient<IModuleDefinitionRepository, ModuleDefinitionRepository>();
            services.AddTransient<IThemeRepository, ThemeRepository>();
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
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<IJobLogRepository, JobLogRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IFolderRepository, FolderRepository>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<ISiteTemplateRepository, SiteTemplateRepository>();
            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<ILanguageRepository, LanguageRepository>();
            services.AddTransient<IVisitorRepository, VisitorRepository>();
            services.AddTransient<IUrlMappingRepository, UrlMappingRepository>();
            services.AddTransient<ISearchContentRepository, SearchContentRepository>();
            services.AddTransient<IMigrationHistoryRepository, MigrationHistoryRepository>();

            // managers
            services.AddTransient<IDBContextDependencies, DBContextDependencies>();
            services.AddTransient<ITenantManager, TenantManager>();
            services.AddTransient<IAliasAccessor, AliasAccessor>();
            services.AddTransient<IUserPermissions, UserPermissions>();
            services.AddTransient<ITenantResolver, TenantResolver>();
            services.AddTransient<IJwtManager, JwtManager>();
            services.AddTransient<ILogManager, LogManager>();
            services.AddTransient<IUpgradeManager, UpgradeManager>();
            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<ILocalizationManager, LocalizationManager>();
            services.AddTransient<ITokenReplace, TokenReplace>();

            // obsolete
            services.AddTransient<ITenantResolver, TenantResolver>(); // replaced by ITenantManager

            return services;
        }

        public static IServiceCollection ConfigureOqtaneCookieOptions(this IServiceCollection services)
        {
            // note that ConfigureApplicationCookie internally uses an ApplicationScheme of "Identity.Application"
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/login"; // overrides .NET Identity default of /Account/Login
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogout = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = PrincipalValidator.ValidateAsync;
            });

            return services;
        }

        public static IServiceCollection ConfigureOqtaneAuthenticationOptions(this IServiceCollection services, IConfigurationRoot Configuration)
        {
            // prevent remapping of claims
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // settings defined in appsettings
            services.Configure<OAuthOptions>(Configuration);
            services.Configure<OpenIdConnectOptions>(Configuration);

            return services;
        }

        public static IServiceCollection ConfigureOqtaneIdentityOptions(this IServiceCollection services, IConfigurationRoot Configuration)
        {
            // default settings
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

                // SignIn settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // User settings
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            });

            // overrides defined in appsettings
            services.Configure<IdentityOptions>(Configuration);

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                services.AddScoped(provider =>
                {
                    var client = new HttpClient(new HttpClientHandler { UseCookies = false });
                    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                    if (httpContextAccessor.HttpContext != null)
                    {
                        client.BaseAddress = new Uri(httpContextAccessor.HttpContext.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host);
                        // set the cookies to allow HttpClient API calls to be authenticated
                        foreach (var cookie in httpContextAccessor.HttpContext.Request.Cookies)
                        {
                            client.DefaultRequestHeaders.Add("Cookie", cookie.Key + "=" + WebUtility.UrlEncode(cookie.Value));
                        }
                    }

                    return client;
                });
            }

            // register a named IHttpClientFactory
            services.AddHttpClient("oqtane", (provider, client) =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                if (httpContextAccessor.HttpContext != null)
                {
                    client.BaseAddress = new Uri(httpContextAccessor.HttpContext.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host);
                    // set the cookies to allow HttpClient API calls to be authenticated
                    foreach (var cookie in httpContextAccessor.HttpContext.Request.Cookies)
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookie.Key + "=" + WebUtility.UrlEncode(cookie.Value));
                    }
                }
            });

            // IHttpClientFactory for calling remote services via RemoteServiceBase (not named = default)
            services.AddHttpClient();

            return services;
        }

        public static IServiceCollection TryAddSwagger(this IServiceCollection services, IConfigurationRoot configuration)
        {
            if (configuration.GetSection("UseSwagger").Value != "false")
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(Constants.Version, new OpenApiInfo { Title = Constants.PackageId, Version = Constants.Version });
                });
            }

            return services;
        }

        private static IServiceCollection AddOqtaneServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var hostedServiceType = typeof(IHostedService);

            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // dynamically register module scoped services (ie. client service classes)
                var implementationTypes = assembly.GetInterfaces<IService>();
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        services.AddScoped(serviceType ?? implementationType, implementationType);
                    }
                }

                // dynamically register module transient services (ie. server DBContext, repository classes)
                implementationTypes = assembly.GetInterfaces<ITransientService>();
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        services.AddTransient(serviceType ?? implementationType, implementationType);
                    }
                }

                // dynamically register hosted services
                var serviceTypes = assembly.GetTypes(hostedServiceType);
                foreach (var serviceType in serviceTypes)
                {
                    if (!services.Any(item => item.ServiceType == serviceType))
                    {
                        services.AddSingleton(hostedServiceType, serviceType);
                    }
                }

                // dynamically register server startup services
                assembly.GetInstances<IServerStartup>()
                    .ToList()
                    .ForEach(x => x.ConfigureServices(services));

                // dynamically register client startup services (these services will only be used when running on Blazor Server)
                assembly.GetInstances<IClientStartup>()
                    .ToList()
                    .ForEach(x => x.ConfigureServices(services));
            }
            return services;
        }

        private static void LoadAssemblies()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null) return;

            AssemblyLoadContext.Default.Resolving += ResolveDependencies;

            var assembliesFolder = new DirectoryInfo(assemblyPath);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // iterate through Oqtane assemblies in /bin ( filter is narrow to optimize loading process )
            foreach (var dll in assembliesFolder.EnumerateFiles($"*.dll", SearchOption.TopDirectoryOnly).Where(f => f.IsOqtaneAssembly()))
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(dll.FullName);
                }
                catch
                {
                    Debug.WriteLine($"Oqtane Error: Cannot Get Assembly Name For {dll.Name}");
                    continue;
                }

                if (!assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())))
                {
                    AssemblyLoadContext.Default.LoadOqtaneAssembly(dll, assemblyName);
                }
            }
        }

        private static void LoadSatelliteAssemblies()
        {
            AssemblyLoadContext.Default.Resolving += ResolveDependencies;

            var installedCultures = LocalizationManager.GetSatelliteAssemblyCultures();
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"*{Constants.SatelliteAssemblyExtension}", SearchOption.AllDirectories))
            {
                var code = Path.GetFileName(Path.GetDirectoryName(file));
                if (installedCultures.Contains(code))
                {
                    try
                    {
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(System.IO.File.ReadAllBytes(file)));
                        Debug.WriteLine($"Oqtane Info: Loaded Satellite Assembly {file}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Oqtane Error: Unable To Load Satellite Assembly {file} - {ex}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Oqtane Error: Culture Not Supported For Satellite Assembly {file}");
                }
            }
        }

        private static Assembly ResolveDependencies(AssemblyLoadContext context, AssemblyName name)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + Path.DirectorySeparatorChar + name.Name + ".dll";
            if (System.IO.File.Exists(assemblyPath))
            {
                return context.LoadFromStream(new MemoryStream(System.IO.File.ReadAllBytes(assemblyPath)));
            }
            else
            {
                return null;
            }
        }

    }
}
