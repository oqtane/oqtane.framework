using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUsersEntityBuilder : BaseEntityBuilder<AspNetUsersEntityBuilder>
    {
        private const string _entityTableName = "AspNetUsers";
        private readonly PrimaryKey<AspNetUsersEntityBuilder> _primaryKey = new("PK_AspNetUsers", x => x.Id);

        public AspNetUsersEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override AspNetUsersEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddStringColumn(table,"Id", 450);
            UserName = AddStringColumn(table,"UserName", 256, true);
            NormalizedUserName = AddStringColumn(table,"NormalizedUserName", 256, true);
            Email = AddStringColumn(table,"Email", 256, true);
            NormalizedEmail = AddStringColumn(table,"NormalizedEmail", 256, true);
            EmailConfirmed = AddBooleanColumn(table,"EmailConfirmed");
            PasswordHash = AddMaxStringColumn(table,"PasswordHash", true);
            SecurityStamp = AddMaxStringColumn(table,"SecurityStamp", true);
            ConcurrencyStamp = AddMaxStringColumn(table,"ConcurrencyStamp", true);
            PhoneNumber = AddMaxStringColumn(table,"PhoneNumber", true);
            PhoneNumberConfirmed = AddBooleanColumn(table,"PhoneNumberConfirmed");
            TwoFactorEnabled = AddBooleanColumn(table,"TwoFactorEnabled");
            LockoutEnd = AddDateTimeOffsetColumn(table,"LockoutEnd", true);
            LockoutEnabled = AddBooleanColumn(table,"LockoutEnabled");
            AccessFailedCount = AddIntegerColumn(table,"AccessFailedCount");

            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }

        public OperationBuilder<AddColumnOperation> UserName { get; set; }

        public OperationBuilder<AddColumnOperation> NormalizedUserName { get; set; }

        public OperationBuilder<AddColumnOperation> Email { get; set; }

        public OperationBuilder<AddColumnOperation> NormalizedEmail { get; set; }

        public OperationBuilder<AddColumnOperation> EmailConfirmed { get; set; }

        public OperationBuilder<AddColumnOperation> PasswordHash { get; set; }

        public OperationBuilder<AddColumnOperation> SecurityStamp { get; set; }

        public OperationBuilder<AddColumnOperation> ConcurrencyStamp { get; set; }

        public OperationBuilder<AddColumnOperation> PhoneNumber { get; set; }

        public OperationBuilder<AddColumnOperation> PhoneNumberConfirmed { get; set; }

        public OperationBuilder<AddColumnOperation> TwoFactorEnabled { get; set; }

        public OperationBuilder<AddColumnOperation> LockoutEnd { get; set; }

        public OperationBuilder<AddColumnOperation> LockoutEnabled { get; set; }

        public OperationBuilder<AddColumnOperation> AccessFailedCount { get; set; }
    }
}
