namespace Oqtane.Skins
{
    public interface ISkinControl
    {
        string Name { get; }
        string Panes { get; } // if a skin has different panes, delimit them with ";"
    }
}
