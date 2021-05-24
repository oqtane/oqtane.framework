using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class UserEntityBuilder : DeletableAuditableBaseEntityBuilder<UserEntityBuilder>
    {
        private const string _entityTableName = "User";
        private readonly PrimaryKey<UserEntityBuilder> _primaryKey = new("PK_User", x => x.UserId);

        public UserEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override UserEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UserId = AddAutoIncrementColumn(table,"UserId");
            Username = AddStringColumn(table,"Username", 256);
            DisplayName = AddStringColumn(table,"DisplayName", 50);
            Email = AddStringColumn(table,"Email", 256);
            PhotoFileId = AddIntegerColumn(table,"PhotoFileId", true);
            LastLoginOn = AddDateTimeColumn(table,"LastLoginOn", true);
            LastIPAddress = AddStringColumn(table,"LastIpAddress", 50);

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
