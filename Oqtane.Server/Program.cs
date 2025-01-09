using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Microsoft.Extensions.Logging;
using Oqtane.Documentation;
using Oqtane.Extensions;

namespace Oqtane.Server
{
    [PrivateApi("Mark Entry-Program as private, since it's not very useful in the public docs")]
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            ConfigUtilities.Configure(host.Services);
            var databaseManager = host.Services.GetService<IDatabaseManager>();
            var install = databaseManager.Install();
            if (!string.IsNullOrEmpty(install.Message))
            {
                var filelogger = host.Services.GetRequiredService<ILogger<Program>>();
                if (filelogger != null)
                {
                    filelogger.LogError($"[Oqtane.Server.Program.Main] {install.Message}");
                }
            }
            host.Run();
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
