using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.Migrations.Extensions
{
    public static class CreateTableBuilderExtensions
    {
        public static void AddForeignKey<TEntityBuilder>(this CreateTableBuilder<TEntityBuilder> table, ForeignKey<TEntityBuilder> foreignKey) where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
        {
            table.ForeignKey(
                name: foreignKey.Name,
                column: foreignKey.Column,
                principalTable: foreignKey.PrincipalTable,
                principalColumn: foreignKey.PrincipalColumn,
                onDelete: foreignKey.OnDeleteAction);
        }

        public static void AddPrimaryKey<TEntityBuilder>(this CreateTableBuilder<TEntityBuilder> table, PrimaryKey<TEntityBuilder> primaryKey) where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
        {
            table.PrimaryKey(primaryKey.Name, primaryKey.Columns);
        }
    }
}
