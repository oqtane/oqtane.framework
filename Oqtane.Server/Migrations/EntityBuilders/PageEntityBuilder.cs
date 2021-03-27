using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class PageEntityBuilder : DeletableAuditableBaseEntityBuilder<PageEntityBuilder>
    {
        private const string _entityTableName = "Page";
        private readonly PrimaryKey<PageEntityBuilder> _primaryKey = new("PK_Page", x => x.PageId);
        private readonly ForeignKey<PageEntityBuilder> _siteForeignKey = new("FK_Page_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public PageEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override PageEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PageId = ActiveDatabase.AddAutoIncrementColumn(table,"PageId");
            SiteId = table.AddIntegerColumn("SiteId");
            if (ActiveDatabase.Name == "SqlServer" || ActiveDatabase.Name == "LocalDB")
            {
                Path = table.AddStringColumn("Path", 50);
            }
            else
            {
                Path = table.AddStringColumn("Path", 256);
            }
            Name = table.AddStringColumn("Name", 50);
            Title = table.AddStringColumn("Title", 200, true);
            ThemeType = table.AddStringColumn("ThemeType", 200, true);
            Icon = table.AddStringColumn("Icon", 50);
            ParentId = table.AddIntegerColumn("ParentId", true);
            Order = table.AddIntegerColumn("Order");
            IsNavigation = table.AddBooleanColumn("IsNavigation");
            Url = table.AddStringColumn("Url", 500, true);
            LayoutType = table.AddStringColumn("LayoutType", 200);
            UserId = table.AddIntegerColumn("UserId", true);
            IsPersonalizable = table.AddBooleanColumn("IsPersonalizable");
            DefaultContainerType = table.AddStringColumn("DefaultContainerType", 200, true);

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> PageId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Path { get; private set; }
        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Title { get; private set; }

        public OperationBuilder<AddColumnOperation> ThemeType { get; private set; }

        public OperationBuilder<AddColumnOperation> Icon { get; private set; }

        public OperationBuilder<AddColumnOperation> ParentId { get; private set; }

        public OperationBuilder<AddColumnOperation> Order { get; private set; }

        public OperationBuilder<AddColumnOperation> IsNavigation { get; private set; }

        public OperationBuilder<AddColumnOperation> Url { get; private set; }

        public OperationBuilder<AddColumnOperation> LayoutType { get; private set; }

        public OperationBuilder<AddColumnOperation> UserId { get; private set; }

        public OperationBuilder<AddColumnOperation> IsPersonalizable { get; private set; }

        public OperationBuilder<AddColumnOperation> DefaultContainerType { get; private set; }
    }
}
