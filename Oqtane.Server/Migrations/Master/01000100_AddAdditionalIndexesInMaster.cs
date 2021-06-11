using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.01.00.01.00")]
    public class AddAdditionalIndexesInMaster : MultiDatabaseMigration
    {
        public AddAdditionalIndexesInMaster(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Update Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.AddIndex("IX_Tenant", "Name");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.AddIndex("IX_Alias", "Name");

            //Update ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleDefinitionsEntityBuilder.AddIndex("IX_ModuleDefinition", "ModuleDefinitionName");

            //Update Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.AddIndex("IX_Job", "JobType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Update Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.DropIndex("IX_Tenant");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.DropIndex("IX_Alias");

            //Update ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleDefinitionsEntityBuilder.DropIndex("IX_ModuleDefinition");

            //Update Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.DropIndex("IX_Job");
        }
    }
}
