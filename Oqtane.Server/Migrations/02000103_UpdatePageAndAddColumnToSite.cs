using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.03")]
    public class UpdatePageAndAddColumnToSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder);
            siteEntityBuilder.AddStringColumn("AdminContainerType", 200, true);

            //Update new column
            migrationBuilder.Sql(
                @"
                    UPDATE Site
                    SET AdminContainerType = ''
                ");

            //Delete records from Page
            migrationBuilder.Sql(
                @"
                    DELETE FROM [Page]
                    WHERE Path = 'admin/tenants';
                ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder);
            siteEntityBuilder.DropColumn("AdminContainerType");
        }
    }
}
