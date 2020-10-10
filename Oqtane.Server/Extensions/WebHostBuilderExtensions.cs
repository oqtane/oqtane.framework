using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;

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
    }
}
