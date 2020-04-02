using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface IModule
    {
        Dictionary<string, string> Properties { get; }

        ModuleDefinition ModuleDefinition { get; }
    }
}
