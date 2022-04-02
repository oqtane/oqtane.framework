using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface ISiteUpgrade
    {
        bool Upgrade(Site site, Alias alias);
    }
}
