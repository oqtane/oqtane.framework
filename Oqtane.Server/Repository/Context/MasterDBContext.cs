using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using System;
using System.Linq;

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext
    {
        private IHttpContextAccessor accessor;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IHttpContextAccessor accessor) : base(options)
        {
            this.accessor = accessor;
        }

        public virtual DbSet<Alias> Alias { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<ModuleDefinition> ModuleDefinition { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobLog> JobLog { get; set; }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            string username = "";
            if (accessor.HttpContext != null && accessor.HttpContext.User.Identity.Name != null)
            {
                username = accessor.HttpContext.User.Identity.Name;
            }
            DateTime date = DateTime.Now;

            var created = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added);

            foreach (var item in created)
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
            }

            return base.SaveChanges();
        }
    }
}
