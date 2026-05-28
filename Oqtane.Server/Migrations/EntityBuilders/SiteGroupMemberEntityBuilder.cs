using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteGroupMemberEntityBuilder : AuditableBaseEntityBuilder<SiteGroupMemberEntityBuilder>
    {
        private const string _entityTableName = "SiteGroupMember";
        private readonly PrimaryKey<SiteGroupMemberEntityBuilder> _primaryKey = new("PK_SiteGroupMember", x => x.SiteGroupMemberId);
        private readonly ForeignKey<SiteGroupMemberEntityBuilder> _groupForeignKey = new("FK_SiteGroupMember_SiteGroup", x => x.SiteGroupId, "SiteGroup", "SiteGroupId", ReferentialAction.Cascade);
        private readonly ForeignKey<SiteGroupMemberEntityBuilder> _siteForeignKey = new("FK_SiteGroupMember_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public SiteGroupMemberEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_groupForeignKey);
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override SiteGroupMemberEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteGroupMemberId = AddAutoIncrementColumn(table, "SiteGroupMemberId");
            SiteGroupId = AddIntegerColumn(table, "SiteGroupId");
            SiteId = AddIntegerColumn(table, "SiteId");
            SynchronizedOn = AddDateTimeColumn(table, "SynchronizedOn", true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteGroupMemberId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteGroupId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> SynchronizedOn { get; set; }
    }
}
