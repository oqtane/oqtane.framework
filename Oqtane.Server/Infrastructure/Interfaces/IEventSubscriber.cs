using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IEventSubscriber
    {
        void EntityChanged(SyncEvent syncEvent);
    }
}
