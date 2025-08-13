using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.00.01.00")]
    public class AddSiteHybridEnabled : MultiDatabaseMigration
    {
        public AddSiteHybridEnabled(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddBooleanColumn("HybridEnabled", true);
            siteEntityBuilder.UpdateData("HybridEnabled", false); // default to false
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.DropColumn("HybridEnabled");
        }
    }
}
