using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Models;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;
using Oqtane.Interfaces;
using Oqtane.Migrations.Framework;
using Oqtane.Repository.Databases.Interfaces;
using Oqtane.Shared;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext, IMultiDatabase
    {
        private readonly IDbConfig _dbConfig;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IDbConfig dbConfig) : base(options)
        {
            _dbConfig = dbConfig;
            Databases = dbConfig.Databases;
        }

        public IEnumerable<IOqtaneDatabase> Databases { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsAssembly, MultiDatabaseMigrationsAssembly>();

            var connectionString = _dbConfig.ConnectionString;
            var configuration = _dbConfig.Configuration;
            var databaseType = _dbConfig.DatabaseType;

            if(string.IsNullOrEmpty(connectionString) && configuration != null)
            {
                if (!String.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
                {
                    connectionString = configuration.GetConnectionString("DefaultConnection")
                        .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                }

                databaseType = configuration.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey];
            }

            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseType))
            {
                if (Databases != null)
                {
                    optionsBuilder.UseOqtaneDatabase(Databases.Single(d => d.TypeName == databaseType), connectionString);
                }
                else
                {
                    optionsBuilder.UseOqtaneDatabase(databaseType, connectionString);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<Alias> Alias { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<ModuleDefinition> ModuleDefinition { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobLog> JobLog { get; set; }

        public override int SaveChanges()
        {
            DbContextUtils.SaveChanges(this, _dbConfig.Accessor);

            return base.SaveChanges();
        }
    }
}
