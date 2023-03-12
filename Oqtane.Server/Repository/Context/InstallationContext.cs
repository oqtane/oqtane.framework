using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using IDatabase = Oqtane.Databases.Interfaces.IDatabase;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Repository
{

    public class InstallationContext : DbContext
    {
        private readonly string _connectionString;
        private readonly IDatabase _database;

        public InstallationContext(IDatabase database, string connectionString)
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
