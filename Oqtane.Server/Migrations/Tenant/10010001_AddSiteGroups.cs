using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.01.00.01")]
    public class AddSiteGroups : MultiDatabaseMigration
    {
        public AddSiteGroups(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteGroupDefinitionEntityBuilder = new SiteGroupDefinitionEntityBuilder(migrationBuilder, ActiveDatabase);
            siteGroupDefinitionEntityBuilder.Create();

            var siteGroupEntityBuilder = new SiteGroupEntityBuilder(migrationBuilder, ActiveDatabase);
            siteGroupEntityBuilder.Create();
            siteGroupEntityBuilder.AddIndex("IX_SiteGroup", new[] { "SiteId", "SiteGroupDefinitionId" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
