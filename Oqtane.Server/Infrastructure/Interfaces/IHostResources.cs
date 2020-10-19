using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    // this interface is for declaring global resources and is useful for scenarios where you want to declare resources in a single location for the entire application
    public interface IHostResources
    {
        List<Resource> Resources { get; } // identifies global resources for an application
    }
}

