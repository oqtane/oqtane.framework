using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteGroupDefinitionEntityBuilder : AuditableBaseEntityBuilder<SiteGroupDefinitionEntityBuilder>
    {
        private const string _entityTableName = "SiteGroupDefinition";
        private readonly PrimaryKey<SiteGroupDefinitionEntityBuilder> _primaryKey = new("PK_SiteGroupDefinition", x => x.SiteGroupDefinitionId);

        public SiteGroupDefinitionEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SiteGroupDefinitionEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteGroupDefinitionId = AddAutoIncrementColumn(table, "SiteGroupDefinitionId");
            Name = AddStringColumn(table, "Name", 200);
            PrimarySiteId = AddIntegerColumn(table, "PrimarySiteId");
            Synchronization = AddBooleanColumn(table, "Synchronization", true);
            Notify = AddBooleanColumn(table, "Notify");
            Synchronize = AddBooleanColumn(table, "Synchronize");
            Localization = AddBooleanColumn(table, "Localization");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteGroupDefinitionId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> PrimarySiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Synchronization { get; set; }

        public OperationBuilder<AddColumnOperation> Notify { get; set; }

        public OperationBuilder<AddColumnOperation> Synchronize { get; set; }

        public OperationBuilder<AddColumnOperation> Localization { get; set; }
    }
}
