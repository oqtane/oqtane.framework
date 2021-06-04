using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Migrations;
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

        public string ColumnName
        {
            get
            {
                var body = Column.Body.ToString();
                return body.Substring(body.IndexOf(".") + 1);
            }
        }
        public ReferentialAction OnDeleteAction { get; }

        public string PrincipalTable { get; }

        public string PrincipalColumn { get; }


    }
}
