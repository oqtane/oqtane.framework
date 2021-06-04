using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.00.01")]
    public class AddColumnToProfileAndUpdatePage : MultiDatabaseMigration
    {
        public AddColumnToProfileAndUpdatePage(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.AddStringColumn("Options", 2000, true);

            //Update new column
            profileEntityBuilder.UpdateColumn("Options", "''");

            //Alter Column in Page table for Sql Server
            if (ActiveDatabase.Name == "SqlServer")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.DropIndex("IX_Page");
                pageEntityBuilder.AlterStringColumn("Path", 256);
                pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.DropColumn("Options");

            //Alter Column in Page table
            if (ActiveDatabase.Name == "SqlServer")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.DropIndex("IX_Page");
                pageEntityBuilder.AlterStringColumn("Path", 50);
                pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);
            }
        }
    }
}
