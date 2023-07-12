using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.04.00.01.01")]
    public class AddNotificationIsRead : MultiDatabaseMigration
    {

        public AddNotificationIsRead(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.AddBooleanColumn("IsRead", true);
            notificationEntityBuilder.UpdateColumn("IsRead", "1", "bool", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.DropColumn("IsRead");
        }

    }


}
