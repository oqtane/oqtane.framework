namespace Oqtane.Infrastructure.Interfaces
{
    public interface IInstallationManager
    {
        void InstallPackages(string folders, bool restart);
        void UpgradeFramework();
        void RestartApplication();
    }
}
