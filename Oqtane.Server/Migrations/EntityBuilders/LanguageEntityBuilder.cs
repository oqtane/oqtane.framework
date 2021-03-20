using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class LanguageEntityBuilder : AuditableBaseEntityBuilder<LanguageEntityBuilder>
    {
        private const string _entityTableName = "Language";
        private readonly PrimaryKey<LanguageEntityBuilder> _primaryKey = new("PK_Language", x => x.LanguageId);
        private readonly ForeignKey<LanguageEntityBuilder> _siteForeignKey = new("FK_Language_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public LanguageEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override LanguageEntityBuilder BuildTable(ColumnsBuilder table)
        {
            LanguageId = table.AddAutoIncrementColumn("LanguageId");
            SiteId = table.AddIntegerColumn("SiteId");
            Name = table.AddStringColumn("Name", 100);
            Code = table.AddStringColumn("Code", 10);
            IsDefault = table.AddBooleanColumn("IsDefault");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> LanguageId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Code { get; set; }

        public OperationBuilder<AddColumnOperation> IsDefault { get; set; }
    }
}
