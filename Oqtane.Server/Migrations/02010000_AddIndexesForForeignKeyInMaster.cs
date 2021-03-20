using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.02.01.00.00")]
    public class AddIndexesForForeignKeyInMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Update JobLog table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder);
            jobLogEntityBuilder.AddIndex("IX_JobLog_JobId", "JobId");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.AddIndex("IX_Alias_TenantId", "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Update JobLog table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder);
            jobLogEntityBuilder.DropIndex("IX_JobLog_JobId");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.DropIndex("IX_Alias_TenantId");
        }
    }
}
