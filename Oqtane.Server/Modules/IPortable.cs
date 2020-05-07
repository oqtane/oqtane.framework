using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface IPortable
    {
        // You Must Set The "ServerAssemblyName" In Your IModule Interface

        string ExportModule(Module module);

        void ImportModule(Module module, string content, string version);
    }
}
