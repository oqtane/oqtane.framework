using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Migrations.EntityBuilders
{
    public abstract class BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        private readonly MigrationBuilder _migrationBuilder;

        protected BaseEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
        {
            _migrationBuilder = migrationBuilder;
            ActiveDatabase = database;
            ForeignKeys = new List<ForeignKey<TEntityBuilder>>();
        }

        protected IDatabase ActiveDatabase { get; }

        protected abstract TEntityBuilder BuildTable(ColumnsBuilder table);

        protected string EntityTableName { get; init; }

        protected PrimaryKey<TEntityBuilder> PrimaryKey { get; init; }

        protected List<ForeignKey<TEntityBuilder>> ForeignKeys { get; }

        private string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }


        // Column Operations

        protected OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return  ActiveDatabase.AddAutoIncrementColumn(table, RewriteName(name));
        }

        public void AddBooleanColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<bool>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddBooleanColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<bool>(name: RewriteName(name), nullable: nullable);
        }

        public void AddDateTimeColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateTime>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<DateTime>(name: RewriteName(name), nullable: nullable);
        }

        public void AddDateTimeOffsetColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateTimeOffset>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeOffsetColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<DateTimeOffset>(name: RewriteName(name), nullable: nullable);
        }

        public void AddIntegerColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<int>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddIntegerColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<int>(name: RewriteName(name), nullable: nullable);
        }

        public void AddMaxStringColumn(string name, bool nullable = false, bool unicode = true)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, unicode: unicode);
        }

        protected OperationBuilder<AddColumnOperation> AddMaxStringColumn(ColumnsBuilder table, string name, bool nullable = false, bool unicode = true)
        {
            return table.Column<string>(name: RewriteName(name), nullable: nullable, unicode: unicode);
        }

        public void AddStringColumn(string name, int length, bool nullable = false, bool unicode = true)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), maxLength: length, nullable: nullable, unicode: unicode);
        }

        protected OperationBuilder<AddColumnOperation> AddStringColumn(ColumnsBuilder table, string name, int length, bool nullable = false, bool unicode = true)
        {
            return table.Column<string>(name: RewriteName(name), maxLength: length, nullable: nullable, unicode: unicode);
        }

        public void AlterStringColumn(string name, int length, bool nullable = false, bool unicode = true)
        {
            _migrationBuilder.AlterColumn<string>(RewriteName(name), RewriteName(EntityTableName), maxLength: length, nullable: nullable, unicode: unicode);
        }

        public void AddDecimalColumn(string name, int precision, int scale, bool nullable = false)
        {
            _migrationBuilder.AddColumn<decimal>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, precision: precision, scale: scale);
        }

        protected OperationBuilder<AddColumnOperation> AddDecimalColumn(ColumnsBuilder table, string name, int precision, int scale, bool nullable = false)
        {
            return table.Column<decimal>(name: RewriteName(name), nullable: nullable, precision: precision, scale: scale);
        }

        public void DropColumn(string name)
        {
            _migrationBuilder.DropColumn(RewriteName(name), RewriteName(EntityTableName));
        }


        //Index Operations

        /// <summary>
        /// Creates a Migration to add an Index to the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to create</param>
        /// <param name="columnName">The name of the column to add to the index</param>
        /// <param name="isUnique">A flag that determines if the Index should be Unique</param>
        public virtual void AddIndex(string indexName, string columnName, bool isUnique = false)
        {
            _migrationBuilder.CreateIndex(
                name: RewriteName(indexName),
                table: RewriteName(EntityTableName),
                column: RewriteName(columnName),
                unique: isUnique);
        }

        public virtual void AddForeignKey(string foreignKeyName, string columnName, string principalTable, string principalColumn, ReferentialAction onDelete)
        {
            _migrationBuilder.AddForeignKey(
                name: RewriteName(foreignKeyName),
                table: RewriteName(EntityTableName),
                column: RewriteName(columnName),
                principalTable: RewriteName(principalTable),
                principalColumn: RewriteName(principalColumn),
                onDelete: onDelete );
        }

        /// <summary>
        /// Creates a Migration to add an Index to the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to create</param>
        /// <param name="columnNames">The names of the columns to add to the index</param>
        /// <param name="isUnique">A flag that determines if the Index should be Unique</param>
        public virtual void AddIndex(string indexName, string[] columnNames, bool isUnique = false)
        {
            _migrationBuilder.CreateIndex(
                name: RewriteName(indexName),
                table: RewriteName(EntityTableName),
                columns: columnNames.Select(RewriteName).ToArray(),
                unique: isUnique);
        }

        /// <summary>
        /// Creates a Migration to drop an Index from the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to drop</param>
        public virtual void DropIndex(string indexName)
        {
            _migrationBuilder.DropIndex(RewriteName(indexName), RewriteName(EntityTableName));
        }


        // Key Operations

        private void AddKeys(CreateTableBuilder<TEntityBuilder> table)
        {
            AddPrimaryKey(table, PrimaryKey);
            foreach (var foreignKey in ForeignKeys)
            {
                AddForeignKey(table, foreignKey);
            }
        }

        public void AddPrimaryKey(CreateTableBuilder<TEntityBuilder> table, PrimaryKey<TEntityBuilder> primaryKey)
        {
            table.PrimaryKey(RewriteName(primaryKey.Name), primaryKey.Columns);
        }

        public void AddForeignKey(CreateTableBuilder<TEntityBuilder> table, ForeignKey<TEntityBuilder> foreignKey)
        {
            table.ForeignKey(
                name: RewriteName(foreignKey.Name),
                column: foreignKey.Column,
                principalTable: RewriteName(foreignKey.PrincipalTable),
                principalColumn: RewriteName(foreignKey.PrincipalColumn),
                onDelete: foreignKey.OnDeleteAction);
        }

        public void AddForeignKey(string name)
        {
            var foreignKey = ForeignKeys.Single(k => k.Name == name);

            _migrationBuilder.AddForeignKey(
                    name: RewriteName(foreignKey.Name),
                    table: RewriteName(EntityTableName),
                    column: RewriteName(foreignKey.ColumnName),
                    principalTable: RewriteName(foreignKey.PrincipalTable),
                    principalColumn: RewriteName(foreignKey.PrincipalColumn),
                    onDelete: foreignKey.OnDeleteAction);
        }

        public void DropForeignKey(ForeignKey<TEntityBuilder> foreignKey)
        {
            DropForeignKey(RewriteName(foreignKey.Name));
        }

        public void DropForeignKey(string keyName)
        {
            _migrationBuilder.DropForeignKey(RewriteName(keyName), RewriteName(EntityTableName));
        }


        // Table Operations

        /// <summary>
        /// Creates a Migration to Create the Entity (table)
        /// </summary>
        public void Create()
        {
            _migrationBuilder.CreateTable(RewriteName(EntityTableName), BuildTable, null, AddKeys);
        }

        /// <summary>
        /// Creates a Migration to Drop the Entity (table)
        /// </summary>
        public void Drop()
        {
            _migrationBuilder.DropTable(RewriteName(EntityTableName));
        }


        //Sql Operations

        public void DeleteFromTable(string condition = "")
        {
            var deleteSql = $"DELETE FROM {RewriteName(EntityTableName)} ";
            if(!string.IsNullOrEmpty(condition))
            {
                deleteSql +=  $"WHERE {condition}";
            }
            _migrationBuilder.Sql(deleteSql);
        }

        public void UpdateColumn(string columnName, string value, string condition = "")
        {
            var updateSql = $"UPDATE {RewriteName(EntityTableName)} SET {RewriteName(columnName)} = {value} ";
            if (!string.IsNullOrEmpty(condition))
            {
                updateSql += $"WHERE {condition}";
            }
            _migrationBuilder.Sql(updateSql);
        }
    }
}

