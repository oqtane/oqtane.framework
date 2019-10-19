using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Oqtane.Logging
{
    static public class DBLoggerExtensions
    {
        static public ILoggingBuilder AddDBLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DBLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<DBLoggerOptions>, DBLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<DBLoggerOptions>, LoggerProviderOptionsChangeTokenSource<DBLoggerOptions, DBLoggerProvider>>());
            return builder;
        }

        static public ILoggingBuilder AddDBLogger(this ILoggingBuilder builder, Action<DBLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddDBLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
