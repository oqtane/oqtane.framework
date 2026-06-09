using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Oqtane.Infrastructure
{
    /// <summary>
    /// FileLogger should only be used in scenarios where a database is not available or tenant/site cannot be determined (ie. during startup)
    /// </summary>
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigurationRoot _config;

        public FileLoggerProvider(IWebHostEnvironment environment, IConfigurationRoot config)
        {
            _environment = environment;
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this, _environment, _config);
        }

        public void Dispose()
        {
            // nothing to dispose
        }
    }
}
