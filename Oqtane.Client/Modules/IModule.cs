using System.Collections.Generic;

namespace Oqtane.Modules
{
    public interface IModule
    {
        Dictionary<string, string> Properties { get; }
    }
}
