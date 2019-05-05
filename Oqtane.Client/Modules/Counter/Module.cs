using Oqtane.Modules;

namespace Oqtane.Client.Modules.Counter
{
    public class Module : IModule
    {
        public string Name { get { return "Counter"; } }
        public string Description { get { return "Increments a counter"; } }
        public string Version { get { return "1.0.0"; } }
        public string Owner { get { return ""; } }
        public string Url { get { return ""; } }
        public string Contact { get { return ""; } }
        public string License { get { return ""; } }
        public string Dependencies { get { return ""; } }
    }
}
