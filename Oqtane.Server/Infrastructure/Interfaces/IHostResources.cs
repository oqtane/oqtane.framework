using Oqtane.Models;
using System;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    public interface IHostResources
    {
        [Obsolete("IHostResources is deprecated. Use module or theme scoped Resources in conjunction with ResourceLevel.Site instead.", false)]
        List<Resource> Resources { get; } // identifies global resources for an application
    }
}

