using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public abstract class DeletableBaseEntityBuilder<TEntityBuilder> : BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        protected DeletableBaseEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
        }

        protected void AddDeletableColumns(ColumnsBuilder table)
        {
            DeletedBy = table.AddStringColumn("DeletedBy", 256, true);
            DeletedOn = table.AddDateTimeColumn("DeletedOn", true);
            IsDeleted = table.AddBooleanColumn("IsDeleted");
        }

        public OperationBuilder<AddColumnOperation> DeletedBy { get; private set; }

        public OperationBuilder<AddColumnOperation> DeletedOn { get; private set; }

        public OperationBuilder<AddColumnOperation> IsDeleted { get; private set; }
    }
}
