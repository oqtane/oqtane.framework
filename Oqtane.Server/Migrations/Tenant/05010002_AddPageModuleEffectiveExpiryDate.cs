using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    // note that the following migration was actually for version 5.0.2 (ie. "Tenant.05.00.02.02")
    [Migration("Tenant.05.01.00.02")]
    public class AddPageModuleEffectiveExpiryDate : MultiDatabaseMigration
    {
        public AddPageModuleEffectiveExpiryDate(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);;
            pageModuleEntityBuilder.AddDateTimeColumn("EffectiveDate", true);
            pageModuleEntityBuilder.AddDateTimeColumn("ExpiryDate", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            pageModuleEntityBuilder.DropColumn("EffectiveDate");
            pageModuleEntityBuilder.DropColumn("ExpiryDate");
        }
    }
}
