using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class RoleEntityBuilder : AuditableBaseEntityBuilder<RoleEntityBuilder>
    {
        private const string _entityTableName = "Role";
        private readonly PrimaryKey<RoleEntityBuilder> _primaryKey = new("PK_Role", x => x.RoleId);
        private readonly ForeignKey<RoleEntityBuilder> _siteForeignKey = new("FK_Role_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public RoleEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override RoleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            RoleId = AddAutoIncrementColumn(table,"RoleId");
            SiteId = AddIntegerColumn(table,"SiteId", true);
            Name = AddStringColumn(table,"Name", 256);
            Description = AddStringColumn(table,"Description", 256);
            IsAutoAssigned = AddBooleanColumn(table,"IsAutoAssigned");
            IsSystem = AddBooleanColumn(table,"IsSystem");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> RoleId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Description { get; set; }

        public OperationBuilder<AddColumnOperation> IsAutoAssigned { get; set; }

        public OperationBuilder<AddColumnOperation> IsSystem { get; set; }
    }
}
