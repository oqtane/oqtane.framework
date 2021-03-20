using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public abstract class AuditableBaseEntityBuilder<TEntityBuilder> : BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        protected AuditableBaseEntityBuilder(MigrationBuilder migrationBuilder) : base (migrationBuilder)
        {
        }

        protected void AddAuditableColumns(ColumnsBuilder table)
        {
            CreatedBy = table.AddStringColumn("CreatedBy", 256);
            CreatedOn = table.AddDateTimeColumn("CreatedOn");
            ModifiedBy = table.AddStringColumn("ModifiedBy", 256);
            ModifiedOn = table.AddDateTimeColumn("ModifiedOn");
        }


        public OperationBuilder<AddColumnOperation> CreatedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedOn { get; private set; }
    }
}
