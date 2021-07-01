using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Oqtane.Infrastructure
{
    /// <summary>
    /// FileLogger should only be used in scenarios where a database is not available or tenant/site cannot be determined ( ie. during startup )
    /// </summary>
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;

        public FileLoggerProvider(IWebHostEnvironment environment, IConfigManager configManager)
        {
            _environment = environment;
            _configManager = configManager;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this, _environment, _configManager);
        }

        public void Dispose()
        {
        }
    }
}
