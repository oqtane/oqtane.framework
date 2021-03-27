using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class PermissionEntityBuilder : AuditableBaseEntityBuilder<PermissionEntityBuilder>
    {
        private const string _entityTableName = "Permission";
        private readonly PrimaryKey<PermissionEntityBuilder> _primaryKey = new("PK_Permission", x => x.PermissionId);
        private readonly ForeignKey<PermissionEntityBuilder> _siteForeignKey = new("FK_Permission_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);
        private readonly ForeignKey<PermissionEntityBuilder> _userForeignKey = new("FK_Permission_User", x => x.UserId, "User", "UserId", ReferentialAction.NoAction);
        private readonly ForeignKey<PermissionEntityBuilder> _roleForeignKey = new("FK_Permission_Role", x => x.RoleId, "Role", "RoleId", ReferentialAction.NoAction);

        public PermissionEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
            ForeignKeys.Add(_userForeignKey);
            ForeignKeys.Add(_roleForeignKey);
        }

        protected override PermissionEntityBuilder BuildTable(ColumnsBuilder table)
        {
            PermissionId = ActiveDatabase.AddAutoIncrementColumn(table,"PermissionId");
            SiteId = table.AddIntegerColumn("SiteId");
            EntityName = table.AddStringColumn("EntityName", 50);
            EntityId = table.AddIntegerColumn("EntityId");
            PermissionName = table.AddStringColumn("PermissionName", 50);
            RoleId = table.AddIntegerColumn("RoleId", true);
            UserId = table.AddIntegerColumn("UserId", true);
            IsAuthorized = table.AddBooleanColumn("IsAuthorized");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> PermissionId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> EntityName { get; set; }

        public OperationBuilder<AddColumnOperation> EntityId { get; set; }

        public OperationBuilder<AddColumnOperation> PermissionName { get; set; }

        public OperationBuilder<AddColumnOperation> RoleId { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> IsAuthorized { get; set; }
    }
}
