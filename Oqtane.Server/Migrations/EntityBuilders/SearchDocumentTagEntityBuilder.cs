using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchDocumentTagEntityBuilder : BaseEntityBuilder<SearchDocumentTagEntityBuilder>
    {
        private const string _entityTableName = "SearchDocumentTag";
        private readonly PrimaryKey<SearchDocumentTagEntityBuilder> _primaryKey = new("PK_SearchDocumentTag", x => x.TagId);
        private readonly ForeignKey<SearchDocumentTagEntityBuilder> _searchDocumentForeignKey = new("FK_SearchDocumentTag_SearchDocument", x => x.SearchDocumentId, "SearchDocument", "SearchDocumentId", ReferentialAction.Cascade);

        public SearchDocumentTagEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;

            ForeignKeys.Add(_searchDocumentForeignKey);
        }

        protected override SearchDocumentTagEntityBuilder BuildTable(ColumnsBuilder table)
        {
            TagId = AddAutoIncrementColumn(table, "TagId");
            SearchDocumentId = AddIntegerColumn(table, "SearchDocumentId");
            Tag = AddStringColumn(table, "Tag", 50);

            return this;
        }

        public OperationBuilder<AddColumnOperation> TagId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchDocumentId { get; private set; }

        public OperationBuilder<AddColumnOperation> Tag { get; private set; }
    }
}
