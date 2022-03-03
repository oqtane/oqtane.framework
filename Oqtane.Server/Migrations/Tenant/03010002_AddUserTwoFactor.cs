using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.01.00.02")]
    public class AddUserTwoFactor : MultiDatabaseMigration
    {
        public AddUserTwoFactor(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.AddBooleanColumn("TwoFactorRequired", false, false);
            userEntityBuilder.AddStringColumn("TwoFactorCode", 6, true);
            userEntityBuilder.AddDateTimeColumn("TwoFactorExpiry", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.DropColumn("TwoFactorRequired");
            userEntityBuilder.DropColumn("TwoFactorCode");
            userEntityBuilder.DropColumn("TwoFactorExpiry");
        }
    }
}
