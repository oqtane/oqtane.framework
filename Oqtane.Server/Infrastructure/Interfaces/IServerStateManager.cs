namespace Oqtane.Infrastructure
{
    public interface IServerStateManager
    {
        ServerState GetServerState(string siteKey);
     }
}
