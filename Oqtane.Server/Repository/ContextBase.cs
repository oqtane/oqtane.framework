using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using System;

namespace Oqtane.Repository
{
    public class ContextBase : DbContext
    {
        private Tenant tenant;

        public ContextBase(ITenantResolver TenantResolver)
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
