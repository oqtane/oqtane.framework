using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    // note that the following migration was actually for version 5.0.2 (ie. "Tenant.05.00.02.01")
    [Migration("Tenant.05.01.00.01")]
    public class AddPageEffectiveExpiryDate : MultiDatabaseMigration
    {
        public AddPageEffectiveExpiryDate(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);;
            pageEntityBuilder.AddDateTimeColumn("EffectiveDate", true);
            pageEntityBuilder.AddDateTimeColumn("ExpiryDate", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.DropColumn("EffectiveDate");
            pageEntityBuilder.DropColumn("ExpiryDate");
        }
    }
}
