using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class TenantEntityBuilder : AuditableBaseEntityBuilder<TenantEntityBuilder>
    {
        private const string _entityTableName = "Tenant";
        private readonly PrimaryKey<TenantEntityBuilder> _primaryKey = new("PK_Tenant", x => x.TenantId);

        public TenantEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database): base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override TenantEntityBuilder BuildTable(ColumnsBuilder table)
        {
            TenantId = ActiveDatabase.AddAutoIncrementColumn(table,"TenantId");
            Name = table.AddStringColumn("Name", 100);
            DBConnectionString = table.AddStringColumn("DBConnectionString", 1024);
            Version = table.AddStringColumn("Version", 50, true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> TenantId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get;private set; }

        public OperationBuilder<AddColumnOperation> DBConnectionString { get;  private set;}

        public OperationBuilder<AddColumnOperation> Version { get; private set; }
    }
}
