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
    public class AddStorageProviders : MultiDatabaseMigration
    {
        public AddStorageProviders(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.AddStringColumn("StorageProviderType", 512, true);
            folderEntityBuilder.UpdateData("StorageProviderType", $"'{Constants.PublicStorageProvider}'", $"{DelimitName(RewriteName("Type"))} = 'Public'");
            folderEntityBuilder.UpdateData("StorageProviderType", $"'{Constants.PrivateStorageProvider}'", $"{DelimitName(RewriteName("Type"))} = 'Private'");
            folderEntityBuilder.DropColumn("Type");

            var fileBlobEntityBuilder = new FileBlobEntityBuilder(migrationBuilder, ActiveDatabase);
            fileBlobEntityBuilder.Create();
            fileBlobEntityBuilder.AddIndex("IX_FileBlob", new [] { "FolderId", "FileName" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
