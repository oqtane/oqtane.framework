using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ModuleDefinitionsEntityBuilder : AuditableBaseEntityBuilder<ModuleDefinitionsEntityBuilder>
    {
        private const string _entityTableName = "ModuleDefinition";
        private readonly PrimaryKey<ModuleDefinitionsEntityBuilder> _primaryKey = new("PK_ModuleDefinition", x => x.ModuleDefinitionId);

        public ModuleDefinitionsEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override ModuleDefinitionsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ModuleDefinitionId = ActiveDatabase.AddAutoIncrementColumn(table,"ModuleDefinitionId");
            ModuleDefinitionName = table.AddStringColumn("ModuleDefinitionName", 200);
            Name = table.AddStringColumn("Name", 200, true);
            Description = table.AddStringColumn("Description", 2000, true);
            Categories = table.AddStringColumn("Categories", 200, true);
            Version = table.AddStringColumn("Version", 50, true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> ModuleDefinitionId { get; private set; }

        public OperationBuilder<AddColumnOperation> ModuleDefinitionName { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> Description { get; private set; }

        public OperationBuilder<AddColumnOperation> Categories { get; private set; }

        public OperationBuilder<AddColumnOperation> Version { get; private set; }
    }
}
