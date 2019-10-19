using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Oqtane.Logging
{
    internal class Logger : ILogger
    {
        public LoggerProvider Provider { get; private set; }
        public string Category { get; private set; }

        public Logger(LoggerProvider Provider, string Category)
        {
            this.Provider = Provider;
            this.Category = Category;
        }

        IDisposable ILogger.BeginScope<TState>(TState State)
        {
            return Provider.ScopeProvider.Push(State);
        }

        bool ILogger.IsEnabled(LogLevel LogLevel)
        {
            return Provider.IsEnabled(LogLevel);
        }

        void ILogger.Log<TState>(LogLevel LogLevel, EventId EventId, TState State, Exception Exception, Func<TState, Exception, string> Formatter)
        {
            if ((this as ILogger).IsEnabled(LogLevel))
            {

                LogEntry logentry = new LogEntry();
                // we need the TenantId and SiteId 
                logentry.Category = this.Category;
                logentry.Level = LogLevel;
                logentry.Text = Exception?.Message ?? State.ToString();
                logentry.Exception = Exception;
                logentry.EventId = EventId;
                logentry.State = State;

                if (State is string)
                {
                    logentry.StateText = State.ToString();
                }
                else if (State is IEnumerable<KeyValuePair<string, object>> Properties)
                {
                    logentry.StateProperties = new Dictionary<string, object>();

                    foreach (KeyValuePair<string, object> item in Properties)
                    {
                        logentry.StateProperties[item.Key] = item.Value;
                    }
                }

                if (Provider.ScopeProvider != null)
                {
                    Provider.ScopeProvider.ForEachScope((value, loggingProps) =>
                    {
                        if (logentry.Scopes == null)
                        {
                            logentry.Scopes = new List<LogScope>();
                        }

                        LogScope Scope = new LogScope();
                        logentry.Scopes.Add(Scope);

                        if (value is string)
                        {
                            Scope.Text = value.ToString();
                        }
                        else if (value is IEnumerable<KeyValuePair<string, object>> props)
                        {
                            if (Scope.Properties == null)
                                Scope.Properties = new Dictionary<string, object>();

                            foreach (var pair in props)
                            {
                                Scope.Properties[pair.Key] = pair.Value;
                            }
                        }
                    }, State);

                }

                Provider.WriteLog(logentry);
            }
        }

     }
}