using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

namespace Oqtane.Migrations.EntityBuilders
{
    public abstract class BaseEntityBuilder<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        private readonly MigrationBuilder _migrationBuilder;

        protected BaseEntityBuilder(MigrationBuilder migrationBuilder)
        {
            _migrationBuilder = migrationBuilder;
            ForeignKeys = new List<ForeignKey<TEntityBuilder>>();
        }

        private void AddKeys(CreateTableBuilder<TEntityBuilder> table)
        {
            table.AddPrimaryKey(PrimaryKey);
            foreach (var foreignKey in ForeignKeys)
            {
                table.AddForeignKey(foreignKey);
            }

        }

        protected abstract TEntityBuilder BuildTable(ColumnsBuilder table);

        protected string EntityTableName { get; init; }

        protected PrimaryKey<TEntityBuilder> PrimaryKey { get; init; }

        protected List<ForeignKey<TEntityBuilder>> ForeignKeys { get; }

        public void AddBooleanColumn(string name)
        {
            _migrationBuilder.AddColumn<bool>(name, EntityTableName);
        }

        public void AddDateTimeColumn(string name, bool nullable = false)
        {
            _migrationBuilder.AddColumn<DateTime>(name, EntityTableName, nullable: nullable);
        }

        /// <summary>
        /// Creates a Migration to add an Index to the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to create</param>
        /// <param name="columnName">The name of the column to add to the index</param>
        /// <param name="isUnique">A flag that determines if the Index should be Unique</param>
        public virtual void AddIndex(string indexName, string columnName, bool isUnique = false)
        {
            _migrationBuilder.CreateIndex(
                name: indexName,
                table: EntityTableName,
                column: columnName,
                unique: isUnique);
        }

        /// <summary>
        /// Creates a Migration to add an Index to the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to create</param>
        /// <param name="columnName">The names of the columns to add to the index</param>
        /// <param name="isUnique">A flag that determines if the Index should be Unique</param>
        public virtual void AddIndex(string indexName, string[] columnNames, bool isUnique = false)
        {
            _migrationBuilder.CreateIndex(
                name: indexName,
                table: EntityTableName,
                columns: columnNames,
                unique: isUnique);
        }

        public void AddStringColumn(string name, int length, bool nullable = false)
        {
            _migrationBuilder.AddColumn<string>(name, EntityTableName, maxLength: length, nullable: nullable);
        }

        public void AlterStringColumn(string name, int length, bool nullable = false)
        {
            _migrationBuilder.AlterColumn<string>(name, EntityTableName, maxLength: length, nullable: nullable);
        }

        /// <summary>
        /// Creates a Migration to Create the Entity (table)
        /// </summary>
        public void Create()
        {
            _migrationBuilder.CreateTable(EntityTableName, BuildTable, null, AddKeys);
        }

        /// <summary>
        /// Creates a Migration to Drop the Entity (table)
        /// </summary>
        public void Drop()
        {
            _migrationBuilder.DropTable(EntityTableName);
        }

        public void DropColumn(string name)
        {
            _migrationBuilder.DropColumn(name, EntityTableName);
        }

        /// <summary>
        /// Creates a Migration to drop an Index from the Entity (table)
        /// </summary>
        /// <param name="indexName">The name of the Index to drop</param>
        public virtual void DropIndex(string indexName)
        {
            _migrationBuilder.DropIndex(indexName, EntityTableName);
        }
    }
}

