using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Models;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentEntityBuilder : AuditableBaseEntityBuilder<SearchContentEntityBuilder>
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
            EntityName = AddStringColumn(table, "EntityName", 50);
            EntityId = AddIntegerColumn(table, "EntityId");
            SiteId = AddIntegerColumn(table, "SiteId");
            Title = AddStringColumn(table, "Title", 255);
            Description = AddMaxStringColumn(table, "Description");
            Body = AddMaxStringColumn(table, "Body");
            Url = AddStringColumn(table, "Url", 255);
            ModifiedTime = AddDateTimeColumn(table, "ModifiedTime");
            IsActive = AddBooleanColumn(table, "IsActive");
            AdditionalContent = AddMaxStringColumn(table, "AdditionalContent");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SearchContentId { get; private set; }

        public OperationBuilder<AddColumnOperation> EntityName { get; private set; }

        public OperationBuilder<AddColumnOperation> EntityId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> Title { get; private set; }

        public OperationBuilder<AddColumnOperation> Description { get; private set; }

        public OperationBuilder<AddColumnOperation> Body { get; private set; }

        public OperationBuilder<AddColumnOperation> Url { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedTime { get; private set; }

        public OperationBuilder<AddColumnOperation> IsActive { get; private set; }

        public OperationBuilder<AddColumnOperation> AdditionalContent { get; private set; }
    }
}
