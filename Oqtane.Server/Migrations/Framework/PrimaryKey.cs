using System;
using System.Linq.Expressions;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.Migrations
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
