using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class PrivateFileProvider : PublicFileProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantManager _tenantManager;

        public override string Name => Constants.PrivateFolderProvider;

        public override string DisplayName => "Private File System";

        public PrivateFileProvider(
            IWebHostEnvironment environment,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ITenantManager tenantManager,
            ILogManager logger) : base(environment, folderRepository, fileRepository, tenantManager, logger)
        {
            _environment = environment;
            _tenantManager = tenantManager;
        }

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
