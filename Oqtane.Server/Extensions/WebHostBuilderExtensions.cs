using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.UI;
using System;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureLocalizationSettings(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                services.Configure<LocalizationOptions>(config.GetSection("Localization"));
                services.AddSingleton(ctx => ctx.GetService<IOptions<LocalizationOptions>>().Value);
                services.AddTransient<ILocalizationManager, LocalizationManager>();
            });
        }

        public static IWebHostBuilder UseRuntime(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                services.Configure<PlateformOptions>(options => {
                    if (Enum.TryParse(config.GetSection("Runtime").Value, out Runtime runtime))
                    {
                        options.Runtime = runtime;
                    }
                    else
                    {
                        options.Runtime = Runtime.Server; // Set Runtime to "Server" by default
                    }
                });
                services.AddSingleton(ctx => ctx.GetService<IOptions<PlateformOptions>>().Value);
            });
        }
    }
}
