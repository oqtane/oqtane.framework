using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
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
            Schema = null;
        }

        protected IDatabase ActiveDatabase { get; }

        protected abstract TEntityBuilder BuildTable(ColumnsBuilder table);

        protected string EntityTableName { get; init; }

        protected PrimaryKey<TEntityBuilder> PrimaryKey { get; init; }

        protected List<ForeignKey<TEntityBuilder>> ForeignKeys { get; }

        protected string Schema { get; init; }

        private string AddSchema(string name)
        {
            if (string.IsNullOrEmpty(Schema))
            {
                return name;
            }
            else
            {
                return $"{Schema}.{name}";
            }
        }

        private string DelimitName(string name)
        {
            return ActiveDatabase.DelimitName(name);
        }

        private string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }

        private string RewriteValue(string value, string type)
        {
            return ActiveDatabase.RewriteValue(value, type);
        }

        // Column Operations

        protected OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name)
        {
            return  ActiveDatabase.AddAutoIncrementColumn(table, RewriteName(name));
        }

        // boolean
        public void AddBooleanColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<bool>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddBooleanColumn(string name, bool nullable, bool defaultValue)
        {
            _migrationBuilder.AddColumn<bool>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddBooleanColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<bool>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddBooleanColumn(ColumnsBuilder table, string name, bool nullable, bool defaultValue)
        {
            return table.Column<bool>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // datetime
        public void AddDateTimeColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateTime>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddDateTimeColumn(string name, bool nullable, DateTime defaultValue)
        {
            _migrationBuilder.AddColumn<DateTime>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<DateTime>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeColumn(ColumnsBuilder table, string name, bool nullable, DateTime defaultValue)
        {
            return table.Column<DateTime>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // datetimeoffset
        public void AddDateTimeOffsetColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateTimeOffset>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddDateTimeOffsetColumn(string name, bool nullable, DateTimeOffset defaultValue)
        {
            _migrationBuilder.AddColumn<DateTimeOffset>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeOffsetColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<DateTimeOffset>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddDateTimeOffsetColumn(ColumnsBuilder table, string name, bool nullable, DateTimeOffset defaultValue)
        {
            return table.Column<DateTimeOffset>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // dateonly
        public void AddDateOnlyColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateOnly>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddDateOnlyColumn(string name, bool nullable, DateOnly defaultValue)
        {
            _migrationBuilder.AddColumn<DateOnly>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddDateOnlyColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<DateOnly>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddDateOnlyColumn(ColumnsBuilder table, string name, bool nullable, DateOnly defaultValue)
        {
            return table.Column<DateOnly>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // timeonly
        public void AddTimeOnlyColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<TimeOnly>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddTimeOnlyColumn(string name, bool nullable, TimeOnly defaultValue)
        {
            _migrationBuilder.AddColumn<TimeOnly>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddTimeOnlyColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<TimeOnly>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddTimeOnlyColumn(ColumnsBuilder table, string name, bool nullable, TimeOnly defaultValue)
        {
            return table.Column<TimeOnly>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // btye
        public void AddByteColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<byte>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddByteColumn(string name, bool nullable, int defaultValue)
        {
            _migrationBuilder.AddColumn<byte>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddByteColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<byte>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddByteColumn(ColumnsBuilder table, string name, bool nullable, int defaultValue)
        {
            return table.Column<byte>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // integer
        public void AddIntegerColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<int>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddIntegerColumn(string name, bool nullable, int defaultValue)
        {
            _migrationBuilder.AddColumn<int>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddIntegerColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<int>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddIntegerColumn(ColumnsBuilder table, string name, bool nullable, int defaultValue)
        {
            return table.Column<int>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }


        // maxstring
        public void AddMaxStringColumn(string name, bool nullable = false, bool unicode = true)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, unicode: unicode, schema: Schema);
        }

        public void AddMaxStringColumn(string name, bool nullable, bool unicode, string defaultValue)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, unicode: unicode, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddMaxStringColumn(ColumnsBuilder table, string name, bool nullable = false, bool unicode = true)
        {
            return table.Column<string>(name: RewriteName(name), nullable: nullable, unicode: unicode);
        }

        protected OperationBuilder<AddColumnOperation> AddMaxStringColumn(ColumnsBuilder table, string name, bool nullable, bool unicode, string defaultValue)
        {
            return table.Column<string>(name: RewriteName(name), nullable: nullable, unicode: unicode, defaultValue: defaultValue);
        }

        // string
        public void AddStringColumn(string name, int length, bool nullable = false, bool unicode = true)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), maxLength: length, nullable: nullable, unicode: unicode, schema: Schema);
        }

        public void AddStringColumn(string name, int length, bool nullable, bool unicode, string defaultValue)
        {
            _migrationBuilder.AddColumn<string>(RewriteName(name), RewriteName(EntityTableName), maxLength: length, nullable: nullable, unicode: unicode, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddStringColumn(ColumnsBuilder table, string name, int length, bool nullable = false, bool unicode = true)
        {
            return table.Column<string>(name: RewriteName(name), maxLength: length, nullable: nullable, unicode: unicode);
        }

        protected OperationBuilder<AddColumnOperation> AddStringColumn(ColumnsBuilder table, string name, int length, bool nullable, bool unicode, string defaultValue)
        {
            return table.Column<string>(name: RewriteName(name), maxLength: length, nullable: nullable, unicode: unicode, defaultValue: defaultValue);
        }

        // decimal
        public void AddDecimalColumn(string name, int precision, int scale, bool nullable = false)
        {
            _migrationBuilder.AddColumn<decimal>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, precision: precision, scale: scale, schema: Schema);
        }

        public void AddDecimalColumn(string name, int precision, int scale, bool nullable, decimal defaultValue)
        {
            _migrationBuilder.AddColumn<decimal>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, precision: precision, scale: scale, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddDecimalColumn(ColumnsBuilder table, string name, int precision, int scale, bool nullable = false)
        {
            return table.Column<decimal>(name: RewriteName(name), nullable: nullable, precision: precision, scale: scale);
        }

        protected OperationBuilder<AddColumnOperation> AddDecimalColumn(ColumnsBuilder table, string name, int precision, int scale, bool nullable, decimal defaultValue)
        {
            return table.Column<decimal>(name: RewriteName(name), nullable: nullable, precision: precision, scale: scale, defaultValue: defaultValue);
        }

        // guid
        public void AddGuidColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<Guid>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, schema: Schema);
        }

        public void AddGuidColumn(string name, bool nullable, Guid defaultValue)
        {
            _migrationBuilder.AddColumn<Guid>(RewriteName(name), RewriteName(EntityTableName), nullable: nullable, defaultValue: defaultValue, schema: Schema);
        }

        protected OperationBuilder<AddColumnOperation> AddGuidColumn(ColumnsBuilder table, string name, bool nullable = false)
        {
            return table.Column<Guid>(name: RewriteName(name), nullable: nullable);
        }

        protected OperationBuilder<AddColumnOperation> AddGuidColumn(ColumnsBuilder table, string name, bool nullable, Guid defaultValue)
        {
            return table.Column<Guid>(name: RewriteName(name), nullable: nullable, defaultValue: defaultValue);
        }

        // alter string
        public void AlterStringColumn(string name, int length, bool nullable = false, bool unicode = true, string index = "")
        {
            if (index != "")
            {
                // indexes are in the form IndexName:Column1,Column2:Unique
                var elements = index.Split(':');
                index = RewriteName(elements[0]) + ":";
                foreach (var column in elements[1].Split(','))
                {
                    index += RewriteName(column) + ",";
                }
                index = index.Substring(0, index.Length - 1) + ":" + elements[2];
            }
            ActiveDatabase.AlterStringColumn(_migrationBuilder, RewriteName(name), RewriteName(EntityTableName), length, nullable, unicode, index);
        }

        // drop column
        public void DropColumn(string name)
        {
            ActiveDatabase.DropColumn(_migrationBuilder, RewriteName(name), RewriteName(EntityTableName));
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
                unique: isUnique,
                schema: Schema);
        }

        public virtual void AddForeignKey(string foreignKeyName, string columnName, string principalTable, string principalColumn, ReferentialAction onDelete)
        {
            _migrationBuilder.AddForeignKey(
                name: RewriteName(foreignKeyName),
                table: RewriteName(EntityTableName),
                column: RewriteName(columnName),
                principalTable: RewriteName(principalTable),
                principalColumn: RewriteName(principalColumn),
                onDelete: onDelete,
                schema: Schema);
        }

        public virtual void AddForeignKey(string foreignKeyName, string columnName, string principalTable, string principalColumn, string principalSchema, ReferentialAction onDelete)
        {
            _migrationBuilder.AddForeignKey(
                name: RewriteName(foreignKeyName),
                table: RewriteName(EntityTableName),
                column: RewriteName(columnName),
                principalTable: RewriteName(principalTable),
                principalColumn: RewriteName(principalColumn),
                principalSchema: RewriteName(principalSchema),
                onDelete: onDelete,
                schema: Schema);
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
                unique: isUnique,
                schema: Schema);
        }

        /// <summary>
        /// Creates a Migration to drop an Index from the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to drop</param>
        public virtual void DropIndex(string indexName)
        {
            _migrationBuilder.DropIndex(RewriteName(indexName), RewriteName(EntityTableName), schema: Schema);
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
                principalSchema: RewriteName(foreignKey.PrincipalSchema),
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
                    principalSchema: RewriteName(foreignKey.PrincipalSchema),
                    onDelete: foreignKey.OnDeleteAction,
                    schema: Schema);
        }

        public void DropForeignKey(ForeignKey<TEntityBuilder> foreignKey)
        {
            DropForeignKey(RewriteName(foreignKey.Name));
        }

        public void DropForeignKey(string keyName)
        {
            _migrationBuilder.DropForeignKey(RewriteName(keyName), RewriteName(EntityTableName), schema: Schema);
        }


        // Table Operations

        /// <summary>
        /// Creates a Migration to Create the Entity (table)
        /// </summary>
        public void Create()
        {
            _migrationBuilder.CreateTable(RewriteName(EntityTableName), BuildTable, Schema, AddKeys);
        }

        /// <summary>
        /// Creates a Migration to Drop the Entity (table)
        /// </summary>
        public void Drop()
        {
            _migrationBuilder.DropTable(RewriteName(EntityTableName), schema: Schema);
        }


        //Sql Operations

        public void DeleteFromTable(string condition = "")
        {
            var deleteSql = $"DELETE FROM {AddSchema(DelimitName(RewriteName(EntityTableName)))} ";
            if(!string.IsNullOrEmpty(condition))
            {
                // note that condition values must be created using RewriteName(), DelimitName(), RewriteValue() if targeting multiple database platforms 
                deleteSql +=  $"WHERE {condition}";
            }
            _migrationBuilder.Sql(deleteSql);
        }

        public void UpdateColumn(string columnName, string value)
        {
            UpdateColumn(columnName, value, "", "");
        }

        public void UpdateColumn(string columnName, string value, string condition)
        {
            UpdateColumn(columnName, value, "", condition);
        }

        public void UpdateColumn(string columnName, string value, string type, string condition)
        {
            var updateSql = $"UPDATE {AddSchema(DelimitName(RewriteName(EntityTableName)))} SET {DelimitName(RewriteName(columnName))} = {RewriteValue(value, type)} ";
            if (!string.IsNullOrEmpty(condition))
            {
                // note that condition values must be created using RewriteName(), DelimitName(), RewriteValue() if targeting multiple database platforms 
                updateSql += $"WHERE {condition}";
            }
            _migrationBuilder.Sql(updateSql);
        }
    }
}

