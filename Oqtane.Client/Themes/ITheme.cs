using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Themes
{
    public interface ITheme
    {
        [Obsolete("This property is deprecated, please use ModuleDeinition to define theme metadata instead.")]
        Dictionary<string, string> Properties { get; }

        ModuleDefinition ModuleDefinition { get; }
    }
}
