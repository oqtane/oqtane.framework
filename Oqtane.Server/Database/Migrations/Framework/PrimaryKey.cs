using System;
using System.Linq.Expressions;
using Oqtane.Database.Migrations.EntityBuilders;

namespace Oqtane.Database.Migrations.Framework
{
    public readonly struct PrimaryKey<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        public PrimaryKey(string name, Expression<Func<TEntityBuilder, object>> columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; }

        public Expression<Func<TEntityBuilder, object>> Columns { get;}


    }
}
