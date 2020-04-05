namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages(string folders, bool restart);
        void UpgradeFramework();
        void RestartApplication();
    }
}
