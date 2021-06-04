namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages();
        bool UninstallPackage(string PackageName);
        void UpgradeFramework();
        void RestartApplication();
    }
}
