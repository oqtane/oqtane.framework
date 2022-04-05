using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.02.02")]
    public class UpdateSettingIsPrivate : MultiDatabaseMigration
    {
        public UpdateSettingIsPrivate(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.AddBooleanColumn("IsPrivate", true);
            settingEntityBuilder.UpdateColumn("IsPrivate", "0", "bool", "");
            settingEntityBuilder.UpdateColumn("IsPrivate", "1", "bool", $"{RewriteName("EntityName")} = 'Site' AND { RewriteName("SettingName")} LIKE 'SMTP%'");
            settingEntityBuilder.DropColumn("IsPublic");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
