using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System;

namespace Oqtane.Server
{
    public class Program
    {
#if DEBUG || RELEASE
        public static void Main(string[] args)
        {
            PrepareConfiguration();
            CreateHostBuilder(args).Build().Run();
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
            PrepareConfiguration();
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

        private static void PrepareConfiguration()
        {
            string config = "";
            using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\appsettings.json"))
            {
                config = reader.ReadToEnd();
            }
            // if using LocalDB create a unique database name
            if (config.Contains("AttachDbFilename=|DataDirectory|\\\\Oqtane.mdf"))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
                config = config.Replace("Initial Catalog=Oqtane", "Initial Catalog=Oqtane-" + timestamp)
                    .Replace("AttachDbFilename=|DataDirectory|\\\\Oqtane.mdf", "AttachDbFilename=|DataDirectory|\\\\Oqtane-" + timestamp + ".mdf");
                using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                {
                    writer.WriteLine(config);
                }
            }
        }
    }
}
