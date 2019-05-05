using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class HostContext : DbContext
    {
        public HostContext(DbContextOptions<HostContext> options) : base(options) { }

        public virtual DbSet<Tenant> Tenant { get; set; }
    }
}
