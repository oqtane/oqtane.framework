using System;
using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface ISynchronizable
    {
        // You Must Set The "ServerManagerType" In Your IModule Interface

        string ExtractModule(Module module, DateTime lastSynchronizedOn);

        void LoadModule(Module module, string content);
    }
}
