using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Database.Migrations.EntityBuilders;

namespace Oqtane.Database.Migrations.Framework
{
    public readonly struct ForeignKey<TEntityBuilder> where TEntityBuilder : BaseEntityBuilder<TEntityBuilder>
    {
        public ForeignKey(string name, Expression<Func<TEntityBuilder, object>> column, string principalTable, string principalColumn, ReferentialAction onDeleteAction)
        {
            Name = name;
            Column = column;
            PrincipalTable = principalTable;
            PrincipalColumn = principalColumn;
            OnDeleteAction = onDeleteAction;
        }

        public string Name { get; }

        public Expression<Func<TEntityBuilder, object>> Column { get;}

        public ReferentialAction OnDeleteAction { get; }

        public string PrincipalTable { get; }

        public string PrincipalColumn { get; }


    }
}
