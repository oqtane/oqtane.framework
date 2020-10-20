using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureLocalizationSettings(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                services.Configure<LocalizationOptions>(config.GetSection(SettingKeys.LocalizationSection));
                services.AddSingleton(ctx => ctx.GetService<IOptions<LocalizationOptions>>().Value);
                services.AddTransient<ILocalizationManager, LocalizationManager>();
            });
        }
    }
}
