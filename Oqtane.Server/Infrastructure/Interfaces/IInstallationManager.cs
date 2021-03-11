namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages(string folders);
        void UpgradeFramework();
        void RestartApplication();
    }
}
