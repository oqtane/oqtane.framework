using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.02.01")]

    public class AddSiteGuidToSite : MultiDatabaseMigration
    {
        public AddSiteGuidToSite(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddStringColumn("SiteGuid", 36, true, false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.DropColumn("SiteGuid");
        }

    }
}
