using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchDocumentPropertyEntityBuilder : BaseEntityBuilder<SearchDocumentPropertyEntityBuilder>
    {
        private const string _entityTableName = "SearchDocumentProperty";
        private readonly PrimaryKey<SearchDocumentPropertyEntityBuilder> _primaryKey = new("PK_SearchDocumentProperty", x => x.PropertyId);
        private readonly ForeignKey<SearchDocumentPropertyEntityBuilder> _searchDocumentForeignKey = new("FK_SearchDocumentProperty_SearchDocument", x => x.SearchDocumentId, "SearchDocument", "SearchDocumentId", ReferentialAction.Cascade);

        public SearchDocumentPropertyEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;

            ForeignKeys.Add(_searchDocumentForeignKey);
        }

        protected override SearchDocumentPropertyEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PropertyId = AddAutoIncrementColumn(table, "PropertyId");
            SearchDocumentId = AddIntegerColumn(table, "SearchDocumentId");
            Name = AddStringColumn(table, "Name", 50);
            Value = AddStringColumn(table, "Value", 50);

            return this;
        }

        public OperationBuilder<AddColumnOperation> PropertyId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchDocumentId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Value { get; private set; }
    }
}
