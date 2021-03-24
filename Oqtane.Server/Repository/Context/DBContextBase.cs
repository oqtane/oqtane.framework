using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;
using Oqtane.Shared;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Repository
{
    public class DBContextBase :  IdentityUserContext<IdentityUser>
    {
        private readonly ITenantResolver _tenantResolver;
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;
        private string _connectionString;
        private string _databaseType;

        public DBContextBase(ITenantResolver tenantResolver, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = String.Empty;
            _tenantResolver = tenantResolver;
            _accessor = httpContextAccessor;
        }

        public DBContextBase(IDbConfig dbConfig, ITenantResolver tenantResolver)
        {
            _accessor = dbConfig.Accessor;
            _configuration = dbConfig.Configuration;
            _connectionString = dbConfig.ConnectionString;
            _databaseType = dbConfig.DatabaseType;
            _tenantResolver = tenantResolver;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrEmpty(_connectionString) && _tenantResolver != null)
            {
                var tenant = _tenantResolver.GetTenant();

                if (tenant != null)
                {
                    _connectionString = tenant.DBConnectionString
                        .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                    _databaseType = tenant.DBType;
                }
                else
                {
                    if (!String.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        _connectionString = _configuration.GetConnectionString("DefaultConnection")
                            .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                    }
                    _databaseType = _configuration.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey];
                }
            }

            if (!string.IsNullOrEmpty(_connectionString) && !string.IsNullOrEmpty(_databaseType))
            {
                optionsBuilder.UseOqtaneDatabase(_databaseType, _connectionString);
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
