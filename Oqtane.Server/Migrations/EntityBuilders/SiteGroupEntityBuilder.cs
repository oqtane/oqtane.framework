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
        private readonly ForeignKey<SiteGroupEntityBuilder> _groupForeignKey = new("FK_SiteGroup_SiteGroupDefinition", x => x.SiteGroupDefinitionId, "SiteGroupDefinition", "SiteGroupDefinitionId", ReferentialAction.Cascade);
        private readonly ForeignKey<SiteGroupEntityBuilder> _siteForeignKey = new("FK_SiteGroup_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public SiteGroupEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_groupForeignKey);
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override SiteGroupEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteGroupId = AddAutoIncrementColumn(table, "SiteGroupId");
            SiteGroupDefinitionId = AddIntegerColumn(table, "SiteGroupDefinitionId");
            SiteId = AddIntegerColumn(table, "SiteId");
            SynchronizedOn = AddDateTimeColumn(table, "SynchronizedOn", true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteGroupId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteGroupDefinitionId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> SynchronizedOn { get; set; }
    }
}
