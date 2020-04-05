using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    
    public class InstallationContext : DbContext
    {
        private readonly string _connectionString;

        public InstallationContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_connectionString);

        public virtual DbSet<ApplicationVersion> ApplicationVersion { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
    }
}
