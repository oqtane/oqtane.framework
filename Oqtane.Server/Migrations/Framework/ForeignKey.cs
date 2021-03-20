using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.EntityBuilders;

namespace Oqtane.Migrations
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
