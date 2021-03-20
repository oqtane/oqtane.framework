using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class RoleEntityBuilder : AuditableBaseEntityBuilder<RoleEntityBuilder>
    {
        private const string _entityTableName = "Role";
        private readonly PrimaryKey<RoleEntityBuilder> _primaryKey = new("PK_Role", x => x.RoleId);
        private readonly ForeignKey<RoleEntityBuilder> _siteForeignKey = new("FK_Role_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public RoleEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override RoleEntityBuilder BuildTable(ColumnsBuilder table)
        {
            RoleId = table.AddAutoIncrementColumn("RoleId");
            SiteId = table.AddIntegerColumn("SiteId", true);
            Name = table.AddStringColumn("Name", 256);
            Description = table.AddStringColumn("Description", 256);
            IsAutoAssigned = table.AddBooleanColumn("IsAutoAssigned");
            IsSystem = table.AddBooleanColumn("IsSystem");

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
