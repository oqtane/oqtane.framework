using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    public interface IHostResources
    {
        List<Resource> Resources { get; } // identifies global resources for an application
    }
}

