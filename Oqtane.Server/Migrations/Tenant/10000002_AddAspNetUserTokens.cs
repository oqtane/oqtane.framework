using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.00.00.02")]
    public class AddAspNetUserTokens : MultiDatabaseMigration
    {
        public AddAspNetUserTokens(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var aspNetUserTokensEntityBuilder = new AspNetUserTokensEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUserTokensEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
