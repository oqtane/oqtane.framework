using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.03.00.01")]
    public class AddFolderCapacity : MultiDatabaseMigration
    {
        public AddFolderCapacity(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.AddIntegerColumn("Capacity", true);
            folderEntityBuilder.UpdateColumn("Capacity", "0");
            folderEntityBuilder.UpdateColumn("Capacity", Constants.UserFolderCapacity.ToString(), $"{ActiveDatabase.RewriteName("Name")} = 'My Folder'");
            folderEntityBuilder.AddStringColumn("ImageSizes", 512, true, true);

            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.AddStringColumn("Description", 512, true, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.DropColumn("ImageSizes");
            folderEntityBuilder.DropColumn("Capacity");

            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.DropColumn("Description");
        }
    }
}
