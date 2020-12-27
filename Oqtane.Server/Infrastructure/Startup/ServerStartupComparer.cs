using System.Collections.Generic;

namespace Oqtane.Infrastructure.Startup
{
    internal class ServerStartupComparer : IComparer<IServerStartup>
    {
        public int Compare(IServerStartup x, IServerStartup y)
        {
            int xOrder = 0, yOrder = 0;

            if (x is IServerOrderedStartup xOrdered)
            {
                xOrder = xOrdered.Order;
            }

            if (y is IServerOrderedStartup yOrdered)
            {
                yOrder = yOrdered.Order;
            }

            return xOrder.CompareTo(yOrder);
        }
    }
}
