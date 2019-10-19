using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Logging
{
    [Microsoft.Extensions.Logging.ProviderAlias("DB")]
    public class DBLoggerProvider : LoggerProvider
    {
        bool Terminated;
        ConcurrentQueue<LogEntry> LogQueue = new ConcurrentQueue<LogEntry>();
        private Tenant tenant;
        internal DBLoggerOptions Settings { get; private set; }

        public DBLoggerProvider(ITenantResolver TenantResolver)
        {
            tenant = TenantResolver.GetTenant();
        }

        public override void WriteLog(LogEntry LogEntry)
        {
            // enrich with tenant information
            LogQueue.Enqueue(LogEntry);
        }

        void ThreadProc()
        {
            Task.Run(() => {

                while (!Terminated)
                {
                    try
                    {
                        AddLogEntry();
                        System.Threading.Thread.Sleep(100);
                    }
                    catch 
                    {
                    }
                }

            });
        }

        void AddLogEntry()
        {
            // check the LogQueue.Count to determine if a threshold has been reached
            // then dequeue the items into temp storage based on TenantId and bulk insert them into the database
            LogEntry logentry = null;
            if (LogQueue.TryDequeue(out logentry))
            {
                // convert logentry object to object which can be stored in database
            }

        }

        protected override void Dispose(bool Disposing)
        {
            Terminated = true;
            base.Dispose(Disposing);
        }

        public DBLoggerProvider(IOptionsMonitor<DBLoggerOptions> Settings)
            : this(Settings.CurrentValue)
        {
            SettingsChangeToken = Settings.OnChange(settings => {
                this.Settings = settings;
            });
        }

        public DBLoggerProvider(DBLoggerOptions Settings)
        {
            this.Settings = Settings;
            ThreadProc();
        }

        public override bool IsEnabled(LogLevel LogLevel)
        {
            bool Result = LogLevel != LogLevel.None
               && this.Settings.LogLevel != LogLevel.None
               && Convert.ToInt32(LogLevel) >= Convert.ToInt32(this.Settings.LogLevel);

            return Result;
        }




    }
}
