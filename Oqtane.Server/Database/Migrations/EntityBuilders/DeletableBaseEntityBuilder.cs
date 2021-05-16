using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;

namespace Oqtane.Database.Migrations.EntityBuilders
{
    public abstract class DeletableBaseEntityBuilder<TEntityBuilder> : BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        protected DeletableBaseEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
        }

        protected void AddDeletableColumns(ColumnsBuilder table)
        {
            DeletedBy = AddStringColumn(table,"DeletedBy", 256, true);
            DeletedOn = AddDateTimeColumn(table,"DeletedOn", true);
            IsDeleted = AddBooleanColumn(table,"IsDeleted");
        }

        public OperationBuilder<AddColumnOperation> DeletedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> DeletedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> IsDeleted { get; private set; }
    }
}
