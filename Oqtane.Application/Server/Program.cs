using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oqtane.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Application.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // defer server startup to Oqtane - do not modify
            var host = BuildWebHost(args);
            var databaseManager = host.Services.GetService<IDatabaseManager>();
            var install = databaseManager.Install();
            if (!string.IsNullOrEmpty(install.Message))
            {
                var filelogger = host.Services.GetRequiredService<ILogger<Program>>();
                if (filelogger != null)
                {
                    filelogger.LogError($"[Oqtane.Application.Server.Program.Main] {install.Message}");
                }
            }
            else
            {
                host.Run();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                    .Build())
                .UseStartup<Startup>()
                .ConfigureLocalizationSettings()
                .Build();
    }
}
