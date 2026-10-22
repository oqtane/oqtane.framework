using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class TestFolderProvider : PublicFolderProvider
    {
        private readonly IFolderRepository _folderRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IWebHostEnvironment _environment;

        public TestFolderProvider(
            ISettingRepository settingRepository,
            IWebHostEnvironment environment,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ITenantManager tenantManager,
            IFolderProviderFactory folderProviderFactory,
            ILogManager logger) : base(environment, folderRepository, fileRepository, tenantManager, folderProviderFactory, logger)
        {
            _folderRepository = folderRepository;
            _settingRepository = settingRepository;
            _environment = environment;
        }

        protected override string GetFolderPath(Folder folder, string folderPath)
        {
            if (folder != null)
            {
                var settings = _settingRepository.GetSettings(EntityNames.FolderConfig, folder.FolderConfigId);
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
