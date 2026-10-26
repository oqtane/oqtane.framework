using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.03.00.01")]
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
            folderEntityBuilder.AddIntegerColumn("FolderConfigId", true);
            folderEntityBuilder.AddStringColumn("MappedPath", 512, true);
            folderEntityBuilder.DropColumn("Type");
            folderEntityBuilder.AddForeignKey("FK_Folder_FolderConfig", "FolderConfigId", "FolderConfig", "FolderConfigId", ReferentialAction.NoAction);

            var fileBlobEntityBuilder = new FileBlobEntityBuilder(migrationBuilder, ActiveDatabase);
            fileBlobEntityBuilder.Create();
            fileBlobEntityBuilder.AddIndex("IX_FileBlob", new string[] { "FolderId", "FileName" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
