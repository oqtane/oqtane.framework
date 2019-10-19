using Microsoft.Extensions.Logging;

namespace Oqtane.Logging
{
    public class DBLoggerOptions
    {
        public DBLoggerOptions()
        {
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
