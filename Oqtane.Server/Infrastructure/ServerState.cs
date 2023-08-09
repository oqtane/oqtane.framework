using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class ServerState
    {
        public string SiteKey { get; set; }
        public List<string> Assemblies { get; set; } = new List<string>();
        public List<Resource>Scripts { get; set; } = new List<Resource>();
        public bool IsInitialized { get; set; } = false;
    }
}
