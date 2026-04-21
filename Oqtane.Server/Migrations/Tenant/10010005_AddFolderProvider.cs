using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.01.00.05")]
    public class AddFolderProvider : MultiDatabaseMigration
    {
        public AddFolderProvider(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderProviderEntityBuilder = new FolderConfigEntityBuilder(migrationBuilder, ActiveDatabase);
            folderProviderEntityBuilder.Create();

            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.AddIntegerColumn("FolderConfigId", false, 0);
            folderEntityBuilder.AddStringColumn("MappedPath", 512, true);
            folderEntityBuilder.AddForeignKey("FK_Folder_FolderConfig", "FolderConfigId", "FolderConfig", "FolderConfigId", ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
