using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Oqtane.Services;
using System.Reflection;
using System;
using System.Linq;
using Oqtane.Modules;
using Oqtane.Shared;
using Oqtane.Providers;
using Microsoft.AspNetCore.Components.Authorization;

namespace Oqtane.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddBaseAddressHttpClient();
            builder.Services.AddOptions();

            // register auth services
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());

            // register scoped core services
            builder.Services.AddScoped<SiteState>();
            builder.Services.AddScoped<IInstallationService, InstallationService>();
            builder.Services.AddScoped<IModuleDefinitionService, ModuleDefinitionService>();
            builder.Services.AddScoped<IThemeService, ThemeService>();
            builder.Services.AddScoped<IAliasService, AliasService>();
            builder.Services.AddScoped<ITenantService, TenantService>();
            builder.Services.AddScoped<ISiteService, SiteService>();
            builder.Services.AddScoped<IPageService, PageService>();
            builder.Services.AddScoped<IModuleService, ModuleService>();
            builder.Services.AddScoped<IPageModuleService, PageModuleService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUserRoleService, UserRoleService>();
            builder.Services.AddScoped<ISettingService, SettingService>();
            builder.Services.AddScoped<IPackageService, PackageService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IJobLogService, JobLogService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFolderService, FolderService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<ISiteTemplateService, SiteTemplateService>();
            builder.Services.AddScoped<ISqlService, SqlService>();
            builder.Services.AddScoped<ISystemService, SystemService>();

            // dynamically register module contexts and repository services
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
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
                        builder.Services.AddScoped(servicetype, implementationtype); // traditional service interface
                    }
                    else
                    {
                        builder.Services.AddScoped(implementationtype, implementationtype); // no interface defined for service
                    }
                }
            }

            await builder.Build().RunAsync();
        }
    }
}
