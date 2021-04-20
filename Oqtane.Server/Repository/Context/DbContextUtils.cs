using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public static class DbContextUtils
    {
        public static void SaveChanges(DbContext context, IHttpContextAccessor accessor)
        {
            var changeTracker = context.ChangeTracker;

            changeTracker.DetectChanges();

            string username = "";
            if (accessor.HttpContext != null && accessor.HttpContext.User.Identity.Name != null)
            {
                username = accessor.HttpContext.User.Identity.Name;
            }
            DateTime date = DateTime.UtcNow;

            var created = changeTracker.Entries()
                .Where(x => x.State == EntityState.Added);

            foreach(var item in created)
            {
                if (item.Entity is IAuditable)
                {
                    item.CurrentValues[nameof(IAuditable.CreatedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.CreatedOn)] = date;
                }
            }

            var modified = changeTracker.Entries()
                .Where(x => x.State == EntityState.Modified || x.State == EntityState.Added);

            foreach (var item in modified)
            {
                if (item.Entity is IAuditable)
                {
                    item.CurrentValues[nameof(IAuditable.ModifiedBy)] = username;
                    item.CurrentValues[nameof(IAuditable.ModifiedOn)] = date;
                }

                if (item.Entity is IDeletable && item.State != EntityState.Added)
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
        }
    }
}
