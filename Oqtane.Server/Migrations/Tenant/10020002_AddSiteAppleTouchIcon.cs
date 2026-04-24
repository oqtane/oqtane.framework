using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.02.00.02")]
    public class AddSiteAppleTouchIcon : MultiDatabaseMigration
    {
        public AddSiteAppleTouchIcon(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddIntegerColumn("AppleTouchIconFileId", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
