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
        private readonly IContentManager _contentManager;
        private readonly IConfigManager _configManager;

        public FileLoggerProvider(IContentManager contentManager, IConfigManager configManager)
        {
            _contentManager = contentManager;
            _configManager = configManager;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this, _contentManager, _configManager);
        }

        public void Dispose()
        {
        }
    }
}
