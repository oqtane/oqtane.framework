using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.04.00.00.03")]
    public class AddProfileValidation : MultiDatabaseMigration
    {
        public AddProfileValidation(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.AddStringColumn("Validation", 200, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
