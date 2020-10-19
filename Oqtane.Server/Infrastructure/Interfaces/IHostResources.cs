using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    // Obsolete - use the Resources collection as part of IModuleControl or IThemeCOntrol and use the ResourceDeclaration.Global property
    // note - not using the [Obsolete] attribute in this case as we want to avoid compilation warnings in the core framework
    public interface IHostResources
    {
        List<Resource> Resources { get; } // identifies global resources for an application
    }
}

