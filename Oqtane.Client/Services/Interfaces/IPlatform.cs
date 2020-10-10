using Oqtane.UI;

namespace Oqtane.Services
{
    public interface IPlatform
    {
        Runtime Runtime { get; }

        string Version { get; }
    }
}
