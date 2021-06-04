using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class PageModuleEntityBuilder : DeletableAuditableBaseEntityBuilder<PageModuleEntityBuilder>
    {
        private const string _entityTableName = "PageModule";
        private readonly PrimaryKey<PageModuleEntityBuilder> _primaryKey = new("PK_PageModule", x => x.PageModuleId);
        private readonly ForeignKey<PageModuleEntityBuilder> _moduleForeignKey = new("FK_PageModule_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.NoAction);
        private readonly ForeignKey<PageModuleEntityBuilder> _pageForeignKey = new("FK_PageModule_Page", x => x.PageId, "Page", "PageId", ReferentialAction.Cascade);

        public PageModuleEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
            ForeignKeys.Add(_pageForeignKey);
        }

        protected override PageModuleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PageModuleId = AddAutoIncrementColumn(table,"PageModuleId");
            PageId = AddIntegerColumn(table,"PageId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Title = AddStringColumn(table,"Title", 200);
            Pane = AddStringColumn(table,"Pane", 50);
            Order = AddIntegerColumn(table,"Order");
            ContainerType = AddStringColumn(table,"ContainerType", 200);

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> PageModuleId { get; private set; }

        public OperationBuilder<AddColumnOperation> PageId { get; private set; }

        public OperationBuilder<AddColumnOperation> ModuleId { get; private set; }

        public OperationBuilder<AddColumnOperation> Title { get; private set; }

        public OperationBuilder<AddColumnOperation> Pane { get; private set; }

        public OperationBuilder<AddColumnOperation> Order { get; private set; }

        public OperationBuilder<AddColumnOperation> ContainerType { get; private set; }
    }
}
