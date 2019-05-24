using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using System;

namespace Oqtane.Repository
{
    public class TenantContext : DbContext
    {
        public virtual DbSet<Site> Site { get; set; }
        public virtual DbSet<Page> Page { get; set; }
        public virtual DbSet<PageModule> PageModule { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<User> User { get; set; }

        private readonly Tenant tenant;

        public TenantContext(ITenantResolver TenantResolver)
        {
            tenant = TenantResolver.GetTenant();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(tenant.DBConnectionString
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
            );
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (tenant.DBSchema != "")
            {
                modelBuilder.HasDefaultSchema(tenant.DBSchema);
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}
