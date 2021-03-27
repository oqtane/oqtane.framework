using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ModuleEntityBuilder : AuditableBaseEntityBuilder<ModuleEntityBuilder>
    {
        private const string _entityTableName = "Module";
        private readonly PrimaryKey<ModuleEntityBuilder> _primaryKey = new("PK_Module", x => x.ModuleId);
        private readonly ForeignKey<ModuleEntityBuilder> _siteForeignKey = new("FK_Module_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public ModuleEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override ModuleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ModuleId = ActiveDatabase.AddAutoIncrementColumn(table,"ModuleId");
            SiteId = table.AddIntegerColumn("SiteId");
            ModuleDefinitionName = table.AddStringColumn("ModuleDefinitionName", 200);
            AllPages = table.AddBooleanColumn("AllPages");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> ModuleId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> ModuleDefinitionName { get; private set; }

        public OperationBuilder<AddColumnOperation> AllPages { get; private set; }
    }
}
