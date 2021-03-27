using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Oqtane.Extensions;
using Oqtane.Interfaces;
using Oqtane.Models;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Repository
{

    public class InstallationContext : DbContext
    {
        private readonly string _connectionString;
        private readonly IOqtaneDatabase _database;

        public InstallationContext(IOqtaneDatabase database, string connectionString)
        {
            _connectionString = connectionString;
            _database = database;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseOqtaneDatabase(_database, _connectionString);

        public virtual DbSet<Alias> Alias { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<ModuleDefinition> ModuleDefinition { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobLog> JobLog { get; set; }


    }
}
