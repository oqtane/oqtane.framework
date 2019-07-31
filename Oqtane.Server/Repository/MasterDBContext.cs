using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext
    {
        public MasterDBContext(DbContextOptions<MasterDBContext> options) : base(options) { }

        public virtual DbSet<Alias> Alias { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
    }
}
