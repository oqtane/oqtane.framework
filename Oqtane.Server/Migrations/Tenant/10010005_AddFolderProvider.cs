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
            folderProviderEntityBuilder.InsertData(new[] { "Name", "Provider", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn" }, new object[] { $"'{Constants.DefaultFolderProvider}'", $"'{Constants.DefaultFolderProvider}'", "''", DateTime.UtcNow, "''", DateTime.UtcNow }, string.Empty);

            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.AddIntegerColumn("FolderConfigId", false, 1);
            folderEntityBuilder.AddStringColumn("MappedPath", 512, true);
            folderEntityBuilder.UpdateData("FolderConfigId", 1);
            folderEntityBuilder.AddForeignKey("FK_Folder_FolderConfig", "FolderConfigId", "FolderConfig", "FolderConfigId", ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}
