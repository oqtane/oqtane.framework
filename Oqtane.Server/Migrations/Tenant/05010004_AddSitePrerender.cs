using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.01.00.04")]
    public class AddSitePrerender : MultiDatabaseMigration
    {
        public AddSitePrerender(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);

            siteEntityBuilder.UpdateColumn("RenderMode", $"'{RenderModes.Interactive}'");

            siteEntityBuilder.AddBooleanColumn("Prerender", true);
            siteEntityBuilder.UpdateColumn("Prerender", "1", "bool", "");

            siteEntityBuilder.AddBooleanColumn("Hybrid", true);
            siteEntityBuilder.UpdateColumn("Hybrid", "0", "bool", "");
            siteEntityBuilder.DropColumn("HybridEnabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
