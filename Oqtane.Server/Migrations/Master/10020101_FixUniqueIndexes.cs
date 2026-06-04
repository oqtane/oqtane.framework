using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.10.02.01.01")]
    public class FixUniqueIndexes : MultiDatabaseMigration
    {
        public FixUniqueIndexes(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            try
            {
                var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
                tenantEntityBuilder.DropIndex("IX_Tenant");
                tenantEntityBuilder.AddIndex("IX_Tenant", "Name", true);

                var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
                moduleDefinitionsEntityBuilder.DropIndex("IX_ModuleDefinition");
                moduleDefinitionsEntityBuilder.AddIndex("IX_ModuleDefinition", "ModuleDefinitionName", true);

                var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
                jobEntityBuilder.DropIndex("IX_Job");
                jobEntityBuilder.AddIndex("IX_Job", "JobType", true);
            }
            catch
            {
                // if duplicate records exist, the unique index will fail to be created, but we don't want to fail the migration
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
