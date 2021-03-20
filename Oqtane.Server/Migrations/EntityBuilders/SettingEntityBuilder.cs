using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SettingEntityBuilder : AuditableBaseEntityBuilder<SettingEntityBuilder>
    {
        private const string _entityTableName = "Setting";
        private readonly PrimaryKey<SettingEntityBuilder> _primaryKey = new("PK_Setting", x => x.SettingId);

        public SettingEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SettingEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SettingId = table.AddAutoIncrementColumn("SettingId");
            EntityName = table.AddStringColumn("EntityName", 50);
            EntityId = table.AddIntegerColumn("EntityId");
            SettingName = table.AddStringColumn("SettingName", 50);
            SettingValue = table.AddMaxStringColumn("SettingValue");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SettingId { get; set; }

        public OperationBuilder<AddColumnOperation> EntityName { get; set; }

        public OperationBuilder<AddColumnOperation> EntityId { get; set; }

        public OperationBuilder<AddColumnOperation> SettingName { get; set; }

        public OperationBuilder<AddColumnOperation> SettingValue { get; set; }
    }
}
