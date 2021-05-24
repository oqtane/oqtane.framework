using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUserClaimsEntityBuilder : BaseEntityBuilder<AspNetUserClaimsEntityBuilder>
    {
        private const string _entityTableName = "AspNetUserClaims";
        private readonly PrimaryKey<AspNetUserClaimsEntityBuilder> _primaryKey = new("PK_AspNetUserClaims", x => x.Id);
        private readonly ForeignKey<AspNetUserClaimsEntityBuilder> _aspNetUsersForeignKey = new("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", ReferentialAction.Cascade);

        public AspNetUserClaimsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_aspNetUsersForeignKey);
        }

        protected override AspNetUserClaimsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            Id = AddAutoIncrementColumn(table,"Id");
            UserId = AddStringColumn(table,"UserId", 450);
            ClaimType = AddMaxStringColumn(table,"ClaimType", true);
            ClaimValue = AddMaxStringColumn(table,"ClaimValue", true);

            return this;
        }

        public OperationBuilder<AddColumnOperation> Id { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> ClaimType { get; set; }

        public OperationBuilder<AddColumnOperation> ClaimValue { get; set; }
    }
}
