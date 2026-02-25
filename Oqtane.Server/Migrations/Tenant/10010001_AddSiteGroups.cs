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
            var siteGroupEntityBuilder = new SiteGroupEntityBuilder(migrationBuilder, ActiveDatabase);
            siteGroupEntityBuilder.Create();

            var siteGroupMemberEntityBuilder = new SiteGroupMemberEntityBuilder(migrationBuilder, ActiveDatabase);
            siteGroupMemberEntityBuilder.Create();
            siteGroupMemberEntityBuilder.AddIndex("IX_SiteGroupMember", new[] { "SiteId", "SiteGroupId" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
