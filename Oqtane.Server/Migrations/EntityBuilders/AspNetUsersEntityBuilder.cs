using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUsersEntityBuilder : BaseEntityBuilder<AspNetUsersEntityBuilder>
    {
        private const string _entityTableName = "AspNetUsers";
        private readonly PrimaryKey<AspNetUsersEntityBuilder> _primaryKey = new("PK_AspNetUsers", x => x.Id);

        public AspNetUsersEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override AspNetUsersEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = table.AddStringColumn("Id", 450);
            UserName = table.AddStringColumn("Username", 256, true);
            NormalizedUserName = table.AddStringColumn("NormalizedUserName", 256, true);
            Email = table.AddStringColumn("Email", 256, true);
            NormalizedEmail = table.AddStringColumn("NormalizedEmail", 256, true);
            EmailConfirmed = table.AddBooleanColumn("EmailConfirmed");
            PasswordHash = table.AddMaxStringColumn("PasswordHash", true);
            SecurityStamp = table.AddMaxStringColumn("SecurityStamp", true);
            ConcurrencyStamp = table.AddMaxStringColumn("ConcurrencyStamp", true);
            PhoneNumber = table.AddMaxStringColumn("PhoneNumber", true);
            PhoneNumberConfirmed = table.AddBooleanColumn("PhoneNumberConfirmed");
            TwoFactorEnabled = table.AddBooleanColumn("TwoFactorEnabled");
            LockoutEnd = table.AddDateTimeOffsetColumn("LockoutEnd", true);
            LockoutEnabled = table.AddBooleanColumn("LockoutEnabled");
            AccessFailedCount = table.AddIntegerColumn("AccessFailedCount");

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
