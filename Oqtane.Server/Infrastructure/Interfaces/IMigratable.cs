using Oqtane.Enums;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public interface IMigratable
    {
        bool Migrate(Tenant tenant, MigrationType migrationType);
    }
}
