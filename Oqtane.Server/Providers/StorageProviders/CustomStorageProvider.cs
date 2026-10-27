using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class CustomStorageProvider : PublicStorageProvider
    {
        private readonly IFolderRepository _folderRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IWebHostEnvironment _environment;

        public CustomStorageProvider(
            ISettingRepository settingRepository,
            IWebHostEnvironment environment,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ITenantManager tenantManager,
            IStorageProviderFactory folderProviderFactory,
            ILogManager logger) : base(environment, folderRepository, fileRepository, tenantManager, folderProviderFactory, logger)
        {
            _folderRepository = folderRepository;
            _settingRepository = settingRepository;
            _environment = environment;
        }

        public override string DisplayName => "Local Server Custom Folder Storage";

        public override string SettingControlType => "Oqtane.Modules.Admin.Files.CustomStorageProviderSettingsControl, Oqtane.Client";

        protected override string GetFolderPath(Folder folder, string folderPath)
        {
            if (folder != null)
            {
                var settings = _settingRepository.GetSettings(EntityNames.StorageLocation, folder.StorageLocationId);
                if (settings.Any(item => item.SettingName == "FolderPath"))
                {
                    return Utilities.PathCombine(_environment.ContentRootPath, settings.First(item => item.SettingName == "FolderPath").SettingValue);
                }
            }
            return "";
        }

        protected override string GetFolderPath(int siteId, string folderPath)
        {
            return GetFolderPath(_folderRepository.GetFolder(siteId, folderPath), folderPath);
        }
    }
}
