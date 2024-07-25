using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentWordEntityBuilder : BaseEntityBuilder<SearchContentWordEntityBuilder>
    {
        private const string _entityTableName = "SearchContentWord";
        private readonly PrimaryKey<SearchContentWordEntityBuilder> _primaryKey = new("PK_SearchContentWord", x => x.SearchContentWordId);
        private readonly ForeignKey<SearchContentWordEntityBuilder> _foreignKey1 = new("FK_SearchContentWord_SearchContent", x => x.SearchContentId, "SearchContent", "SearchContentId", ReferentialAction.Cascade);
        private readonly ForeignKey<SearchContentWordEntityBuilder> _foreignKey2 = new("FK_SearchContentWord_SearchWord", x => x.SearchWordId, "SearchWord", "SearchWordId", ReferentialAction.Cascade);

        public SearchContentWordEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_foreignKey1);
            ForeignKeys.Add(_foreignKey2);
        }

        protected override SearchContentWordEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SearchContentWordId = AddAutoIncrementColumn(table, "SearchContentWordId");
            SearchContentId = AddIntegerColumn(table, "SearchContentId");
            SearchWordId = AddIntegerColumn(table, "SearchWordId");
            Count = AddIntegerColumn(table, "Count");
            CreatedOn = AddDateTimeColumn(table, "CreatedOn");
            ModifiedOn = AddDateTimeColumn(table, "ModifiedOn");

            return this;
        }

        public OperationBuilder<AddColumnOperation> SearchContentWordId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchContentId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchWordId { get; private set; }

        public OperationBuilder<AddColumnOperation> Count { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedOn { get; private set; }
    }
}
