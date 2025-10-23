using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUserTokensEntityBuilder : BaseEntityBuilder<AspNetUserTokensEntityBuilder>
    {
        private const string _entityTableName = "AspNetUserTokens";
        private readonly PrimaryKey<AspNetUserTokensEntityBuilder> _primaryKey = new("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
        private readonly ForeignKey<AspNetUserTokensEntityBuilder> _aspNetUsersForeignKey = new("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", ReferentialAction.Cascade);

        public AspNetUserTokensEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_aspNetUsersForeignKey);
        }

        protected override AspNetUserTokensEntityBuilder BuildTable(ColumnsBuilder table)
        {
            UserId = AddStringColumn(table, "UserId", 450);
            LoginProvider = AddStringColumn(table, "LoginProvider", 128);
            Name = AddStringColumn(table, "Name", 128);
            Value = AddMaxStringColumn(table, "Value", true);

            return this;
        }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> LoginProvider { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Value { get; set; }
    }
}
