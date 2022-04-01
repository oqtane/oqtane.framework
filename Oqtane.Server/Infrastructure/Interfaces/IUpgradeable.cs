using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IUpgradeable
    {
        string GetVersions(Alias alias);

        bool Upgrade(Alias alias, string version);
    }
}
