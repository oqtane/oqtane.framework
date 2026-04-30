using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class EventDistributorHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISyncManager _syncManager;
        private readonly ICacheManager _cache;
        private readonly ILogger<EventDistributorHostedService> _filelogger;

        public EventDistributorHostedService(IServiceProvider serviceProvider, ISyncManager syncManager, ICacheManager cache, ILogger<EventDistributorHostedService> filelogger)
        {
            _serviceProvider = serviceProvider;
            _syncManager = syncManager;
            _cache = cache;
            _filelogger = filelogger;
        }

        void EntityChanged(object sender, SyncEvent syncEvent)
        {
            List<Type> eventSubscribers = _cache.GetCache("eventsubscribers", entry =>
            {
                eventSubscribers = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes(typeof(IEventSubscriber)))
                    {
                        eventSubscribers.Add(type);
                    }
                }
                return eventSubscribers;
            }, TimeSpan.MaxValue, TimeSpan.MaxValue);

            foreach (var type in eventSubscribers)
            {
                try
                {
                    var obj = ActivatorUtilities.CreateInstance(_serviceProvider, type) as IEventSubscriber;
                    if (obj != null)
                    {
                        obj.EntityChanged(syncEvent);
                    }
                }
                catch (Exception ex)
                {
                    _filelogger.LogError(Utilities.LogMessage(this, $"Error In EventSubscriber {type.AssemblyQualifiedName} - {ex.Message}"));
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _syncManager.EntityChanged += EntityChanged;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _syncManager.EntityChanged -= EntityChanged;
        }
    }
}
