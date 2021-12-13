using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.01.04")]
    public class AddSiteVisitorTracking : MultiDatabaseMigration
    {
        public AddSiteVisitorTracking(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);

            siteEntityBuilder.AddBooleanColumn("VisitorTracking", true);
            siteEntityBuilder.UpdateColumn("VisitorTracking", "1", "bool", "");
            siteEntityBuilder.AddBooleanColumn("CaptureBrokenUrls", true);
            siteEntityBuilder.UpdateColumn("CaptureBrokenUrls", "1", "bool", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);

            siteEntityBuilder.DropColumn("VisitorTracking");
            siteEntityBuilder.DropColumn("CaptureBrokenUrls");
        }
    }
}
