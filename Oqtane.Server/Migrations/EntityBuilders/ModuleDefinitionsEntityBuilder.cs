using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ModuleDefinitionsEntityBuilder : AuditableBaseEntityBuilder<ModuleDefinitionsEntityBuilder>
    {
        private const string _entityTableName = "ModuleDefinition";
        private readonly PrimaryKey<ModuleDefinitionsEntityBuilder> _primaryKey = new("PK_ModuleDefinition", x => x.ModuleDefinitionId);

        public ModuleDefinitionsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override ModuleDefinitionsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ModuleDefinitionId = AddAutoIncrementColumn(table,"ModuleDefinitionId");
            ModuleDefinitionName = AddStringColumn(table,"ModuleDefinitionName", 200);
            Name = AddStringColumn(table,"Name", 200, true);
            Description = AddStringColumn(table,"Description", 2000, true);
            Categories = AddStringColumn(table,"Categories", 200, true);
            Version = AddStringColumn(table,"Version", 50, true);

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
