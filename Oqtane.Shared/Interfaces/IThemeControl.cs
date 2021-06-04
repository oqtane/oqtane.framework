using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Themes
{
    public interface IThemeControl
    {
        /// <summary>
        /// Friendly name for a theme
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Screen shot of a theme - assumed to be in the ThemePath() folder
        /// </summary>
        string Thumbnail { get; }

        /// <summary>
        /// Identifies all panes in a theme ( delimited by "," or ";") - assumed to be a layout if no panes specified
        /// </summary>
        string Panes { get; }

        /// <summary>
        /// Identifies all resources in a theme
        /// </summary>
        List<Resource> Resources { get; }
    }
}
