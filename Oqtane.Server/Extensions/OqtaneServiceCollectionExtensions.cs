using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Oqtane.Infrastructure;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        public static IServiceCollection AddOqtaneAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("ViewPage", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Page, PermissionNames.View)));
                options.AddPolicy("EditPage", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Page, PermissionNames.Edit)));
                options.AddPolicy("ViewModule", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Module, PermissionNames.View)));
                options.AddPolicy("EditModule", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Module, PermissionNames.Edit)));
                options.AddPolicy("ViewFolder", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Folder, PermissionNames.View)));
                options.AddPolicy("EditFolder", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Folder, PermissionNames.Edit)));
                options.AddPolicy("ListFolder", policy => policy.Requirements.Add(new PermissionRequirement(EntityNames.Folder, PermissionNames.Browse)));
            });

            return services;
        }

        public static IServiceCollection AddOqtane(this IServiceCollection services, Runtime runtime, string[] supportedCultures)
        {
            LoadAssemblies();
            LoadSatelliteAssemblies(supportedCultures);
            services.AddOqtaneServices(runtime);

            return services;
        }


        public static IServiceCollection AddOqtaneTransientServices(this IServiceCollection services)
        {
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
            services.AddTransient<ILocalizationManager, LocalizationManager>();
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<IJobLogRepository, JobLogRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IFolderRepository, FolderRepository>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<ISiteTemplateRepository, SiteTemplateRepository>();
            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<IUpgradeManager, UpgradeManager>();

            return services;
        }

        public static IServiceCollection AddOqtaneSwaggerDocs(this IServiceCollection services, Action<OqtaneSwaggerOptions> optionsSetup)
        {
            var swaggerOptions = new OqtaneSwaggerOptions();
            optionsSetup.Invoke(swaggerOptions);

            if (swaggerOptions.Enable)
            {
                services.AddSwaggerGen(c => { c.SwaggerDoc(swaggerOptions.Name, new OpenApiInfo { Title = swaggerOptions.Title, Version = swaggerOptions.Version }); });
            }

            return services;
        }

        private static IServiceCollection AddOqtaneServices(this IServiceCollection services, Runtime runtime)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var hostedServiceType = typeof(IHostedService);
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // dynamically register module services, contexts, and repository classes
                var implementationTypes = assembly.GetInterfaces<IService>();
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        services.AddScoped(serviceType ?? implementationType, implementationType);
                    }
                }

                // dynamically register hosted services
                var serviceTypes = assembly.GetTypes(hostedServiceType);
                foreach (var serviceType in serviceTypes)
                {
                    if (serviceType.IsSubclassOf(typeof(HostedServiceBase)))
                    {
                        services.AddSingleton(hostedServiceType, serviceType);
                    }
                }

                var startUps = assembly.GetInstances<IServerStartup>();
                foreach (var startup in startUps)
                {
                    startup.ConfigureServices(services);
                }

                if (runtime == Runtime.Server)
                {
                    assembly.GetInstances<IClientStartup>()
                        .ToList()
                        .ForEach(x => x.ConfigureServices(services));
                }
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
                    Console.WriteLine($"Not Assembly : {dll.Name}");
                    continue;
                }

                if (!assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())))
                {
                    try
                    {
                        var pdb = Path.ChangeExtension(dll.FullName, ".pdb");
                        Assembly assembly = null;

                        // load assembly ( and symbols ) from stream to prevent locking files ( as long as dependencies are in /bin they will load as well )
                        if (File.Exists(pdb))
                        {
                            assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)), new MemoryStream(File.ReadAllBytes(pdb)));
                        }
                        else
                        {
                            assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll.FullName)));
                        }
                        Console.WriteLine($"Loaded : {assemblyName}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed : {assemblyName}\n{e}");
                    }
                }
            }
        }

        private static void LoadSatelliteAssemblies(string[] supportedCultures)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (assemblyPath == null)
            {
                return;
            }

            AssemblyLoadContext.Default.Resolving += ResolveDependencies;

            foreach (var culture in supportedCultures)
            {
                if (culture == Constants.DefaultCulture)
                {
                    continue;
                }

                var assembliesFolder = new DirectoryInfo(Path.Combine(assemblyPath, culture));
                if (assembliesFolder.Exists)
                {
                    foreach (var assemblyFile in assembliesFolder.EnumerateFiles(Constants.StalliteAssemblyExtension))
                    {
                        AssemblyName assemblyName;
                        try
                        {
                            assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                        }
                        catch
                        {
                            Console.WriteLine($"Not Satellite Assembly : {assemblyFile.Name}");
                            continue;
                        }

                        try
                        {
                            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyFile.FullName)));
                            Console.WriteLine($"Loaded : {assemblyName}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed : {assemblyName}\n{e}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"The satellite assemblies folder named '{culture}' is not found.");
                }
            }
        }

        private static Assembly ResolveDependencies(AssemblyLoadContext context, AssemblyName name)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\" + name.Name + ".dll";
            if (File.Exists(assemblyPath))
            {
                return context.LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyPath)));
            }
            else
            {
                return null;
            }
        }

    }
}
