using System;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public interface ILogManager
    {
        void Log(LogLevel level, object @class, LogFunction function, string message, params object[] args);
        void Log(LogLevel level, object @class, LogFunction function, Exception exception, string message, params object[] args);
        void Log(int siteId, LogLevel level, object @class, LogFunction function, string message, params object[] args);
        void Log(int siteId, LogLevel level, object @class, LogFunction function, Exception exception, string message, params object[] args);
        void Log(Log log);
    }
}
