using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;

namespace Oqtane.Repository
{

    public class InstallationContext : DbContext
    {
        private readonly string _connectionString;
        private readonly string _databaseType;

        public InstallationContext(string databaseType, string connectionString)
        {
            _connectionString = connectionString;
            _databaseType = databaseType;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseOqtaneDatabase(_databaseType, _connectionString);

        public virtual DbSet<Alias> Alias { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<ModuleDefinition> ModuleDefinition { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobLog> JobLog { get; set; }


    }
}
