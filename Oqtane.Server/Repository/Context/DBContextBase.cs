using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using System;
using System.Linq;

namespace Oqtane.Repository
{
    public class DBContextBase :  IdentityUserContext<IdentityUser> 
    {
        private Tenant tenant;
        private IHttpContextAccessor accessor;

        public DBContextBase(ITenantResolver TenantResolver, IHttpContextAccessor accessor)
        {
            tenant = TenantResolver.GetTenant();
            this.accessor = accessor;
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
            base.OnModelCreating(modelBuilder);

            if (tenant.DBSchema != "")
            {
                modelBuilder.HasDefaultSchema(tenant.DBSchema);
            }
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            string username = "";
            if (accessor.HttpContext != null && accessor.HttpContext.User.Identity.Name != null)
            {
                username = accessor.HttpContext.User.Identity.Name;
            }
            DateTime date = DateTime.UtcNow;

            var created = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added);

            foreach(var item in created)
            {
                if (item.Entity is IAuditable entity)
                {
                    item.CurrentValues[nameof(IAuditable.CreatedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.CreatedOn)] = date;
                }
            }

            var modified = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified || x.State == EntityState.Added);

            foreach (var item in modified)
            {
                if (item.Entity is IAuditable entity)
                {
                    item.CurrentValues[nameof(IAuditable.ModifiedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.ModifiedOn)] = date;
                }

                if (item.Entity is IDeletable deleted && item.State != EntityState.Added)
                {
                    if ((bool)item.CurrentValues[nameof(IDeletable.IsDeleted)]
                        && !item.GetDatabaseValues().GetValue<bool>(nameof(IDeletable.IsDeleted)))
                    {
                        item.CurrentValues[nameof(IDeletable.DeletedBy)] = username;
                        item.CurrentValues[nameof(IDeletable.DeletedOn)] = date;
                    }
                    else if (!(bool)item.CurrentValues[nameof(IDeletable.IsDeleted)]
                        && item.GetDatabaseValues().GetValue<bool>(nameof(IDeletable.IsDeleted)))
                    {
                        item.CurrentValues[nameof(IDeletable.DeletedBy)] = null;
                        item.CurrentValues[nameof(IDeletable.DeletedOn)] = null;
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}
