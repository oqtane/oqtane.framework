using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.06.01.03.01")]
    public class AddTimeZone : MultiDatabaseMigration
    {
        public AddTimeZone(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddStringColumn("TimeZoneId", 50, true);

            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.AddStringColumn("TimeZoneId", 50, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
