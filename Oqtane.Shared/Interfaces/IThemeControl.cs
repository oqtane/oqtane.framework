using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Themes
{
    public interface IThemeControl
    {
        string Panes { get; } // identifies all panes in a theme ( delimited by ";" ) - assumed to be a layout if no panes specified
        List<Resource> Resources { get; } // identifies all resources in a theme
    }
}
