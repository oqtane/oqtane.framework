using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class AspNetUserLoginsEntityBuilder : BaseEntityBuilder<AspNetUserLoginsEntityBuilder>
    {
        private const string _entityTableName = "AspNetUserLogins";
        private readonly PrimaryKey<AspNetUserLoginsEntityBuilder> _primaryKey = new("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
        private readonly ForeignKey<AspNetUserLoginsEntityBuilder> _foreignKey = new("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", ReferentialAction.Cascade);

        public AspNetUserLoginsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_foreignKey);
        }

        protected override AspNetUserLoginsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            LoginProvider = AddStringColumn(table, "LoginProvider", 128);
            ProviderKey = AddStringColumn(table, "ProviderKey", 450);
            ProviderDisplayName = AddMaxStringColumn(table, "ProviderDisplayName", true);
            UserId = AddStringColumn(table, "UserId", 450);
            return this;
        }

        public OperationBuilder<AddColumnOperation> LoginProvider { get; set; }

        public OperationBuilder<AddColumnOperation> ProviderKey { get; set; }

        public OperationBuilder<AddColumnOperation> ProviderDisplayName { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }
    }
}
