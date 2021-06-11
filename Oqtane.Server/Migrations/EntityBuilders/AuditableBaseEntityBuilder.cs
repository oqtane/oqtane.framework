using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public abstract class AuditableBaseEntityBuilder<TEntityBuilder> : BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        protected AuditableBaseEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base (migrationBuilder, database)
        {
        }

        protected void AddAuditableColumns(ColumnsBuilder table)
        {
            CreatedBy = AddStringColumn(table,"CreatedBy", 256);
            CreatedOn = AddDateTimeColumn(table,"CreatedOn");
            ModifiedBy = AddStringColumn(table,"ModifiedBy", 256);
            ModifiedOn = AddDateTimeColumn(table,"ModifiedOn");
        }


        public OperationBuilder<AddColumnOperation> CreatedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> CreatedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> ModifiedOn { get; private set; }
    }
}
