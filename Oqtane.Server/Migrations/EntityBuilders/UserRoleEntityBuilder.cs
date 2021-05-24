using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class UserRoleEntityBuilder : AuditableBaseEntityBuilder<UserRoleEntityBuilder>
    {
        private const string _entityTableName = "UserRole";
        private readonly PrimaryKey<UserRoleEntityBuilder> _primaryKey = new("PK_UserRole", x => x.UserRoleId);
        private readonly ForeignKey<UserRoleEntityBuilder> _userForeignKey = new("FK_UserRole_User", x => x.UserId, "User", "UserId", ReferentialAction.Cascade);
        private readonly ForeignKey<UserRoleEntityBuilder> _roleForeignKey = new("FK_UserRole_Role", x => x.RoleId, "Role", "RoleId", ReferentialAction.NoAction);

        public UserRoleEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_userForeignKey);
            ForeignKeys.Add(_roleForeignKey);
        }

        protected override UserRoleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UserRoleId = AddAutoIncrementColumn(table,"UserRoleId");
            UserId = AddIntegerColumn(table,"UserId");
            RoleId = AddIntegerColumn(table,"RoleId");
            EffectiveDate = AddDateTimeColumn(table,"EffectiveDate", true);
            ExpiryDate = AddDateTimeColumn(table,"ExpiryDate", true);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> UserRoleId { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> RoleId { get; set; }

        public OperationBuilder<AddColumnOperation> EffectiveDate { get; set; }

        public OperationBuilder<AddColumnOperation> ExpiryDate { get; set; }
    }
}
