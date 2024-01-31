using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    // note that the following migration was actually for version 5.0.2 (ie. "Tenant.05.00.02.03")
    [Migration("Tenant.05.01.00.03")]
    public class AddProfileAutocomplete : MultiDatabaseMigration
    {
        public AddProfileAutocomplete(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.AddStringColumn("Autocomplete", 50, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
