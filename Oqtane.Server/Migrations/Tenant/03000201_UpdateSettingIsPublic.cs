using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.02.01")]
    public class UpdateSettingIsPublic : MultiDatabaseMigration
    {
        public UpdateSettingIsPublic(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.UpdateColumn("IsPublic", "1", "bool", $"{RewriteName("SettingName")} NOT LIKE 'SMTP%'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.UpdateColumn("IsPublic", "0", "bool", $"{RewriteName("SettingName")} NOT LIKE 'SMTP%'");
        }
    }
}
