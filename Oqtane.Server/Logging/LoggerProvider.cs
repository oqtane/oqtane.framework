using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Oqtane.Logging
{
    public abstract class LoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
    {
        ConcurrentDictionary<string, Logger> loggers = new ConcurrentDictionary<string, Logger>();
        IExternalScopeProvider fScopeProvider;
        protected IDisposable SettingsChangeToken;

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider ScopeProvider)
        {
            fScopeProvider = ScopeProvider;
        }

        ILogger ILoggerProvider.CreateLogger(string Category)
        {
            return loggers.GetOrAdd(Category,
            (category) => {
                return new Logger(this, category);
            });
        }

        void IDisposable.Dispose()
        {
            if (!this.IsDisposed)
            {
                try
                {
                    Dispose(true);
                }
                catch
                {
                }

                this.IsDisposed = true;
                GC.SuppressFinalize(this);                
            }
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (SettingsChangeToken != null)
            {
                SettingsChangeToken.Dispose();
                SettingsChangeToken = null;
            }
        }

        public LoggerProvider()
        {
        }

        ~LoggerProvider()
        {
            if (!this.IsDisposed)
            {
                Dispose(false);
            }
        }

        public abstract bool IsEnabled(LogLevel LogLevel);

        public abstract void WriteLog(LogEntry LogEntry);

        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (fScopeProvider == null)
                    fScopeProvider = new LoggerExternalScopeProvider();
                return fScopeProvider;
            }
        }

        public bool IsDisposed { get; protected set; }
    }
}