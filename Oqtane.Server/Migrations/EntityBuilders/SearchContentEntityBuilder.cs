using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentEntityBuilder : BaseEntityBuilder<SearchContentEntityBuilder>
    {
        private const string _entityTableName = "SearchContent";
        private readonly PrimaryKey<SearchContentEntityBuilder> _primaryKey = new("PK_SearchContent", x => x.SearchContentId);

        public SearchContentEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SearchContentEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SearchContentId = AddAutoIncrementColumn(table, "SearchContentId");
            SiteId = AddIntegerColumn(table, "SiteId");
            EntityName = AddStringColumn(table, "EntityName", 50);
            EntityId = AddStringColumn(table, "EntityId", 50);
            Title = AddStringColumn(table, "Title", 200);
            Description = AddMaxStringColumn(table, "Description");
            Body = AddMaxStringColumn(table, "Body");
            Url = AddStringColumn(table, "Url", 500);
            Permissions = AddStringColumn(table, "Permissions", 100);
            ContentModifiedBy = AddStringColumn(table, "ContentModifiedBy", 256);
            ContentModifiedOn = AddDateTimeColumn(table, "ContentModifiedOn");
            AdditionalContent = AddMaxStringColumn(table, "AdditionalContent");
            CreatedOn = AddDateTimeColumn(table, "CreatedOn");

            return this;
        }

        public OperationBuilder<AddColumnOperation> SearchContentId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> EntityName { get; private set; }

        public OperationBuilder<AddColumnOperation> EntityId { get; private set; }

        public OperationBuilder<AddColumnOperation> Title { get; private set; }

        public OperationBuilder<AddColumnOperation> Description { get; private set; }

        public OperationBuilder<AddColumnOperation> Body { get; private set; }

        public OperationBuilder<AddColumnOperation> Url { get; private set; }

        public OperationBuilder<AddColumnOperation> Permissions { get; private set; }

        public OperationBuilder<AddColumnOperation> ContentModifiedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> ContentModifiedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> AdditionalContent { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }
    }
}
