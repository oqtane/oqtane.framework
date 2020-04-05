using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext
    {
        private IHttpContextAccessor _accessor;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IHttpContextAccessor accessor) : base(options)
        {
            _accessor = accessor;
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
            if (_accessor.HttpContext != null && _accessor.HttpContext.User.Identity.Name != null)
            {
                username = _accessor.HttpContext.User.Identity.Name;
            }
            DateTime date = DateTime.UtcNow;

            var created = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added);

            foreach (var item in created)
            {
                if (item.Entity is IAuditable)
                {
                    item.CurrentValues[nameof(IAuditable.CreatedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.CreatedOn)] = date;
                }
            }

            var modified = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified || x.State == EntityState.Added);

            foreach (var item in modified)
            {
                if (item.Entity is IAuditable)
                {
                    item.CurrentValues[nameof(IAuditable.ModifiedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.ModifiedOn)] = date;
                }
            }

            return base.SaveChanges();
        }
    }
}
