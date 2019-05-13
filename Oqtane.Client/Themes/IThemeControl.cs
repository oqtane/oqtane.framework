namespace Oqtane.Themes
{
    public interface IThemeControl
    {
        string Name { get; }
        string Panes { get; } // if a theme has different panes, delimit them with ";"
    }
}
