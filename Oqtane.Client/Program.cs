using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Oqtane.Services;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
            HttpClient httpClient = new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};

            builder.Services.AddSingleton(httpClient);
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

            await LoadClientAssemblies(httpClient);

            // dynamically register module contexts and repository services
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var implementationTypes = assembly.GetTypes()
                    .Where(item => item.GetInterfaces().Contains(typeof(IService)));

                foreach (Type implementationtype in implementationTypes)
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

                assembly.GetInstances<IClientStartup>()
                    .ToList()
                    .ForEach(x => x.ConfigureServices(builder.Services));
            }

            await builder.Build().RunAsync();
        }

        private static async Task LoadClientAssemblies(HttpClient http)
        {
            var list = await http.GetFromJsonAsync<List<string>>($"/~/api/ModuleDefinition/load");
            // get list of loaded assemblies on the client ( in the client-side hosting module the browser client has its own app domain )
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToList();
            foreach (var name in list)
            {
                if (assemblyList.Contains(name)) continue;
                // download assembly from server and load
                var bytes = await http.GetByteArrayAsync($"/~/api/ModuleDefinition/load/{name}.dll");
                Assembly.Load(bytes);
            }
        }
    }
}
