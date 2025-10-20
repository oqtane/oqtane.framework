using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUserPasskeysEntityBuilder : BaseEntityBuilder<AspNetUserPasskeysEntityBuilder>
    {
        private const string _entityTableName = "AspNetUserPasskeys";
        private readonly PrimaryKey<AspNetUserPasskeysEntityBuilder> _primaryKey = new("PK_AspNetUserPasskeys", x => x.CredentialId);
        private readonly ForeignKey<AspNetUserPasskeysEntityBuilder> _aspNetUsersForeignKey = new("FK_AspNetUserPasskeys_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", ReferentialAction.Cascade);

        public AspNetUserPasskeysEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_aspNetUsersForeignKey);
        }

        protected override AspNetUserPasskeysEntityBuilder BuildTable(ColumnsBuilder table)
        {
            CredentialId = AddBinaryColumn(table, "CredentialId", 1024);
            UserId = AddStringColumn(table, "UserId", 450);
            Data = AddMaxStringColumn(table, "Data");

            return this;
        }

        public OperationBuilder<AddColumnOperation> CredentialId { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> Data { get; set; }
    }
}
