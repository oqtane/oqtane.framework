using System;

namespace Oqtane.Skins
{
    public interface ISkin
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
