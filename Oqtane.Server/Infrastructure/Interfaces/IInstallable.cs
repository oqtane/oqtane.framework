namespace Oqtane.Infrastructure
{
    public interface IInstallable
    {
        bool Install(string version);
        bool Uninstall();
    }
}
