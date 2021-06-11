using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.01.00")]
    public class AddAdditionalIndexesInTenant : MultiDatabaseMigration
    {
        public AddAdditionalIndexesInTenant(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Index on Site
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.AddIndex("IX_Site", new [] {"TenantId", "Name"}, true);

            //Create Index on Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder, ActiveDatabase);
            roleEntityBuilder.AddIndex("IX_Role", new [] {"SiteId", "Name"}, true);

            //Create Index on Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.AddIndex("IX_Profile", new [] {"SiteId", "Name"}, true);

            //Create Index on File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.AddIndex("IX_File", new [] {"FolderId", "Name"}, true);

            //Add Columns to Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.AddStringColumn("FromDisplayName", 50, true);
            notificationEntityBuilder.AddStringColumn("FromEmail", 256, true);
            notificationEntityBuilder.AddStringColumn("ToDisplayName", 50, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Index on Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.DropIndex("IX_Site");

            //Drop Index on Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder, ActiveDatabase);
            roleEntityBuilder.DropIndex("IX_Role");

            //Drop Index on Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.DropIndex("IX_Profile");

            //Drop Index on File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.DropIndex("IX_File");

            //Drop Columns from Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.DropColumn("FromDisplayName");
            notificationEntityBuilder.DropColumn("FromEmail");
            notificationEntityBuilder.DropColumn("ToDisplayName");
        }
    }
}
