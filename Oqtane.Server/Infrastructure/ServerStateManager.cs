using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    // singleton
    public class ServerStateManager
    {
        private List<ServerState> _serverStates { get; set; }

        public ServerStateManager()
        {
            _serverStates = new List<ServerState>();
        }

        public ServerState GetServerState(int siteId)
        {
            var serverState = _serverStates.FirstOrDefault(item => item.SiteId == siteId);
            if (serverState == null)
            {
                serverState = new ServerState();
                serverState.SiteId = siteId;
                serverState.Assemblies = new List<string>();
                serverState.Scripts = new List<Resource>();
                return serverState;
            }
            else
            {
                return serverState;
            }
        }

        public void SetServerState(int siteId, ServerState serverState)
        {
            var serverstate = _serverStates.FirstOrDefault(item => item.SiteId == siteId);
            if (serverstate == null)
            {
                serverState.SiteId = siteId;
                _serverStates.Add(serverState);
            }
            else
            {
                serverstate.Assemblies = serverState.Assemblies;
                serverstate.Scripts = serverState.Scripts;
            }
        }
    }
}
