using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class UserEntityBuilder : DeletableAuditableBaseEntityBuilder<UserEntityBuilder>
    {
        private const string _entityTableName = "User";
        private readonly PrimaryKey<UserEntityBuilder> _primaryKey = new("PK_User", x => x.UserId);

        public UserEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override UserEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UserId = table.AddAutoIncrementColumn("UserId");
            Username = table.AddStringColumn("Username", 256);
            DisplayName = table.AddStringColumn("DisplayName", 50);
            Email = table.AddStringColumn("Email", 256);
            PhotoFileId = table.AddIntegerColumn("PhotoFileId", true);
            LastLoginOn = table.AddDateTimeColumn("LastLoginOn", true);
            LastIPAddress = table.AddStringColumn("LastIpAddress", 50);

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> UserId { get; private set; }

        public OperationBuilder<AddColumnOperation> Username { get; private set; }

        public OperationBuilder<AddColumnOperation> DisplayName { get; private set; }

        public OperationBuilder<AddColumnOperation> Email { get; private set; }

        public OperationBuilder<AddColumnOperation> PhotoFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> LastLoginOn { get; private set; }

        public OperationBuilder<AddColumnOperation> LastIPAddress { get; private set; }
    }
}
