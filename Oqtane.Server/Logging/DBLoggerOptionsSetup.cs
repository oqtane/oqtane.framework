using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Configuration;

namespace Oqtane.Logging
{
    internal class DBLoggerOptionsSetup : ConfigureFromConfigurationOptions<DBLoggerOptions>
    {
        public DBLoggerOptionsSetup(ILoggerProviderConfiguration<DBLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration) {}
    }
}
