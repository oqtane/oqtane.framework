using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.01.00.02")]
    public class AddCultureCode : MultiDatabaseMigration
    {
        public AddCultureCode(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddStringColumn("CultureCode", 10, true);
            siteEntityBuilder.UpdateData("CultureCode", $"'{Shared.Constants.DefaultCulture}'");

            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.AddStringColumn("CultureCode", 10, true);
            userEntityBuilder.UpdateData("CultureCode", $"'{Shared.Constants.DefaultCulture}'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
