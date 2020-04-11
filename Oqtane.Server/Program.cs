using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
// DO NOT REMOVE - needed for client-side Blazor
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;

namespace Oqtane.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var installationManager = serviceScope.ServiceProvider.GetService<IInstallationManager>();
                // install any modules or themes stored in nugget, then restart app to ensure all is loaded in order
                installationManager.InstallPackages("Modules,Themes", true);
                var databaseManager = serviceScope.ServiceProvider.GetService<DatabaseManager>();
                databaseManager.StartupMigration();
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseStartup<Startup>()
                .Build();
    }
}
