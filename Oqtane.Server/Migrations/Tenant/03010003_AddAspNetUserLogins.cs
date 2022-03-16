using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.01.00.03")]
    public class AddAspNetUserLogins : MultiDatabaseMigration
    {
        public AddAspNetUserLogins(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var aspNetUserLoginsEntityBuilder = new AspNetUserLoginsEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUserLoginsEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var aspNetUserLoginsEntityBuilder = new AspNetUserLoginsEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUserLoginsEntityBuilder.Drop();
        }
    }
}
