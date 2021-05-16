using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Database.Migrations.Framework;
using Oqtane.Interfaces;
using Oqtane.Database.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Database.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.01.01")]
    public class AddAdditionColumnToNotifications : MultiDatabaseMigration
    {
        public AddAdditionColumnToNotifications(IOqtaneDatabase database) : base(database)
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
