using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class PageEntityBuilder : DeletableAuditableBaseEntityBuilder<PageEntityBuilder>
    {
        private const string _entityTableName = "Page";
        private readonly PrimaryKey<PageEntityBuilder> _primaryKey = new("PK_Page", x => x.PageId);
        private readonly ForeignKey<PageEntityBuilder> _siteForeignKey = new("FK_Page_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public PageEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override PageEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PageId = AddAutoIncrementColumn(table,"PageId");
            SiteId = AddIntegerColumn(table,"SiteId");
            if (ActiveDatabase.Name == "SqlServer")
            {
                Path = AddStringColumn(table,"Path", 50);
            }
            else
            {
                Path = AddStringColumn(table,"Path", 256);
            }
            Name = AddStringColumn(table,"Name", 50);
            Title = AddStringColumn(table,"Title", 200, true);
            ThemeType = AddStringColumn(table,"ThemeType", 200, true);
            Icon = AddStringColumn(table,"Icon", 50);
            ParentId = AddIntegerColumn(table,"ParentId", true);
            Order = AddIntegerColumn(table,"Order");
            IsNavigation = AddBooleanColumn(table,"IsNavigation");
            Url = AddStringColumn(table,"Url", 500, true);
            UserId = AddIntegerColumn(table,"UserId", true);
            IsPersonalizable = AddBooleanColumn(table,"IsPersonalizable");
            DefaultContainerType = AddStringColumn(table,"DefaultContainerType", 200, true);

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

        public OperationBuilder<AddColumnOperation> UserId { get; private set; }

        public OperationBuilder<AddColumnOperation> IsPersonalizable { get; private set; }

        public OperationBuilder<AddColumnOperation> DefaultContainerType { get; private set; }
    }
}
