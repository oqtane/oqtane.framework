using System.Linq;
using System.Security.Policy;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class PrivateStorageProvider : PublicStorageProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantManager _tenantManager;

        public PrivateStorageProvider(
            IWebHostEnvironment environment,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ITenantManager tenantManager,
            IStorageProviderFactory storageProviderFactory,
            ILogManager logger) : base(environment, folderRepository, fileRepository, tenantManager, storageProviderFactory, logger)
        {
            _environment = environment;
            _tenantManager = tenantManager;
        }

        public override string DisplayName { get; } = "Local Server Private Storage";

        protected override string GetFolderPath(Folder folder, string folderPath)
        {
            return GetFolderPath(folder.SiteId, folderPath);
        }

        protected override string GetFolderPath(int siteId, string folderPath)
        {
            return Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", _tenantManager.GetTenant().TenantId.ToString(), "Sites", siteId.ToString(), folderPath);
        }

    }
}
