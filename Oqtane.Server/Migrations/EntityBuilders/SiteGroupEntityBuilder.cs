using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteGroupEntityBuilder : AuditableBaseEntityBuilder<SiteGroupEntityBuilder>
    {
        private const string _entityTableName = "SiteGroup";
        private readonly PrimaryKey<SiteGroupEntityBuilder> _primaryKey = new("PK_SiteGroup", x => x.SiteGroupId);

        public SiteGroupEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SiteGroupEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteGroupId = AddAutoIncrementColumn(table, "SiteGroupId");
            Name = AddStringColumn(table, "Name", 200);
            Type = AddStringColumn(table, "Type", 50);
            PrimarySiteId = AddIntegerColumn(table, "PrimarySiteId");
            Synchronize = AddBooleanColumn(table, "Synchronize");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteGroupId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Type { get; set; }

        public OperationBuilder<AddColumnOperation> PrimarySiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Synchronize { get; set; }
    }
}
