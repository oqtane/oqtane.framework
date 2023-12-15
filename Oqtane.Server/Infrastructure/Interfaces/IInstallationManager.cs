using System.Threading.Tasks;

namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages();
        bool UninstallPackage(string PackageName);
        int RegisterAssemblies();
        Task UpgradeFramework();
        void RestartApplication();
    }
}
