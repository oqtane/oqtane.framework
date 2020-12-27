using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Services;

namespace Oqtane.Infrastructure.Startup
{
    public class ClientStartupComparer : IComparer<IClientStartup>
    {
        public int Compare(IClientStartup x, IClientStartup y)
        {
            int xOrder = 0, yOrder = 0;

            if (x is IClientOrderedStartup xOrdered)
            {
                xOrder = xOrdered.Order;
            }

            if (y is IClientOrderedStartup yOrdered)
            {
                yOrder = yOrdered.Order;
            }

            return xOrder.CompareTo(yOrder);
        }
    }
}
