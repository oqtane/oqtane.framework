using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations.EntityBuilders
{
    public class SearchContentWordsEntityBuilder : BaseEntityBuilder<SearchContentWordsEntityBuilder>
    {
        private const string _entityTableName = "SearchContentWords";
        private readonly PrimaryKey<SearchContentWordsEntityBuilder> _primaryKey = new("PK_SearchContentWords", x => x.WordId);
        private readonly ForeignKey<SearchContentWordsEntityBuilder> _searchContentForeignKey = new("FK_SearchContentWords_SearchContent", x => x.SearchContentId, "SearchContent", "SearchContentId", ReferentialAction.Cascade);
        private readonly ForeignKey<SearchContentWordsEntityBuilder> _wordSourceForeignKey = new("FK_SearchContentWords_WordSource", x => x.WordSourceId, "SearchContentWordSource", "WordSourceId", ReferentialAction.Cascade);

        public SearchContentWordsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;

            ForeignKeys.Add(_searchContentForeignKey);
            ForeignKeys.Add(_wordSourceForeignKey);
        }

        protected override SearchContentWordsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            WordId = AddAutoIncrementColumn(table, "WordId");
            SearchContentId = AddIntegerColumn(table, "SearchContentId");
            WordSourceId = AddIntegerColumn(table, "WordSourceId");
            Count = AddIntegerColumn(table, "Count");

            return this;
        }

        public OperationBuilder<AddColumnOperation> WordId { get; private set; }

        public OperationBuilder<AddColumnOperation> SearchContentId { get; private set; }

        public OperationBuilder<AddColumnOperation> WordSourceId { get; private set; }

        public OperationBuilder<AddColumnOperation> Count { get; private set; }
    }
}
