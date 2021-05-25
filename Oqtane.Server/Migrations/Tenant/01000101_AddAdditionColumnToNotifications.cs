using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.01.01")]
    public class AddAdditionColumnToNotifications : MultiDatabaseMigration
    {
        public AddAdditionColumnToNotifications(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.AddDateTimeColumn("SendOn", true);

            //Update new Column
            notificationEntityBuilder.UpdateColumn("SendOn", $"{ActiveDatabase.RewriteName("CreatedOn")}", $"{ActiveDatabase.RewriteName("SendOn")} IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.DropColumn("SendOn");
        }
    }
}
