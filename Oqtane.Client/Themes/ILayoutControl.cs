namespace Oqtane.Themes
{
    public interface ILayoutControl
    {
        string Name { get; } // friendly name for a layout
        string Thumbnail { get; } // screen shot of a layout - assumed to be in the ThemePath() folder
        string Panes { get; } // identifies all panes in a theme ( delimited by "," or ";" ) 

    }
}
