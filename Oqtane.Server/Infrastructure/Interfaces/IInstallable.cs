using Microsoft.EntityFrameworkCore;
using Oqtane.Enums;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IInstallable
    {
        bool Install(Tenant tenant, string version);

        bool Uninstall(Tenant tenant);
    }
}
