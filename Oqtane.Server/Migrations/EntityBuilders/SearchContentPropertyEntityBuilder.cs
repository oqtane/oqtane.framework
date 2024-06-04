using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentPropertyEntityBuilder : BaseEntityBuilder<SearchContentPropertyEntityBuilder>
    {
        private const string _entityTableName = "SearchContentProperty";
        private readonly PrimaryKey<SearchContentPropertyEntityBuilder> _primaryKey = new("PK_SearchContentProperty", x => x.PropertyId);
        private readonly ForeignKey<SearchContentPropertyEntityBuilder> _searchContentForeignKey = new("FK_SearchContentProperty_SearchContent", x => x.SearchContentId, "SearchContent", "SearchContentId", ReferentialAction.Cascade);

        public SearchContentPropertyEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;

            ForeignKeys.Add(_searchContentForeignKey);
        }

        protected override SearchContentPropertyEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PropertyId = AddAutoIncrementColumn(table, "PropertyId");
            SearchContentId = AddIntegerColumn(table, "SearchContentId");
            Name = AddStringColumn(table, "Name", 50);
            Value = AddStringColumn(table, "Value", 50);

            return this;
        }

        public OperationBuilder<AddColumnOperation> PropertyId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchContentId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Value { get; private set; }
    }
}
