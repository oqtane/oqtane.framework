using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public interface IDatabaseManager
    {
        bool IsInstalled();
        Installation Install();
        Installation Install(InstallConfig install);
    }
}
