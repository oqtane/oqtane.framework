using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public interface IDatabaseManager
    {
        Installation IsInstalled();
        Installation Install();
        Installation Install(InstallConfig install);
    }
}
