using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface IModule
    {
        [Obsolete("This properties is deprecated, please use ModuleDeinition to define module metadata instead.")]
        Dictionary<string, string> Properties { get; }

        ModuleDefinition ModuleDefinition { get; }
    }
}
