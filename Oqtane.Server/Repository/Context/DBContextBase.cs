using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;
using Oqtane.Interfaces;
using Oqtane.Migrations.Framework;
using Oqtane.Repository.Databases.Interfaces;
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
            Databases = dbConfig.Databases;
            _tenantResolver = tenantResolver;
        }

        public IEnumerable<IOqtaneDatabase> Databases { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsAssembly, MultiDatabaseMigrationsAssembly>();

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
                if (Databases != null)
                {
                    optionsBuilder.UseOqtaneDatabase(Databases.Single(d => d.Name == _databaseType), _connectionString);
                }
                else
                {
                    optionsBuilder.UseOqtaneDatabase(_databaseType, _connectionString);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (Databases != null)
            {
                var database = Databases.Single(d => d.Name == _databaseType);

                database.UpdateIdentityStoreTableNames(builder);
            }

        }

        public override int SaveChanges()
        {
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChanges();
        }
    }
}
