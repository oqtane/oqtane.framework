using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

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

        public PageModuleEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
            ForeignKeys.Add(_pageForeignKey);
        }

        protected override PageModuleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PageModuleId = ActiveDatabase.AddAutoIncrementColumn(table,"PageModuleId");
            PageId = table.AddIntegerColumn("PageId");
            ModuleId = table.AddIntegerColumn("ModuleId");
            Title = table.AddStringColumn("Title", 200);
            Pane = table.AddStringColumn("Pane", 50);
            Order = table.AddIntegerColumn("Order");
            ContainerType = table.AddStringColumn("ContainerType", 200);

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
