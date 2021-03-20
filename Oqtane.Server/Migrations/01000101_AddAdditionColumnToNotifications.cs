using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.01.01")]
    public class AddAdditionColumnToNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder);
            notificationEntityBuilder.AddDateTimeColumn("SendOn", true);

            migrationBuilder.Sql(
                @"
                    UPDATE Notification
                    SET SendOn = CreatedOn
                    WHERE SendOn IS NULL;
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder);
            notificationEntityBuilder.DropColumn("SendOn");
        }
    }
}
