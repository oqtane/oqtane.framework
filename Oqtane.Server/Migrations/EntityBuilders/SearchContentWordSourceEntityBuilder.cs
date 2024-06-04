using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentWordSourceEntityBuilder : BaseEntityBuilder<SearchContentWordSourceEntityBuilder>
    {
        private const string _entityTableName = "SearchContentWordSource";
        private readonly PrimaryKey<SearchContentWordSourceEntityBuilder> _primaryKey = new("PK_SearchContentWordSource", x => x.WordSourceId);

        public SearchContentWordSourceEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SearchContentWordSourceEntityBuilder BuildTable(ColumnsBuilder table)
        {
            WordSourceId = AddAutoIncrementColumn(table, "WordSourceId");
            Word = AddStringColumn(table, "Word", 255);

            return this;
        }

        public OperationBuilder<AddColumnOperation> WordSourceId { get; private set; }

        public OperationBuilder<AddColumnOperation> Word { get; private set; }
    }
}
