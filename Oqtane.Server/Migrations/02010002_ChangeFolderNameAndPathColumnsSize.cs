using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.01.00.02")]
    public class ChangeFolderNameAndPathColumnsSize : MultiDatabaseMigration
    {
        public ChangeFolderNameAndPathColumnsSize(IOqtaneDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

            folderEntityBuilder.AlterStringColumn("Name", 250);

            // Drop the index is needed because the Path is already associated with IX_Folder
            folderEntityBuilder.DropIndex("IX_Folder");
            folderEntityBuilder.AlterStringColumn("Path", 1000);
            folderEntityBuilder.AddIndex("IX_Folder", new[] { "SiteId", "Path" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

            folderEntityBuilder.AlterStringColumn("Name", 50);

            folderEntityBuilder.DropIndex("IX_Folder");
            folderEntityBuilder.AlterStringColumn("Path", 50);
            folderEntityBuilder.AddIndex("IX_Folder", new[] { "SiteId", "Path" }, true);
        }
    }
}
