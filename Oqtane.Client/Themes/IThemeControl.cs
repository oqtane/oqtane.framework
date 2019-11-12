namespace Oqtane.Themes
{
    public interface IThemeControl
    {
        string Panes { get; } // identifies all panes in a theme ( delimited by ";" ) - assumed to be a layout if no panes specified
    }
}
