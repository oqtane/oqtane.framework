using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class DBContextBase :  IdentityUserContext<IdentityUser>
    {
        private ITenantResolver _tenantResolver;
        private IHttpContextAccessor _accessor;

        public DBContextBase(ITenantResolver tenantResolver, IHttpContextAccessor accessor)
        {
            _tenantResolver = tenantResolver;
            _accessor = accessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenant = _tenantResolver.GetTenant();
            if (tenant != null)
            {
                var connectionString = tenant.DBConnectionString
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                optionsBuilder.UseOqtaneDatabase(connectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

        public override int SaveChanges()
        {
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChanges();
        }
    }
}
