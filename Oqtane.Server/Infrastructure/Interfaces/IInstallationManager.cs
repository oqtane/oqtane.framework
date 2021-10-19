using System.Threading.Tasks;

namespace Oqtane.Infrastructure
{
    public interface IInstallationManager
    {
        void InstallPackages();
        bool UninstallPackage(string PackageName);
        Task UpgradeFramework();
        void RestartApplication();
    }
}
