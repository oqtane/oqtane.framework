using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class TenantEntityBuilder : AuditableBaseEntityBuilder<TenantEntityBuilder>
    {
        private const string _entityTableName = "Tenant";
        private readonly PrimaryKey<TenantEntityBuilder> _primaryKey = new("PK_Tenant", x => x.TenantId);

        public TenantEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database): base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override TenantEntityBuilder BuildTable(ColumnsBuilder table)
        {
            TenantId = AddAutoIncrementColumn(table,"TenantId");
            Name = AddStringColumn(table,"Name", 100);
            DBConnectionString = AddStringColumn(table,"DBConnectionString", 1024);
            Version = AddStringColumn(table,"Version", 50, true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> TenantId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get;private set; }

        public OperationBuilder<AddColumnOperation> DBConnectionString { get;  private set;}

        public OperationBuilder<AddColumnOperation> Version { get; private set; }
    }
}
