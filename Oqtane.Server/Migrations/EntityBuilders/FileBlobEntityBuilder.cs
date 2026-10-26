using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class FileBlobEntityBuilder : AuditableBaseEntityBuilder<FileBlobEntityBuilder>
    {
        private const string _entityTableName = "FileBlob";
        private readonly PrimaryKey<FileBlobEntityBuilder> _primaryKey = new("PK_FileBlob", x => new { x.FolderId, x.FileName });
        private readonly ForeignKey<FileBlobEntityBuilder> _folderForeignKey = new("FK_FileBlob_Folder", x => x.FolderId, "Folder", "FolderId", ReferentialAction.Cascade);

        public FileBlobEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_folderForeignKey);
        }

        protected override FileBlobEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FileBlobId = AddAutoIncrementColumn(table, "FileBlobId");
            FolderId = AddIntegerColumn(table, "FolderId", false);
            FileName = AddStringColumn(table, "FileName", 50, false);
            Blob = table.Column<byte[]>(name: ActiveDatabase.RewriteName("Blob"), nullable: true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> FileBlobId { get; set; }

        public OperationBuilder<AddColumnOperation> FolderId { get; set; }

        public OperationBuilder<AddColumnOperation> FileName { get; set; }

        public OperationBuilder<AddColumnOperation> Blob { get; set; }
    }
}
