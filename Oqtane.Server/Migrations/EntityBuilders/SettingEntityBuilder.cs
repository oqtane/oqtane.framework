using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SettingEntityBuilder : AuditableBaseEntityBuilder<SettingEntityBuilder>
    {
        private const string _entityTableName = "Setting";
        private readonly PrimaryKey<SettingEntityBuilder> _primaryKey = new("PK_Setting", x => x.SettingId);

        public SettingEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SettingEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SettingId = AddAutoIncrementColumn(table,"SettingId");
            EntityName = AddStringColumn(table,"EntityName", 50);
            EntityId = AddIntegerColumn(table,"EntityId");
            SettingName = AddStringColumn(table,"SettingName", 200);
            SettingValue = AddMaxStringColumn(table,"SettingValue");

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
