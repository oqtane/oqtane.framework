using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.02.01.00.00")]
    public class AddIndexesForForeignKeyInMaster : MultiDatabaseMigration
    {
        public AddIndexesForForeignKeyInMaster(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Update JobLog table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder, ActiveDatabase);
            jobLogEntityBuilder.AddIndex("IX_JobLog_JobId", "JobId");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.AddIndex("IX_Alias_TenantId", "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Update JobLog table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder, ActiveDatabase);
            jobLogEntityBuilder.DropIndex("IX_JobLog_JobId");

            //Update Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.DropIndex("IX_Alias_TenantId");
        }
    }
}
