using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext
    {
        private readonly IDbConfig _dbConfig;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IDbConfig dbConfig) : base(options)
        {
            _dbConfig = dbConfig;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _dbConfig.ConnectionString;
            var configuration = _dbConfig.Configuration;

            if(string.IsNullOrEmpty(connectionString) && configuration != null)
            {
                if (!String.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
                {
                    connectionString = configuration.GetConnectionString("DefaultConnection")
                        .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                }

            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseOqtaneDatabase(connectionString);
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
