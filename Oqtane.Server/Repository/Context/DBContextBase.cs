using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;
using Oqtane.Models;
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Repository
{
    public class DBContextBase :  IdentityUserContext<IdentityUser>
    {
        private readonly IDbConfig _dbConfig;
        private readonly ITenantResolver _tenantResolver;

        public DBContextBase(IDbConfig dbConfig, ITenantResolver tenantResolver)
        {
            _dbConfig = dbConfig;
            _tenantResolver = tenantResolver;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _dbConfig.ConnectionString;

            if (string.IsNullOrEmpty(connectionString) && _tenantResolver != null)
            {
                var tenant = _tenantResolver.GetTenant();
                var configuration = _dbConfig.Configuration;

                if (tenant != null)
                {
                    connectionString = tenant.DBConnectionString
                        .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                }
                else
                {
                    if (!String.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
                    {
                        connectionString = configuration.GetConnectionString("DefaultConnection")
                            .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                    }
                }
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseOqtaneDatabase(connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        public override int SaveChanges()
        {
            DbContextUtils.SaveChanges(this, _dbConfig.Accessor);

            return base.SaveChanges();
        }
    }
}
