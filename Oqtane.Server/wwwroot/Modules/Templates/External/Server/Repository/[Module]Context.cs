using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace [Owner].[Module].Repository
{
    public class [Module]Context : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.[Module]> [Module] { get; set; }

        public [Module]Context(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}
