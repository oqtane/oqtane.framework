using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class FileEntityBuilder : DeletableAuditableBaseEntityBuilder<FileEntityBuilder>
    {
        private const string _entityTableName = "File";
        private readonly PrimaryKey<FileEntityBuilder> _primaryKey = new("PK_File", x => x.FileId);
        private readonly ForeignKey<FileEntityBuilder> _folderForeignKey = new("FK_File_Folder", x => x.FolderId, "Folder", "FolderId", ReferentialAction.Cascade);

        public FileEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_folderForeignKey);
        }

        protected override FileEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FileId = ActiveDatabase.AddAutoIncrementColumn(table,"FileId");
            FolderId = table.AddIntegerColumn("FolderId");
            Name = table.AddStringColumn("Name", 50);
            Extension = table.AddStringColumn("Extension", 50);
            Size = table.AddIntegerColumn("Size");
            ImageHeight = table.AddIntegerColumn("ImageHeight");
            ImageWidth = table.AddIntegerColumn("ImageWidth");

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> FileId { get; set; }

        public OperationBuilder<AddColumnOperation> FolderId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Extension { get; set; }

        public OperationBuilder<AddColumnOperation> Size { get; set; }

        public OperationBuilder<AddColumnOperation> ImageHeight { get; set; }

        public OperationBuilder<AddColumnOperation> ImageWidth { get; set; }
    }
}
