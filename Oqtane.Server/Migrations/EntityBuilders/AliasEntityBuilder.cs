using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AliasEntityBuilder : AuditableBaseEntityBuilder<AliasEntityBuilder>
    {
        private const string _entityTableName = "Alias";
        private readonly PrimaryKey<AliasEntityBuilder> _primaryKey = new("PK_Alias", x => x.AliasId);
        private readonly ForeignKey<AliasEntityBuilder> _tenantForeignKey = new("FK_Alias_Tenant", x => x.TenantId, "Tenant", "TenantId", ReferentialAction.Cascade);

        public AliasEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_tenantForeignKey);
        }

        protected override AliasEntityBuilder BuildTable(ColumnsBuilder table)
        {
            AliasId = ActiveDatabase.AddAutoIncrementColumn(table,"AliasId");
            Name = table.AddStringColumn("Name", 200);
            TenantId = table.AddIntegerColumn("TenantId");
            SiteId = table.AddIntegerColumn("SiteId");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> AliasId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> TenantId { get; private set; }
    }
}
