using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Themes
{
    public interface IThemeControl
    {
        string Name { get; } // friendly name for a theme
        string Thumbnail { get; } // screen shot of a theme - assumed to be in the ThemePath() folder
        string Panes { get; } // identifies all panes in a theme ( delimited by "," or ";") - assumed to be a layout if no panes specified
        List<Resource> Resources { get; } // identifies all resources in a theme
    }
}
