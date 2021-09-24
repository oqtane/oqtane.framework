using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Oqtane.Infrastructure
{
    public class FileLogger : ILogger
    {
        protected readonly FileLoggerProvider _FileLoggerProvider;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;

        public FileLogger(FileLoggerProvider FileLoggerProvider, IWebHostEnvironment environment,IConfigManager configManager)
        {
            _FileLoggerProvider = FileLoggerProvider;
            _environment = environment;
            _configManager = configManager;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_configManager.GetSetting("Logging:FileLogger:LogLevel:Default", "") == "")
            {
                _configManager.AddOrUpdateSetting("Logging:FileLogger:LogLevel:Default", "Error", true);
                if (logLevel < LogLevel.Error)
                {
                    return;
                }
            }

            if (!IsEnabled(logLevel))
            {
                return;
            }

            string folder = Path.Combine(_environment.ContentRootPath, "Content", "Log");
            var filepath = Path.Combine(folder, "error.log");
            long size = 0;

            // ensure directory exists
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(filepath))
            {
                var fileinfo = new FileInfo(filepath);
                size = fileinfo.Length;

                // only retain an error log for the current day as it is intended for development purposes
                if (fileinfo.LastWriteTimeUtc.Date < DateTime.UtcNow.Date)
                {
                    File.Delete(filepath);
                }
            }

            // do not allow the log to exceed 1 MB
            if (size < 1000000)
            {
                try
                {
                    var logentry = string.Format("{0} [{1}] {2} {3}", "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]", logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : "");

                    using (var streamWriter = new StreamWriter(filepath, true))
                    {
                        streamWriter.WriteLine(logentry);
                    }
                }
                catch
                {
                    // error occurred
                }
            }
        }
    }
}
