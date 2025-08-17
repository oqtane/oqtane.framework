using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Infrastructure
{
    // singleton
    public interface IServerStateManager
    {
        ServerState GetServerState(string siteKey);
    }

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
                serverState.IsInitialized = false;
                _serverStates.Add(serverState);
            }
            return serverState;
        }
    }
}
