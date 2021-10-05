using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.00.01")]
    public class AddSiteRuntime : MultiDatabaseMigration
    {
        public AddSiteRuntime(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddStringColumn("Runtime", 50, true, true);
            siteEntityBuilder.UpdateColumn("Runtime", "'Server'");
            siteEntityBuilder.AddStringColumn("RenderMode", 50, true, true);
            siteEntityBuilder.UpdateColumn("RenderMode", "'ServerPrerendered'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.DropColumn("Runtime");
            siteEntityBuilder.DropColumn("RenderMode");
        }
    }
}
