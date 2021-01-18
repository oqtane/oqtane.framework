using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Providers;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

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

            // Register localization services
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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
            builder.Services.AddScoped<ILocalizationService, LocalizationService>();
            builder.Services.AddScoped<ILanguageService, LanguageService>();

            await LoadClientAssemblies(httpClient);

            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (var assembly in assemblies)
            {
                // dynamically register module services 
                var implementationTypes = assembly.GetInterfaces<IService>(); 
                foreach (var implementationType in implementationTypes)
                {
                    if (implementationType.AssemblyQualifiedName != null)
                    {
                        var serviceType = Type.GetType(implementationType.AssemblyQualifiedName.Replace(implementationType.Name, $"I{implementationType.Name}"));
                        builder.Services.AddScoped(serviceType ?? implementationType, implementationType);
                    }
                }

                // register client startup services
                var startUps = assembly.GetInstances<IClientStartup>();
                foreach (var startup in startUps)
                {
                    startup.ConfigureServices(builder.Services);
                }
            }

            var host = builder.Build();
            var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
            var interop = new Interop(jsRuntime);
            var localizationCookie = await interop.GetCookie(CookieRequestCultureProvider.DefaultCookieName);
            var culture = CookieRequestCultureProvider.ParseCookieValue(localizationCookie).UICultures[0].Value;
            var localizationService = host.Services.GetRequiredService<ILocalizationService>();
            var cultures = await localizationService.GetCulturesAsync();

            if (culture == null || !cultures.Any(c => c.Name.Equals(culture, StringComparison.OrdinalIgnoreCase)))
            {
                culture = cultures.Single(c => c.IsDefault).Name;
            }

            SetCulture(culture);

            ServiceActivator.Configure(host.Services);

            await host.RunAsync();
        }

        private static async Task LoadClientAssemblies(HttpClient http)
        {
            // get list of loaded assemblies on the client 
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToList();

            // get assemblies from server and load into client app domain
            var zip = await http.GetByteArrayAsync($"/~/api/Installation/load");

            // asemblies and debug symbols are packaged in a zip file
            using (ZipArchive archive = new ZipArchive(new MemoryStream(zip)))
            {
                var dlls = new Dictionary<string, byte[]>();
                var pdbs = new Dictionary<string, byte[]>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!assemblies.Contains(Path.GetFileNameWithoutExtension(entry.Name)))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            byte[] file = memoryStream.ToArray();
                            switch (Path.GetExtension(entry.Name))
                            {
                                case ".dll":
                                    dlls.Add(entry.Name, file);
                                    break;
                                case ".pdb":
                                    pdbs.Add(entry.Name, file);
                                    break;
                            }
                        }
                    }
                }

                foreach (var item in dlls)
                {
                    if (pdbs.ContainsKey(item.Key))
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value), new MemoryStream(pdbs[item.Key]));
                    }
                    else
                    {
                        AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(item.Value));
                    }
                }
            }
        }

        private static void SetCulture(string culture)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
