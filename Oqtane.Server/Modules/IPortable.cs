using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface IPortable
    {
        // You Must Set The "ServerAssemblyName" In Your IModule Interface

        string ExportModule(Module Module);

        void ImportModule(Module Module, string Content, string Version);
    }
}
