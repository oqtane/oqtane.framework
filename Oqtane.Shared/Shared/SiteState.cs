using Oqtane.Models;

namespace Oqtane.Shared
{
    // this class is used for passing state between components and services as well as controllers and repositories
    public class SiteState
    {
        public Alias Alias { get; set; }
        public string AntiForgeryToken { get; set; } // for use in client services
        public string RemoteIPAddress { get; set; } // captured in _host as cannot be reliably retrieved on Blazor Server
    }
}
