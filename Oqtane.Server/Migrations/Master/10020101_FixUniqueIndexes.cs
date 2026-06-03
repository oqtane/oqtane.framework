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
            //Update Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.DropIndex("IX_Tenant");
            tenantEntityBuilder.AddIndex("IX_Tenant", "Name", true);

            //Update ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleDefinitionsEntityBuilder.DropIndex("IX_ModuleDefinition");
            moduleDefinitionsEntityBuilder.AddIndex("IX_ModuleDefinition", "ModuleDefinitionName", true);

            //Update Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.DropIndex("IX_Job");
            jobEntityBuilder.AddIndex("IX_Job", "JobType", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
