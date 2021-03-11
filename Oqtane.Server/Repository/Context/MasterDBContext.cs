using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Microsoft.Extensions.Configuration;
using Oqtane.Extensions;

namespace Oqtane.Repository
{
    public class MasterDBContext : DbContext
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;

        public MasterDBContext(DbContextOptions<MasterDBContext> options, IHttpContextAccessor accessor, IConfiguration configuration) : base(options)
        {
            _accessor = accessor;
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!String.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")))
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    .Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString());

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
            DbContextUtils.SaveChanges(this, _accessor);

            return base.SaveChanges();
        }
    }
}
