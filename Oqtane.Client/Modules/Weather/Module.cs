using Oqtane.Modules;

namespace Oqtane.Client.Modules.Weather
{
    public class Module : IModule
    {
        public string Name { get { return "Weather"; } }
        public string Description { get { return "Displays random weather using a service"; } }
        public string Version { get { return "1.0.0"; } }
        public string Owner { get { return ""; } }
        public string Url { get { return ""; } }
        public string Contact { get { return ""; } }
        public string License { get { return ""; } }
        public string Dependencies { get { return ""; } }
    }
}
