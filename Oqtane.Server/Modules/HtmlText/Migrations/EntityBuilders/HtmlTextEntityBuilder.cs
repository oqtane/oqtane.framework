using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Modules.HtmlText.Migrations.EntityBuilders
{
    public class HtmlTextEntityBuilder : AuditableBaseEntityBuilder<HtmlTextEntityBuilder>
    {
        private const string _entityTableName = "HtmlText";
        private readonly PrimaryKey<HtmlTextEntityBuilder> _primaryKey = new("PK_HtmlText", x => x.HtmlTextId);
        private readonly ForeignKey<HtmlTextEntityBuilder> _moduleForeignKey = new("FK_HtmlText_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public HtmlTextEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override HtmlTextEntityBuilder BuildTable(ColumnsBuilder table)
        {
            HtmlTextId = ActiveDatabase.AddAutoIncrementColumn(table,"HtmlTextId");
            ModuleId = table.AddIntegerColumn("ModuleId");
            Content = table.AddMaxStringColumn("Content");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> HtmlTextId { get; set; }

        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }

        public OperationBuilder<AddColumnOperation> Content { get; set; }
    }
}
