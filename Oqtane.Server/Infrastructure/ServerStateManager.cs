using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    // singleton
    public class ServerStateManager : IServerStateManager
    {
        private List<ServerState> _serverStates { get; set; }

        public ServerStateManager()
        {
            _serverStates = new List<ServerState>();
        }

        public ServerState GetServerState(string siteKey)
        {
            var serverState = _serverStates.FirstOrDefault(item => item.SiteKey == siteKey);
            if (serverState == null)
            {
                serverState = new ServerState();
                serverState.SiteKey = siteKey;
                serverState.Assemblies = new List<string>();
                serverState.Scripts = new List<Resource>();
                serverState.IsInitialized = false;
                _serverStates.Add(serverState);
            }
            return serverState;
        }
    }
}
