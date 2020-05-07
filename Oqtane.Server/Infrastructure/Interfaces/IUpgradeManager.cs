using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IUpgradeManager
    {
        void Upgrade(Tenant tenant, string version);
    }
}
