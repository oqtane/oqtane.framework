using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class FolderEntityBuilder : DeletableAuditableBaseEntityBuilder<FolderEntityBuilder>
    {
        private const string _entityTableName = "Folder";
        private readonly PrimaryKey<FolderEntityBuilder> _primaryKey = new("PK_Folder", x => x.FolderId);
        private readonly ForeignKey<FolderEntityBuilder> _siteForeignKey = new("FK_Folder_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public FolderEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override FolderEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FolderId = AddAutoIncrementColumn(table,"FolderId");
            SiteId = AddIntegerColumn(table,"SiteId");
            ParentId = AddIntegerColumn(table,"ParentId", true);
            Name = AddStringColumn(table,"Name", 50);
            Path = AddStringColumn(table,"Path", 50);
            Order = AddIntegerColumn(table,"Order");
            IsSystem = AddBooleanColumn(table,"IsSystem");

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> FolderId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> ParentId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Path { get; set; }

        public OperationBuilder<AddColumnOperation> Order { get; set; }

        public OperationBuilder<AddColumnOperation> IsSystem { get; set; }
    }
}
