using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class StorageLocationEntityBuilder : AuditableBaseEntityBuilder<StorageLocationEntityBuilder>
    {
        private const string _entityTableName = "StorageLocation";
        private readonly PrimaryKey<StorageLocationEntityBuilder> _primaryKey = new("PK_StorageLocation", x => x.StorageLocationId);
        private readonly ForeignKey<StorageLocationEntityBuilder> _siteForeignKey = new("FK_StorageLocation_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public StorageLocationEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override StorageLocationEntityBuilder BuildTable(ColumnsBuilder table)
        {
            StorageLocationId = AddAutoIncrementColumn(table, "StorageLocationId");
            SiteId = AddIntegerColumn(table, "SiteId", true);
            Name = AddStringColumn(table, "Name", 255);
            Provider = AddStringColumn(table, "Provider", 255);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> StorageLocationId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Provider { get; set; }
    }
}
