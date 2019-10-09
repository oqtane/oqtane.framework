namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages(string Folders);
        void RestartApplication();
    }
}
