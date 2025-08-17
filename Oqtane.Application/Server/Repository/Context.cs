using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;

namespace Oqtane.Application.Repository
{
    public class Context : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.MyModule> MyModule { get; set; }

        public Context(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.MyModule>().ToTable(ActiveDatabase.RewriteName("MyModule"));
        }
    }
}
