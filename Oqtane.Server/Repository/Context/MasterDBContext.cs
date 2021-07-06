using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Models;
using Microsoft.Extensions.Configuration;
using Oqtane.Databases.Interfaces;
using Oqtane.Extensions;
using Oqtane.Interfaces;
using Oqtane.Migrations.Framework;
using Oqtane.Repository.Databases.Interfaces;
using Oqtane.Shared;
using System.Threading.Tasks;
using System.Threading;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext, IMultiDatabase
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;
        private string _connectionString;
        private string _databaseType;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IHttpContextAccessor accessor, IConfiguration configuration) : base(options)
        {
            _accessor = accessor;
            _configuration = configuration;
        }

        public IDatabase ActiveDatabase { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsAssembly, MultiDatabaseMigrationsAssembly>();

            if(_configuration != null)
            {
                if (!String.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _connectionString = _configuration.GetConnectionString("DefaultConnection")
                        .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());
                }

                _databaseType = _configuration.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey];
            }

            if (!String.IsNullOrEmpty(_databaseType))
            {
                var type = Type.GetType(_databaseType);
                ActiveDatabase = Activator.CreateInstance(type) as IDatabase;
            }

            if (!string.IsNullOrEmpty(_connectionString) && ActiveDatabase != null)
            {
                optionsBuilder.UseOqtaneDatabase(ActiveDatabase, _connectionString);
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
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
