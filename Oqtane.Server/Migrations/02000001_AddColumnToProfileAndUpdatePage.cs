using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.00.01")]
    public class AddColumnToProfileAndUpdatePage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder);
            profileEntityBuilder.AddStringColumn("Options", 2000, true);

            ///Update new field
            migrationBuilder.Sql(
                @"
                    UPDATE Profile
                    SET Options = ''
                ");

            //Alter Column in Page table
            if (migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
                pageEntityBuilder.DropIndex("IX_Page");
                pageEntityBuilder.AlterStringColumn("Path", 256);
                pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder);
            profileEntityBuilder.DropColumn("Options");

            //Alter Column in Page table
            if (migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
                pageEntityBuilder.DropIndex("IX_Page");
                pageEntityBuilder.AlterStringColumn("Path", 50);
                pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);
            }
        }
    }
}
