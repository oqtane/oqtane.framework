using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchDocumentEntityBuilder : AuditableBaseEntityBuilder<SearchDocumentEntityBuilder>
    {
        private const string _entityTableName = "SearchDocument";
        private readonly PrimaryKey<SearchDocumentEntityBuilder> _primaryKey = new("PK_SearchDocument", x => x.SearchDocumentId);

        public SearchDocumentEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SearchDocumentEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SearchDocumentId = AddAutoIncrementColumn(table, "SearchDocumentId");
            EntryId = AddIntegerColumn(table, "EntryId");
            IndexerName = AddStringColumn(table, "IndexerName", 50);
            SiteId = AddIntegerColumn(table, "SiteId");
            Title = AddStringColumn(table, "Title", 255);
            Description = AddMaxStringColumn(table, "Description");
            Body = AddMaxStringColumn(table, "Body");
            Url = AddStringColumn(table, "Url", 255);
            ModifiedTime = AddDateTimeColumn(table, "ModifiedTime");
            IsActive = AddBooleanColumn(table, "IsActive");
            AdditionalContent = AddMaxStringColumn(table, "AdditionalContent");
            LanguageCode = AddStringColumn(table, "LanguageCode", 20);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SearchDocumentId { get; private set; }

        public OperationBuilder<AddColumnOperation> EntryId { get; private set; }

        public OperationBuilder<AddColumnOperation> IndexerName { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Title { get; private set; }

        public OperationBuilder<AddColumnOperation> Description { get; private set; }

        public OperationBuilder<AddColumnOperation> Body { get; private set; }

        public OperationBuilder<AddColumnOperation> Url { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedTime { get; private set; }

        public OperationBuilder<AddColumnOperation> IsActive { get; private set; }

        public OperationBuilder<AddColumnOperation> AdditionalContent { get; private set; }

        public OperationBuilder<AddColumnOperation> LanguageCode { get; private set; }

    }
}
