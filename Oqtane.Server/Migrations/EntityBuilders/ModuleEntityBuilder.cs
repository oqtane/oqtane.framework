using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ModuleEntityBuilder : AuditableBaseEntityBuilder<ModuleEntityBuilder>
    {
        private const string _entityTableName = "Module";
        private readonly PrimaryKey<ModuleEntityBuilder> _primaryKey = new("PK_Module", x => x.ModuleId);
        private readonly ForeignKey<ModuleEntityBuilder> _siteForeignKey = new("FK_Module_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public ModuleEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override ModuleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ModuleId = AddAutoIncrementColumn(table,"ModuleId");
            SiteId = AddIntegerColumn(table,"SiteId");
            ModuleDefinitionName = AddStringColumn(table,"ModuleDefinitionName", 200);
            AllPages = AddBooleanColumn(table,"AllPages");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> ModuleId { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> ModuleDefinitionName { get; private set; }

        public OperationBuilder<AddColumnOperation> AllPages { get; private set; }
    }
}
