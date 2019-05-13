using System;

namespace Oqtane.Themes
{
    public interface ITheme
    {
        string Name { get; }
        string Version { get; }
        string Owner { get; }
        string Url { get; }
        string Contact { get; }
        string License { get; }
        string Dependencies { get; }
    }
}
