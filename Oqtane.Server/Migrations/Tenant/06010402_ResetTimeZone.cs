using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.06.01.04.02")]
    public class ResetTimeZone : MultiDatabaseMigration
    {
        public ResetTimeZone(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // resetting value as framework now uses IANA ID consistently for time zones
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.UpdateData("TimeZoneId", "''");

            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.UpdateData("TimeZoneId", "''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
