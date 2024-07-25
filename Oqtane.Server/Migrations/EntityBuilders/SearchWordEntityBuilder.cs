using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchWordEntityBuilder : BaseEntityBuilder<SearchWordEntityBuilder>
    {
        private const string _entityTableName = "SearchWord";
        private readonly PrimaryKey<SearchWordEntityBuilder> _primaryKey = new("PK_SearchWord", x => x.SearchWordId);

        public SearchWordEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SearchWordEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SearchWordId = AddAutoIncrementColumn(table, "SearchWordId");
            Word = AddStringColumn(table, "Word", 255);
            CreatedOn = AddDateTimeColumn(table, "CreatedOn");

            return this;
        }

        public OperationBuilder<AddColumnOperation> SearchWordId { get; private set; }

        public OperationBuilder<AddColumnOperation> Word { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }
    }
}
