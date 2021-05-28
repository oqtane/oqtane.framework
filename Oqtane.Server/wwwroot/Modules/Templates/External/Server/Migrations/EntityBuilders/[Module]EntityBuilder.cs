using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace [Owner].[Module].Migrations.EntityBuilders
{
    public class [Module]EntityBuilder : AuditableBaseEntityBuilder<[Module]EntityBuilder>
    {
        private const string _entityTableName = "[Owner][Module]";
        private readonly PrimaryKey<[Module]EntityBuilder> _primaryKey = new("PK_[Owner][Module]", x => x.[Module]Id);
        private readonly ForeignKey<[Module]EntityBuilder> _moduleForeignKey = new("FK_[Owner][Module]_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public [Module]EntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override [Module]EntityBuilder BuildTable(ColumnsBuilder table)
        {
            [Module]Id = AddAutoIncrementColumn(table,"[Module]Id");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> [Module]Id { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
