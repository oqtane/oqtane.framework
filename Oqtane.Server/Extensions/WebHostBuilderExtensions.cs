using System;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using Oqtane.UI;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseRuntime(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                if (!Enum.TryParse(config.GetSection("Runtime").Value, out Runtime runtime))
                {
                    services.AddSingleton<IPlatform>(new Platform()); // Set Runtime to "Server" by default
                }
                else
                {
                    services.AddSingleton<IPlatform>(new Platform(runtime));
                }
            });
        }
    }
}
