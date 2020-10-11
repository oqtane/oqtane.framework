using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var httpClient = new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};
            
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton(httpClient);
            builder.Services.AddOptions();

            // Register localization services
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddOqtaneAuthentication();

            // Register oqtane services
            builder.Services.AddOqtaneServices();
            await builder.Services.AddOqtaneClientServices(httpClient);

            await builder.Build().RunAsync();
        }
    }
}
