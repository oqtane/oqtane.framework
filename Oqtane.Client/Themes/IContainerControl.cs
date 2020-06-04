namespace Oqtane.Themes
{
    public interface IContainerControl
    {
        string Name { get; } // friendly name for a container
        string Thumbnail { get; } // screen shot of a container - assumed to be in the ThemePath() folder
    }
}
