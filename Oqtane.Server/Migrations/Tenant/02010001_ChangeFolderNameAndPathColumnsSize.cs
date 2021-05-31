using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.01.00.01")]
    public class ChangeFolderNameAndPathColumnsSize : MultiDatabaseMigration
    {
        public ChangeFolderNameAndPathColumnsSize(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

                folderEntityBuilder.AlterStringColumn("Name", 256);

                // Drop the index is needed because the Path is already associated with IX_Folder
                folderEntityBuilder.DropForeignKey("FK_Folder_Site");
                folderEntityBuilder.DropIndex("IX_Folder");
                folderEntityBuilder.AlterStringColumn("Path", 512);
                folderEntityBuilder.AddIndex("IX_Folder", new[] { "SiteId", "Path" }, true);
                folderEntityBuilder.AddForeignKey("FK_Folder_Site");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

                folderEntityBuilder.AlterStringColumn("Name", 50);

                folderEntityBuilder.DropIndex("IX_Folder");
                folderEntityBuilder.AlterStringColumn("Path", 50);
                folderEntityBuilder.AddIndex("IX_Folder", new[] { "SiteId", "Path" }, true);
            }
        }
    }
}
