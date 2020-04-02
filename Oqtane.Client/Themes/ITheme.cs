using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Themes
{
    public interface ITheme
    {
        Dictionary<string, string> Properties { get; }

        ModuleDefinition ModuleDefinition { get; }
    }
}
