using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface ISynchronizable
    {
        // You Must Set The "ServerManagerType" In Your IModule Interface

        string ExtractModule(Module module);

        void LoadModule(Module module, string content, string version);
    }
}
