using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.01.00.02")]
    public class DropAppVersionsTableinTenant : MultiDatabaseMigration
    {
        public DropAppVersionsTableinTenant(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Drop AppVersions table
            var appVersionsEntityBuilder = new AppVersionsEntityBuilder(migrationBuilder, ActiveDatabase);
            appVersionsEntityBuilder.Drop();
        }
    }
}
