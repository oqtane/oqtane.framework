using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.01.00.01.00")]
    public class AddAdditionalIndexesInMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Update Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder);
            tenantEntityBuilder.AddIndex("IX_Tenant", "Name");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.AddIndex("IX_Alias", "Name");

            //Update ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder);
            moduleDefinitionsEntityBuilder.AddIndex("IX_ModuleDefinition", "ModuleDefinitionName");

            //Update Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder);
            jobEntityBuilder.AddIndex("IX_Job", "JobType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Update Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder);
            tenantEntityBuilder.DropIndex("IX_Tenant");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.DropIndex("IX_Alias");

            //Update ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder);
            moduleDefinitionsEntityBuilder.DropIndex("IX_ModuleDefinition");

            //Update Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder);
            jobEntityBuilder.DropIndex("IX_Job");
        }
    }
}
