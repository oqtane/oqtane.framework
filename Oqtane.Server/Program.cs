using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using System.Diagnostics;

namespace Oqtane.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var databaseManager = serviceScope.ServiceProvider.GetService<IDatabaseManager>();
                var install = databaseManager.Install();
                if (!string.IsNullOrEmpty(install.Message))
                {
                    Debug.WriteLine($"Oqtane Error: {install.Message}");
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseStartup<Startup>()
                .ConfigureLocalizationSettings()
                .Build();
    }
}
