using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
// DO NOT REMOVE - needed for client-side Blazor
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;

namespace Oqtane.Server
{
    public class Program
    {
#if DEBUG || RELEASE
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var manager = serviceScope.ServiceProvider.GetService<DatabaseManager>();
                manager.StartupMigration();
            }
            //DatabaseManager.StartupMigration();                    
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
#endif

#if WASM
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseStartup<Startup>()
                .Build();
#endif

    }
}
