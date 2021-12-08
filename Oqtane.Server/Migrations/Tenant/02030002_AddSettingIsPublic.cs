using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.03.00.02")]
    public class AddSettingIsPublic : MultiDatabaseMigration
    {
        public AddSettingIsPublic(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.AddBooleanColumn("IsPublic", true);
            settingEntityBuilder.UpdateColumn("IsPublic", "0", "bool", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.DropColumn("IsPublic");
        }
    }
}
